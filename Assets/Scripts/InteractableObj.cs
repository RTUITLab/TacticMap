using Microsoft.MixedReality.Toolkit.UI;
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

    [Header("Settings")]
    [SerializeField] private ColorPalette colorPalette;
    [SerializeField] private Vector3 offset;

    [Header("Objects Meshes")]
    [SerializeField] private MeshRenderer symbol; //Условное обозначение на топографической карте.
    [SerializeField] private MeshRenderer model;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("MRTK dependencies")]
    [SerializeField] private ObjectManipulator objectManipulator;
    [SerializeField] private BoundsControl boundingBox;

    private bool canGrab = true;
    private int colorID;
    private Map map;
    private string catherName = "";
    private static float delta = 0.02f;
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
        transform.localPosition += offset;
        direction = transform.localPosition;
    }

    private void Update()
    {
        if (localStatus == Statuses.Mine || direction == transform.localPosition) { return; }
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, direction, delta);
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
        if (localStatus == Statuses.Mine)
        {
            return true;
        }
        return false;
    }

    public bool NeedSyncRotation()
    {
        if (localStatus == Statuses.Mine)
        {
            return true;
        }
        return false;
    }

    public bool NeedSyncScale()
    {
        if (localStatus == Statuses.Mine)
        {
            return true;
        }
        return false;
    }

    public void ApplyDirection(float x, float y, float z)
    {
        direction = new Vector3(x, y, z);
    }

    public void UpdRotation(float x, float y, float z, float w)
    {
        transform.localRotation = new Quaternion(x, y, z, w);
    }

    public void UpdScale(float x, float y, float z)
    {
        transform.localScale = new Vector3(x, y, z);
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
            ChangeAllColors(colorPalette.standardColor);
            boundingBox.enabled = false;
            objectManipulator.enabled = false;
        }
        else if (localStatus == Statuses.Nobody)
        {
            ChangeAllColors(colorPalette.GetColors()[colorID]);
            if (canGrab)
            {
                boundingBox.enabled = true;
                objectManipulator.enabled = true;
            }
            catherName = "";
        }
        textMesh.text = catherName;
    }

    private void ChangeAllColors(Color color)
    {
        symbol.material.color = color;
        model.material.color = color;
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
            model.enabled = true;
            symbol.enabled = false;
        }
        else if (type == DisplayTypes.Symbol)
        {
            model.enabled = false;
            symbol.enabled = true;
        }
    }

    public void OnSpawn(int id, int colorID, Map map, DisplayTypes displayType, bool canGrab)
    {
        this.canGrab = canGrab;
        this.id = id;
        this.map = map;
        ChangeDisplayType(displayType);
        ChangeAllColors(colorPalette.GetColors()[colorID]);
        this.colorID = colorID;
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
