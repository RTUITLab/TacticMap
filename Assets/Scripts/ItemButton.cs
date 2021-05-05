using UnityEngine;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private byte modelID;
    [SerializeField] private ItemMenu itemMenu;
    [SerializeField] private Material[] materials;
    [SerializeField] private Renderer modelRenderer;

    public void SetObjMaterial(ObjMaterial objMaterial)
    {
        modelRenderer.material = materials[(int)objMaterial - 1];
    }

    public void SpawnModel()
    {
        itemMenu.SpawnModel(modelID);
    }
}
