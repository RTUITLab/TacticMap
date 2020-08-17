using Photon.Pun;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class InteractableObj : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Material onStolenMaterial;
    private Material standartMaterial;
    [SerializeField] private MeshRenderer[] coloredObjs;
    private int catchedStatus = 0;
    private int objNum;
    private Map map;
    [HideInInspector] public Transform transform;
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;

    void Awake()
    {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
        transform = gameObject.transform;
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        transform.localPosition += offset;
        standartMaterial = coloredObjs[0].material;
    }

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
    /// Обьект ни кем не схвачен = 2 и -2, обьект мой = 1, обьект чужой = -1
    /// </summary>
    /// <param name="catchedStatus"></param>
    public void CatchObj(int catchedStatus)
    {
        this.catchedStatus = catchedStatus;

        if (catchedStatus == -1)    //Кто то другой взял обьект
        {
            ChangeAllMaterial(onStolenMaterial);
        }
        else if (catchedStatus == 2)    //Отпустили обьект
        {
            map.SyncCatchedStatus(objNum, catchedStatus);
            ChangeAllMaterial(standartMaterial);
        }
        else if (catchedStatus == -2)
        {
            ChangeAllMaterial(standartMaterial);
        }
        else if(catchedStatus == 1) //Взяли обьект
        {
            map.SyncCatchedStatus(objNum, catchedStatus);
        }

        Debug.Log($"Статус обьекта обновлён на {catchedStatus}");
    }

    private void ChangeAllMaterial(Material material)
    {
        for (int i = 0; i < coloredObjs.Length; ++i)
        {
            coloredObjs[i].material = material;
        }
    }
}
