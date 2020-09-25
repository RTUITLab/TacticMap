using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class InteractableObj : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject symbol; //Условное обозначение на топографической карте.
    [SerializeField] private GameObject model;
    [SerializeField] private MeshRenderer[] coloredObjs;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Material[] standartMaterials;
    private int myMaterialId;
    private ObjectManipulator objectManipulator;
    private BoundingBox boundingBox;
    private int ID;
    private Map map;
    [HideInInspector] public Transform transform;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;
    private string catherName = "";
    private static float speed = 10f;

    private Vector3 direction = Vector3.zero;

    private Statuses _localStatus = Statuses.Nobody;
    public Statuses localStatus
    {
        get { return _localStatus; }
        set
        {
            _localStatus = value;
            OnStatusChangeEvent();
        }
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
        direction = transform.localPosition;

        OnStatusChangeEvent += setObjSettings;
    }

    private void Update()
    {
        if(localStatus == Statuses.Mine) { return; }

        if(direction != transform.localPosition)
        {
            transform.Translate(Time.deltaTime * (direction - transform.localPosition).normalized * Vector3.Distance(transform.localPosition, direction) * speed);  //Возможно это перебор (;
        }
    }

    #region transform
    /// <summary>
    /// Checking the need for synchronization.
    /// </summary>
    /// <returns></returns>
    public bool NeedSyncPosition()
    {
        if(lastPosition != transform.localPosition && localStatus == Statuses.Mine)
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

    public void SetID(int objNum)
    {
        this.ID = objNum;
    }

    public int GetID()
    {
        return ID;
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
            localStatus = Statuses.Mine;
        }
        else
        {
            direction = transform.localPosition;    //Что бы предмет не двигался после того как его отпустили локально 
            localStatus = Statuses.Nobody;
            map.SyncCatchedStatus(GetID(), false);
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
            localStatus = Statuses.Them;
        }
        else
        {
            localStatus = Statuses.Nobody;
        }
    }

    private void setObjSettings()
    {
        if(NetworkManager.gameStatus == GameStatus.Offline) { return; }
        if(localStatus == Statuses.Mine)
        {
            map.SyncCatchedStatus(GetID(), true);
            catherName = "";
        }
        else if (localStatus == Statuses.Them)
        {
            ChangeAllMaterial(standartMaterials[(int)ObjMaterial.Gray]);
            boundingBox.enabled = false;
            objectManipulator.enabled = false;
        }
        else if (localStatus == Statuses.Nobody)
        {
            ChangeAllMaterial(standartMaterials[myMaterialId]);
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
        if(other.tag == "recycle" && (localStatus == Statuses.Nobody && (PhotonNetwork.IsMasterClient || NetworkManager.gameStatus == GameStatus.Offline)))
        {
            map.DestroyObj(GetID());
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
        else if(type == DisplayTypes.Symbol)
        {
            model.SetActive(false);
            symbol.SetActive(true);
        }
    }

    public void OnSpawn(int id, int materialId, Map map, DisplayTypes displayType)
    {
        SetID(id);
        this.map = map;
        ChangeDisplayType(displayType);
        ChangeAllMaterial(standartMaterials[materialId]);
        myMaterialId = materialId;
    }

    public void log(string str)
    {
        Debug.Log(str);
    }
}
