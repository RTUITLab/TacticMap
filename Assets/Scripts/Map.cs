﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(ObjectManipulator))]
[RequireComponent(typeof(BoundsControl))]
[AddComponentMenu("TacticMap/Map")]

public class Map : MonoBehaviourPunCallbacks
{
    private List<InteractableObj> objs = new List<InteractableObj>();
    private int colorID = -1;
    private int modelID = 0;
    private DisplayTypes _displayType = DisplayTypes.Model;
    private Transform transform;
    private bool grabbed = false;

    [Header("Settings")]
    [SerializeField] private ModelsStorage modelsStorage;

    [Header("Dependencies")]
    [SerializeField] private PhotonView photonView;
    [SerializeField] private ObjectManipulator manipulationHandler;
    [SerializeField] private BoundsControl bounding;

    private void Awake()
    {
        transform = gameObject.transform;

        manipulationHandler.OnManipulationStarted.AddListener((data) => Grab());
        manipulationHandler.OnManipulationEnded.AddListener((data) => Release());
        bounding.RotateStarted.AddListener(Grab);
        bounding.RotateStopped.AddListener(Release);
        bounding.ScaleStarted.AddListener(Grab);
        bounding.ScaleStopped.AddListener(Release);
    }

    private void Start()
    {
        StartCoroutine(syncCoroutine());
    }

    void Update()
    {

    }

    public void BtnChangeDisplayType()
    {
        if (_displayType == DisplayTypes.Model)
        {
            _displayType = DisplayTypes.Symbol;
        }
        else
        {
            _displayType = DisplayTypes.Model;
        }

        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].ChangeDisplayType(_displayType);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < objs.Count; ++i)
            {
                photonView.RPC("SyncPos", RpcTarget.Others, i, objs[i].transform.localPosition.x, objs[i].transform.localPosition.y, objs[i].transform.localPosition.z);
                photonView.RPC("SyncRot", RpcTarget.Others, i, objs[i].transform.localRotation.x, objs[i].transform.localRotation.y, objs[i].transform.localRotation.z, objs[i].transform.localRotation.w);
                photonView.RPC("SyncScale", RpcTarget.Others, i, objs[i].transform.localScale.x, objs[i].transform.localScale.y, objs[i].transform.localScale.z);
            }
        }
    }

    private void SetGrabable(Statuses statuse)
    {
        bool grabable = false;
        if (statuse == Statuses.Mine)
        {
            ;
        }
        else if (statuse == Statuses.Nobody)
        {
            grabable = true;
        }

        bounding.enabled = grabable;
        manipulationHandler.enabled = grabable;
    }

    public void SyncCatchedStatus(int id, bool status)   // true я захватил, false - я отпустил
    {
        photonView.RPC("SyncStatus", RpcTarget.Others, id, status, UserName.instance.userName);
    }

    #region Spawn button
    public void SetColor(int ColorID)
    {
        this.colorID = ColorID;
    }

    public void SetModel(int modelID)
    {
        this.modelID = modelID;
    }

    public void Spawn()
    {
        photonView.RPC("OnlineSpawn", RpcTarget.AllBuffered, modelID, colorID);
    }

    #endregion

    private void Grab()
    {
        UpdObjGrabable(false);
        grabbed = true;
    }

    private void Release()
    {
        UpdObjGrabable(true);
        grabbed = false;
    }

    private void UpdObjGrabable(bool status)
    {
        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].SetGrabable(status);
        }
    }

    public void ClearArea()
    {
        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].DestroyObject();
        }
        objs.Clear();

        manipulationHandler.OnManipulationStarted.RemoveAllListeners();
        manipulationHandler.OnManipulationEnded.RemoveAllListeners();
        bounding.RotateStarted.RemoveAllListeners();
        bounding.RotateStopped.RemoveAllListeners();
        bounding.ScaleStarted.RemoveAllListeners();
        bounding.ScaleStopped.RemoveAllListeners();
    }

    [PunRPC]
    private void destroy(int id)
    {
        InteractableObj buff = objs[id];
        objs.RemoveAt(id);

        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].UpdId(i);
        }

        buff.DestroyObject();
    }

    [PunRPC]
    private void OnlineSpawn(int modelID, int materialID)
    {
        GameObject newObj = Instantiate(modelsStorage.GetModels()[modelID], gameObject.transform.position, transform.rotation, gameObject.transform);
        InteractableObj interactableObj = newObj.GetComponent<InteractableObj>();
        interactableObj.OnSpawn(objs.Count, materialID, this, _displayType, !grabbed);
        interactableObj.OnDestroy.AddListener((num) => photonView.RPC("destroy", RpcTarget.AllBuffered, num));
        interactableObj.OnCatchStatusChange.AddListener((status) => SetGrabable(status));
        objs.Add(interactableObj);
    }

    [PunRPC]
    private void SyncPos(int id, float x, float y, float z)
    {
        objs[id].ApplyDirection(x, y, z);
    }

    [PunRPC]
    private void SyncRot(int id, float x, float y, float z, float w)
    {
        objs[id].UpdRotation(x, y, z, w);
    }

    [PunRPC]
    private void SyncScale(int id, float x, float y, float z)
    {
        objs[id].UpdScale(x, y, z);
    }

    [PunRPC]
    private void SyncStatus(int id, bool status, string name)
    {
        objs[id].CatchObj(status, name);
    }

    IEnumerator syncCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < objs.Count; ++i)
            {
                if (objs[i].NeedSyncPosition())
                {
                    photonView.RPC("SyncPos", RpcTarget.Others, i, objs[i].transform.localPosition.x, objs[i].transform.localPosition.y, objs[i].transform.localPosition.z);
                }

                if (objs[i].NeedSyncRotation())
                {
                    photonView.RPC("SyncRot", RpcTarget.Others, i, objs[i].transform.localRotation.x, objs[i].transform.localRotation.y, objs[i].transform.localRotation.z, objs[i].transform.localRotation.w);
                }

                if (objs[i].NeedSyncScale())
                {
                    photonView.RPC("SyncScale", RpcTarget.Others, i, objs[i].transform.localScale.x,
                        objs[i].transform.localScale.y, objs[i].transform.localScale.z);
                }
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
}
