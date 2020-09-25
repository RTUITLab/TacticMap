using TMPro;
using UnityEngine;

public class UserName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private string standartName = "Hololens";
    private string startString = "Your name: ";
    private string fieldName = "userName";
    public static string userName = "";

    private TouchScreenKeyboard keyboard;

    void Start()
    {
        try
        {
            userName = PlayerPrefs.GetString(fieldName);
            if(userName == "")
            {
                PlayerPrefs.SetString(fieldName, standartName);
                userName = standartName;
            }
            PrintName(userName);
        }
        catch
        {   
            PlayerPrefs.SetString(fieldName, standartName);
            userName = standartName;
            PrintName(userName);
        }
    }

    private void PrintName(string userName)
    {
        textMesh.text = startString + "\n" + userName;
    }

    public void BtnSetNewUserName()
    {
        if (inputField.text != "")
        {
            userName = inputField.text.Trim();
            PlayerPrefs.SetString(fieldName, userName);
            PrintName(userName);
        }
        else
        {
            PrintName(userName);
        }
        inputField.text = "";
        keyboard.active = false;
    }

    public void TriggerOpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }
}
