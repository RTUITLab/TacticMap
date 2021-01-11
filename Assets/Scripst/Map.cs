using System.Collections.Generic;
using System.Security.Policy;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(ManipulationHandler))]     //Устарело.
[RequireComponent(typeof(BoundingBox))]
[AddComponentMenu("TacticMap/Map")]

public class Map : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] prefabs;
    private List<InteractableObj> objs = new List<InteractableObj>();
    private PhotonView photonView;
    private DisplayTypes _displayType = DisplayTypes.Model;
    private Transform transform;
    private ObjMaterial material = ObjMaterial.Gray;
    private ManipulationHandler manipulationHandler;
    private BoundingBox bounding;

    private void Awake()
    {
        photonView = gameObject.GetComponent<PhotonView>();
        manipulationHandler = gameObject.GetComponent<ManipulationHandler>();
        bounding = gameObject.GetComponent<BoundingBox>();
        transform = gameObject.transform;

        manipulationHandler.OnManipulationStarted.AddListener((data) => Grab());
        manipulationHandler.OnManipulationEnded.AddListener((data) => Release());
        bounding.RotateStarted.AddListener(Grab);
        bounding.RotateStopped.AddListener(Release);
        bounding.ScaleStarted.AddListener(Grab);
        bounding.ScaleStopped.AddListener(Release);
    }

    void Update()
    {
        for (int i = 0; i < objs.Count; ++i)
        {
            if (objs[i].NeedSyncPosition())
            {
                photonView.RPC("SyncPos", RpcTarget.Others, i, objs[i].transform.localPosition.x, objs[i].transform.localPosition.y, objs[i].transform.localPosition.z);
                objs[i].AfterPositionSync();
            }

            if (objs[i].NeedSyncRotation())
            {
                photonView.RPC("SyncRot", RpcTarget.Others, i, objs[i].transform.localRotation.x, objs[i].transform.localRotation.y, objs[i].transform.localRotation.z, objs[i].transform.localRotation.w);
                objs[i].AfterRotationSync();
            }

            if (objs[i].NeedSyncScale())
            {
                photonView.RPC("SyncScale", RpcTarget.Others, i, objs[i].transform.localScale.x, objs[i].transform.localScale.y, objs[i].transform.localScale.z);
                objs[i].AfterScaleSync();
            }
        }
    }

    public void Spawn(int id)
    {
        photonView.RPC("OnlineSpawn", RpcTarget.AllBuffered, id);
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
        photonView.RPC("SyncStatus", RpcTarget.Others, id, status, UserName.userName);
    }

    public void SetMaterial(int idMaterial)
    {
        photonView.RPC("SyncMaterial", RpcTarget.AllBuffered, idMaterial);
    }

    private void Grab()
    {
        UpdObjGrabable(false);
    }

    private void Release()
    {
        UpdObjGrabable(true);
    }

    private void UpdObjGrabable(bool status)
    {
        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].SetGrabable(status);
        }
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
    private void OnlineSpawn(int id)
    {
        GameObject newObj = Instantiate(prefabs[id], gameObject.transform.position, transform.rotation, gameObject.transform);
        InteractableObj interactableObj = newObj.GetComponent<InteractableObj>();
        interactableObj.OnSpawn(objs.Count, (int)material, this, _displayType);
        interactableObj.OnDestroy.AddListener((num) => photonView.RPC("destroy", RpcTarget.AllBuffered, num));
        interactableObj.OnCatchStatusChange.AddListener((status) => SetGrabable(status));
        objs.Add(interactableObj);
    }

    [PunRPC]
    private void SyncPos(int id, float x, float y, float z)
    {
        objs[id].UpdPosition(x, y, z);
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

    [PunRPC]
    private void SyncMaterial(int idMaterial)
    {
        material = (ObjMaterial)idMaterial;
    }
}
