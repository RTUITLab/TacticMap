using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemMenu : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private ItemButton[] itemButtons;
    private ObjMaterial currMaterial = ObjMaterial.Red;

    public void ChangeMaterial()
    {
        currMaterial = (currMaterial == ObjMaterial.Red) ? ObjMaterial.Blue : ObjMaterial.Red;
        foreach (var itemButton in itemButtons)
        {
            itemButton.SetObjMaterial(currMaterial);
        }
    }

    public void SpawnModel(int modelID)
    {
        map.SetMaterial((int)currMaterial);
        map.SetModel(modelID);
        map.Spawn();
    }

    public void ChangeDisplayType()
    {
        map.BtnChangeDisplayType();
    }
}
