﻿using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class InteractableObj : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject symbol; //Условное обозначение на топографической карте.
    [SerializeField] private GameObject model;
    [SerializeField] private Material onStolenMaterial;
    [SerializeField] private MeshRenderer[] coloredObjs;
    [SerializeField] private TextMeshProUGUI textMesh;
    private Material standartMaterial;
    private ObjectManipulator objectManipulator;
    private BoundingBox boundingBox;
    private int objNum;
    [HideInInspector] public Map map;
    [HideInInspector] public Transform transform;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;
    private string catherName = "";

    private status _localStatus = status.nobody;
    private status localStatus
    {
        get { return _localStatus; }
        set
        {
            _localStatus = value;
            OnStatusChangeEvent();
        }
    }

    public enum status
    {
        nobody,
        them,
        mine
    }

    public enum displayType
    {
        model,
        symbol
    }


    public delegate void Action();
    public event Action OnStatusChangeEvent;

    void Awake()
    {
        objectManipulator = gameObject.GetComponent<ObjectManipulator>();
        boundingBox = gameObject.GetComponent<BoundingBox>();

        transform = gameObject.transform;
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        transform.localPosition += offset;

        standartMaterial = coloredObjs[0].material;
        OnStatusChangeEvent += setObjSettings;
    }

    #region transform
    public bool NeedSyncPosition()
    {
        if (transform.localPosition != lastPosition)
        {
            return true;
        }
        return false;
    }

    public bool NeedSyncRotation()
    {
        if (transform.localRotation != lastRotation)
        {
            return true;
        }
        return false;
    }

    public bool NeedSyncScale()
    {
        if (transform.localScale != lastScale)
        {
            return true;
        }
        return false;
    }

    public void AfterPositionSync()
    {
        lastPosition = transform.localPosition;
    }

    public void AfterRotationSync()
    {
        lastRotation = transform.localRotation;
    }

    public void AfterScaleSync()
    {
        lastScale = transform.localScale;
    }

    public void UpdPosition(float x, float y, float z)
    {
        transform.localPosition = new Vector3(x, y, z);
        AfterPositionSync();
    }

    public void UpdRotation(float x, float y, float z, float w)
    {
        transform.localRotation = new Quaternion(x, y, z, w);
        AfterRotationSync();
    }

    public void UpdScale(float x, float y, float z)
    {
        transform.localScale = new Vector3(x, y, z);
        AfterScaleSync();
    }

    #endregion transform

    public void SetNumber(int objNum)
    {
        this.objNum = objNum;
    }

    public int GetNumber()
    {
        return objNum;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Called only when a manipulation starts (true) or stops (false).
    /// </summary>
    /// <param name="catchedStatus"></param>
    public void LocalCatch(bool catchedStatus)
    {
        if (catchedStatus)
        {
            localStatus = status.mine;
        }
        else
        {
            localStatus = status.nobody;
            map.SyncCatchedStatus(GetNumber(), false);
        }
    }

    /// <summary>
    /// if someone grabs an object, then comes here TRUE. If the object is released, then comes FALSE. Called outside (photon).
    /// </summary>
    /// <param name="catchedStatus"></param>
    public void CatchObj(bool catchedStatus, string name)
    {
        catherName = name;
        if (catchedStatus)
        {
            localStatus = status.them;
        }
        else
        {
            localStatus = status.nobody;
        }
    }

    private void setObjSettings()
    {
        if(!NetworkManager.isOnline) { return; }
        if(localStatus == status.mine)
        {
            map.SyncCatchedStatus(GetNumber(), true);
            catherName = "";
        }
        else if (localStatus == status.them)
        {
            ChangeAllMaterial(onStolenMaterial);
            boundingBox.enabled = false;
            objectManipulator.enabled = false;
        }
        else if (localStatus == status.nobody)
        {
            ChangeAllMaterial(standartMaterial);
            boundingBox.enabled = true;
            objectManipulator.enabled = true;
            catherName = "";
        }
        textMesh.text = catherName;
    }

    private void ChangeAllMaterial(Material material)
    {
        for (int i = 0; i < coloredObjs.Length; ++i)
        {
            coloredObjs[i].material = material;
        }
    }

    public void OnTriggerStay(Collider other)   //Мусорка
    {
        if(other.tag == "recycle" && (localStatus == status.nobody && (PhotonNetwork.IsMasterClient || !NetworkManager.isOnline)))
        {
            map.DestroyObj(GetNumber());
        }
    }


    /// <summary>
    /// if True - 3D Model active
    /// </summary>
    /// <param name="type"></param>
    public void ChangeDisplayType(displayType type)
    {
        if (type == displayType.model)
        {
            model.SetActive(true);
            symbol.SetActive(false);
        }
        else if(type == displayType.symbol)
        {
            model.SetActive(false);
            symbol.SetActive(true);
        }
    }
}
