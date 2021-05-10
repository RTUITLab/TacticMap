using UnityEngine;
using UnityEditor;

public class EditorExtensions : MonoBehaviour
{

    [MenuItem("TacticMap/Network/StartOnline")]
    [MenuItem("CONTEXT/NetworkManager/StartOnline")]
    private static void StartOnlineMatch(MenuCommand command)
    {
        NetworkManager networkManager;
        if (command.context == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
        else
        {
            networkManager = (NetworkManager)command.context;
        }
        networkManager.StartGame(true);
    }

    [MenuItem("TacticMap/Network/StartOffline")]
    [MenuItem("CONTEXT/NetworkManager/StartOffline")]
    private static void StartOfflineMatch(MenuCommand command)
    {
        NetworkManager networkManager;
        if (command.context == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
        else
        {
            networkManager = (NetworkManager)command.context;
        }
        networkManager.StartGame(false);
    }

    [MenuItem("TacticMap/Network/StopGame")]
    [MenuItem("CONTEXT/NetworkManager/StopGame")]
    private static void StopGame(MenuCommand command)
    {
        NetworkManager networkManager;
        if (command.context == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
        else
        {
            networkManager = (NetworkManager)command.context;
        }
        networkManager.StopGame();
    }
}
