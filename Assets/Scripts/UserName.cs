using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;

public class UserName : MonoBehaviour
{
    public static UserName instance { private set; get; }
    [SerializeField] private TextMeshProUGUI previewName;
    [SerializeField] private MRTKTMPInputField inputField;
    [SerializeField] private string standartName = "Hololens";
    private string startString = "Your name: ";
    private string fieldName = "userName";
    public string userName { private set; get; }

    private void Awake()
    {
        userName = string.Empty;
        instance = this;
    }

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
        previewName.text = startString + "\n" + userName;
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
    }
}
