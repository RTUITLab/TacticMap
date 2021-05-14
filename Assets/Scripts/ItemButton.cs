using UnityEngine;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private byte modelID;
    [SerializeField] private ItemMenu itemMenu;
    [SerializeField] private Renderer modelRenderer;

    public void SetObjColor(Color color)
    {
        modelRenderer.material.color = color;
    }

    public void SpawnModel()
    {
        itemMenu.SpawnModel(modelID);
    }
}
