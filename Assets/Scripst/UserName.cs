using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private string standartName = "Hololens";
    private string startString = "Your name: ";
    public static string userName = "";

    private TouchScreenKeyboard keyboard;

    void Start()
    {
        try
        {
            userName = PlayerPrefs.GetString("userName");
            if(userName == "")
            {
                PlayerPrefs.SetString("userName", standartName);
                userName = standartName;
            }
            PrintName(userName);
        }
        catch
        {   
            PlayerPrefs.SetString("userName", standartName);
            userName = standartName;
            Debug.Log("nen");
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
            PlayerPrefs.SetString("userName", userName);
            PrintName(userName);
        }
        else
        {
            PrintName(userName);
        }
        inputField.text = "";
    }

    public void TriggerOpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }
}
