﻿using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
[RequireComponent(typeof(BoundsControl))]
[RequireComponent(typeof(ObjectManipulator))]
[AddComponentMenu("TacticMap/Interactable Object")]

public class InteractableObj : MonoBehaviour
{
    [HideInInspector] public UnityEvent OnStatusChangeEvent;
    [HideInInspector] public DestroyEvent OnDestroy;
    [HideInInspector] public CatchEvent OnCatchStatusChange;
    [HideInInspector] public Transform transform;

    [SerializeField] private InteractionObjectsSettings settings;
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject symbol; //Условное обозначение на топографической карте.
    [SerializeField] private GameObject model;
    [SerializeField] private MeshRenderer[] coloredObjs;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private ObjectManipulator objectManipulator;
    [SerializeField] private BoundsControl boundingBox;

    private bool canGrab = true;
    private int myMaterialId;
    private Map map;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;
    private string catherName = "";
    private static float speed = 10f;
    private Vector3 direction = Vector3.zero;

    public int id
    {
        private set;
        get;
    }

    private Statuses _localStatus = Statuses.Nobody;
    public Statuses localStatus
    {
        get { return _localStatus; }
        set
        {
            _localStatus = value;
            OnStatusChangeEvent.Invoke();
        }
    }

    void Awake()
    {
        OnStatusChangeEvent.AddListener(setObjSettings);

        boundingBox.RotateStarted.AddListener(Grab);
        boundingBox.RotateStopped.AddListener(Release);
        boundingBox.ScaleStarted.AddListener(Grab);
        boundingBox.ScaleStopped.AddListener(Release);
        objectManipulator.OnManipulationStarted.AddListener((data) => Grab());
        objectManipulator.OnManipulationEnded.AddListener((data) => Release());

        transform = gameObject.transform;
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        transform.localPosition += offset;
        direction = transform.localPosition;
    }

    private void Update()
    {
        if (localStatus == Statuses.Mine) { return; }

        if (direction != transform.localPosition)
        {
            transform.Translate(Time.deltaTime * (direction - transform.localPosition).normalized * Vector3.Distance(transform.localPosition, direction) * speed);  //Возможно это перебор (;
        }
    }

    public void UpdId(int id)
    {
        this.id = id;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    public void Grab()
    {
        localStatus = Statuses.Mine;
        OnCatchStatusChange.Invoke(localStatus);
    }

    public void Release()
    {
        direction = transform.localPosition;    //Что бы предмет не двигался после того как его отпустили локально 
        localStatus = Statuses.Nobody;
        map.SyncCatchedStatus(id, false);
        OnCatchStatusChange.Invoke(localStatus);
    }

    #region transform
    /// <summary>
    /// Checking the need for synchronization.
    /// </summary>
    /// <returns></returns>
    public bool NeedSyncPosition()
    {
        if (lastPosition != transform.localPosition && localStatus == Statuses.Mine)
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
        direction = new Vector3(x, y, z);
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


    /// <summary>
    /// if someone grabs an object, then comes here TRUE. If the object is released, then comes FALSE. Called outside (photon).
    /// </summary>
    /// <param name="catchedStatus"></param>
    public void CatchObj(bool catchedStatus, string name)
    {
        catherName = name;
        if (catchedStatus)
        {
            localStatus = Statuses.Them;
        }
        else
        {
            localStatus = Statuses.Nobody;
        }
    }

    public void SetGrabable(bool status)
    {
        canGrab = status;
        if(localStatus != Statuses.Them)
        {
            objectManipulator.enabled = status;
            boundingBox.enabled = status;
        }
    }

    private void setObjSettings()
    {
        if (localStatus == Statuses.Mine)
        {
            map.SyncCatchedStatus(id, true);
            catherName = "";
        }
        else if (localStatus == Statuses.Them)
        {
            ChangeAllMaterial(settings.standartMaterials[(int)ObjMaterial.Gray]);
            boundingBox.enabled = false;
            objectManipulator.enabled = false;
        }
        else if (localStatus == Statuses.Nobody)
        {
            ChangeAllMaterial(settings.standartMaterials[myMaterialId]);
            if (canGrab)
            {
                boundingBox.enabled = true;
                objectManipulator.enabled = true;
            }
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
        if (other.tag == "recycle" && (localStatus == Statuses.Nobody && PhotonNetwork.IsMasterClient))
        {
            OnDestroy.Invoke(id);
        }
    }


    /// <summary>
    /// if True - 3D Model active
    /// </summary>
    /// <param name="type"></param>
    public void ChangeDisplayType(DisplayTypes type)
    {
        if (type == DisplayTypes.Model)
        {
            model.SetActive(true);
            symbol.SetActive(false);
        }
        else if (type == DisplayTypes.Symbol)
        {
            model.SetActive(false);
            symbol.SetActive(true);
        }
    }

    public void OnSpawn(int id, int materialId, Map map, DisplayTypes displayType, bool canGrab)
    {
        this.canGrab = canGrab;
        this.id = id;
        this.map = map;
        ChangeDisplayType(displayType);
        ChangeAllMaterial(settings.standartMaterials[materialId]);
        myMaterialId = materialId;
    }

    public void log(string str)
    {
        Debug.Log(str);
    }
}

[System.Serializable]
public class DestroyEvent : UnityEvent<int>
{
    /* так будет всё работать (; */
}

[System.Serializable]
public class CatchEvent : UnityEvent<Statuses>
{
    /* так будет всё работать (; */
}
