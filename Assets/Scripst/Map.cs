using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class Map : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] prefabs;
    List<InteractableObj> objs = new List<InteractableObj>();
    private PhotonView photonView;

    private void Awake()
    {
        photonView = gameObject.GetComponent<PhotonView>();    
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

    public void popObj(int id)
    {
        objs.RemoveAt(id);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < objs.Count; ++i)
            {
                photonView.RPC("SyncPos", RpcTarget.Others, i, objs[i].transform.localPosition.x, objs[i].transform.localPosition.y, objs[i].transform.localPosition.z);
                photonView.RPC("SyncRot", RpcTarget.Others, i, objs[i].transform.localRotation.x, objs[i].transform.localRotation.y, objs[i].transform.localRotation.z, objs[i].transform.localRotation.w);
            }
        }
    }

    public void SyncCatchedStatus(int id, bool status)   // true я захватил, false - я отпустил
    {
        photonView.RPC("SyncStatus", RpcTarget.Others, id, status);
    }

    public void DestroyObj(int id)
    {
        photonView.RPC("destroy", RpcTarget.AllBuffered, id);
    }

    [PunRPC] private void destroy(int id)
    {
        InteractableObj buff = objs[id];
        objs.RemoveAt(id);

        for (int i = 0; i < objs.Count; ++i)
        {
            objs[i].SetNumber(i);
        }

        buff.DestroyObject();
    }

    [PunRPC] private void OnlineSpawn(int id)
    {
        GameObject newObj = Instantiate(prefabs[id], gameObject.transform.position, Quaternion.identity, gameObject.transform);
        InteractableObj interactableObj = newObj.GetComponent<InteractableObj>();
        interactableObj.SetNumber(objs.Count);
        interactableObj.map = this;
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

    [PunRPC] private void SyncStatus(int id, bool status)
    {
        objs[id].CatchObj(status);
    }
}
