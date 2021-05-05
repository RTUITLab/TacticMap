using UnityEngine;
using TMPro;

public class LogPlate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    private void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

     private void HandleLog(string logString, string stackTrace, LogType type)
    {
        textMeshPro.text = $"\n [{type}] : {logString} {textMeshPro.text}";
    }
}
