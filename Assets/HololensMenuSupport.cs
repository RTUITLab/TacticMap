using System;
using UnityEngine;

public class HololensMenuSupport : MonoBehaviour
{
    [Header("Hidden Menu")]
    [SerializeField] private GameObject hiddenMenu;

    [Header("Useless hide button")]
    [SerializeField] private GameObject hideMenuButton;

    [Header("For Hololens 2 components")]
    [SerializeField] private MonoBehaviour[] uselessComponents;

    private void Awake()
    {
        if (Environment.Is64BitProcess)
        {
            hiddenMenu.SetActive(true); 
            hideMenuButton.SetActive(false);
            Debug.Log("Is 64 Bit Process");
            foreach (MonoBehaviour script in uselessComponents)
            {
                script.enabled = false;
            }
        }
    }
}
