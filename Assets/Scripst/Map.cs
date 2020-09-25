using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class Map : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] prefabs;
    private List<InteractableObj> objs = new List<InteractableObj>();
    private PhotonView photonView;
    private DisplayTypes _displayType = DisplayTypes.Model;
    private Transform transform;
    private ObjMaterial material = ObjMaterial.Gray;
   
    private void Awake()
    {
        photonView = gameObject.GetComponent<PhotonView>();
        transform = gameObject.transform;
    }

    void Update()
    {
        if (NetworkManager.gameStatus == GameStatus.Offline) { return; }

        for (int i = 0; i < objs.Count; ++i)
        {
            //if (objs[i].localStatus == Statuses.Them && objs[i].localStatus == Statuses.Nobody) { return; }
            if (objs[i].NeedSyncPosition())
            {
                Debug.Log("Позицию отрпавил");
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
        if (NetworkManager.gameStatus == GameStatus.Online)
        {
            photonView.RPC("OnlineSpawn", RpcTarget.AllBuffered, id);
        }
        else
        {
            OnlineSpawn(id);    //Без интернета вызываеться тот же метод что и через сеть. От сюд и название.
        }
    }

    public void BtnChangeDisplayType()
    {
        if(_displayType == DisplayTypes.Model)
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

    public void SyncCatchedStatus(int id, bool status)   // true я захватил, false - я отпустил
    {
        if(NetworkManager.gameStatus == GameStatus.Offline) { return; }
        photonView.RPC("SyncStatus", RpcTarget.Others, id, status, UserName.userName);
    }

    public void DestroyObj(int id)
    {
        if (NetworkManager.gameStatus == GameStatus.Online)
        {
            photonView.RPC("destroy", RpcTarget.AllBuffered, id);
        }
        else
        {
            destroy(id);
        }
    }

    public void SetMaterial(int idMaterial) 
    {
        if (NetworkManager.gameStatus == GameStatus.Online)
        {
            photonView.RPC("SyncMaterial", RpcTarget.AllBuffered, idMaterial);
        }
        else
        {
            material = (ObjMaterial)idMaterial;
        }
    }

    [PunRPC] private void destroy(int id)
    {
        InteractableObj buff = objs[id];
        objs.RemoveAt(id);

        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].SetID(i);
        }

        buff.DestroyObject();
    }

    [PunRPC] private void OnlineSpawn(int id)
    {
        GameObject newObj = Instantiate(prefabs[id], gameObject.transform.position, transform.rotation, gameObject.transform);
        InteractableObj interactableObj = newObj.GetComponent<InteractableObj>();
        interactableObj.OnSpawn(objs.Count, (int)material, this, _displayType);
        objs.Add(interactableObj);
    }

    [PunRPC] private void SyncPos(int id, float x, float y, float z)
    {
        objs[id].UpdPosition(x, y, z);
    }

    [PunRPC] private void SyncRot(int id, float x, float y, float z, float w)
    {
        objs[id].UpdRotation(x, y, z, w);
    }

    [PunRPC] private void SyncScale(int id, float x, float y, float z)
    {
        objs[id].UpdScale(x, y, z);
    }

    [PunRPC] private void SyncStatus(int id, bool status, string name)
    {
        objs[id].CatchObj(status, name);
    }

    [PunRPC] private void SyncMaterial(int idMaterial)
    {
        material = (ObjMaterial)idMaterial;
    }
}
