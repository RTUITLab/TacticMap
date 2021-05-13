using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;

public class AnchorSystem : MonoBehaviour
{
    [SerializeField] private WorldAnchorManager worldAnchorManager;
    [SerializeField] private WorldAnchor worldAnchor;
    [SerializeField] private PhotonView photonView;
    private byte[] anchor = new byte[0];
    private WorldAnchorTransferBatch transferBatch;

    private void Awake()
    {
        transferBatch = new WorldAnchorTransferBatch();
    }

    private void Start()
    {
        Invoke("Check", 30);
    }

    private void Check()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExportWorldAnchor();
        }
    }


    public void ExportWorldAnchor()
    {
        WorldAnchorStore.GetAsync(OnCompleted);
    }

    private void OnCompleted(WorldAnchorStore store)
    {
        transferBatch.AddWorldAnchor("worldAnchor", worldAnchor);
        WorldAnchorTransferBatch.ExportAsync(transferBatch, OnDataAvailable, OnExportCompleted);
    }

    private void OnExportCompleted(SerializationCompletionReason completionreason)
    {
        Debug.Log($"OnCompleted: {completionreason}");
        if (completionreason == SerializationCompletionReason.Succeeded)
        {
            Debug.Log($"Anchor lenght: {anchor.Length}");
            photonView.RPC("SendAnchorComplete", RpcTarget.OthersBuffered, true);
        }
    }

    private void OnDataAvailable(byte[] anchorData)
    {
        photonView.RPC("SendAnchor", RpcTarget.OthersBuffered, anchorData);
        byte[] temp = new byte[anchor.Length + anchorData.Length];
        anchor.CopyTo(temp, 0);
        anchorData.CopyTo(temp, anchor.Length);
        anchor = temp;
    }

    public void ImportAnchor(byte[] anchorData)
    {
        WorldAnchorTransferBatch.ImportAsync(anchorData, OnImportComplete);
    }

    private void OnImportComplete(SerializationCompletionReason completionreason, WorldAnchorTransferBatch deserializedtransferbatch)
    {
        Debug.Log($"OnComplete: {completionreason}");
        worldAnchor = deserializedtransferbatch.LockObject(deserializedtransferbatch.GetAllIds()[0], this.gameObject);
        WorldAnchorStore.GetAsync((store) =>
        {
            store.Save("worldAnchor", worldAnchor);
        });
    }

    [PunRPC]
    private void SendAnchor(byte[] anchorData)
    {
        Debug.Log("pakage");
        byte[] temp = new byte[anchor.Length + anchorData.Length];
        anchor.CopyTo(temp, 0);
        anchorData.CopyTo(temp, anchor.Length);
        anchor = temp;
    }

    [PunRPC]
    private void SendAnchorComplete(bool check)
    {
        Debug.Log("Anchor sending comlpete");
        ImportAnchor(anchor);
    }
}
