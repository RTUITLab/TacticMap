using UnityEngine;

public class ItemButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private byte modelID;

    [Header("Dependencies")]
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
