﻿using System;
using System.Globalization;
using UnityEngine;

public class HololensMenuSupport : MonoBehaviour
{
    [Header("Hidden Menu")]
    [SerializeField] private GameObject hiddenMenu;
    [SerializeField] [Range(0, 1f)] private float menuTranslation = 0.25f;

    [Header("Useless hide button")] 
    [SerializeField] private GameObject hideMenuButton;

    [Header("For Hololens 2 components")]
    [SerializeField] private MonoBehaviour[] uselessComponents;

    private void Start()
    {
      CheckCPU();  
    }

    private void CheckCPU()
    {
        Debug.Log("Check CPU");

        if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) >= 0)
        {
            Debug.Log("ARM");
        }
        else
        {
            Debug.Log("x86");
            transform.Translate(new Vector3(0, 0, menuTranslation));
            hiddenMenu.SetActive(true);
            hideMenuButton.SetActive(false);
            foreach (MonoBehaviour script in uselessComponents)
            {
                script.enabled = false;
            }
        }
    }
}