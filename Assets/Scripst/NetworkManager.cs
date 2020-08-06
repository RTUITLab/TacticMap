using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string version;
    void Start()
    {
        PhotonNetwork.GameVersion = version;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Вы вошли в комнату");
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
        Debug.Log("Создание комнаты");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //Если нет комнаты в которую можно войти, создаём свою
    {
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)  //Если не получилось создать комнату, то корабль точно тонет.
    {
        Debug.Log($"Создание комнаты не возможно. {returnCode}, {message}");
    }

    public override void OnLeftRoom() //Когды вышел из комнаты. Наверно надо ливнуть в лобби (которого нет ибо мртк странно работает)
    {
        Debug.Log("Выход из комнаты");
    }
}
