using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string version;
    [SerializeField] private byte maxPlayers;
    [SerializeField] private Map map;
    [SerializeField] private GameObject[] inGame;   //Игровое поле и другие обьекты доступные только во время игры.
    [SerializeField] private GameObject menuUI;     //UI который не должен быть виден во время игры.
    public static GameStatus gameStatus = GameStatus.Offline;

    public void StartGame(bool isOnline)
    {
        if (isOnline)
        {
            PhotonNetwork.GameVersion = version;
            PhotonNetwork.NickName = UserName.userName;
            PhotonNetwork.ConnectUsingSettings();
            menuUI.SetActive(false);
        }
        else
        {
            gameStatus = GameStatus.Offline;
            SetObjActive(inGame, true);
            menuUI.SetActive(false);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Вы вошли в комнату");
        SetObjActive(inGame, true);
        gameStatus = GameStatus.Online;
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = maxPlayers });
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

    private void SetObjActive(GameObject[] objts, bool active)
    {
        for (int i = 0; i < objts.Length; ++i)
        {
            objts[i].SetActive(active);
        }
    }
}
