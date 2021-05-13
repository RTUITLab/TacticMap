using UnityEngine;

public class ItemMenu : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private ItemButton[] itemButtons;
    [SerializeField] private InteractionObjectsSettings interactionObjectsSettings;

    private void Awake()
    {
        ChangeColor(interactionObjectsSettings.GetCurrColor());
    }

    public void ChangeColor()
    { 
        ChangeColor(interactionObjectsSettings.GetNextColor());
    }

    private void ChangeColor(Color color)
    {
        foreach (var itemButton in itemButtons)
        {
            itemButton.SetObjColor(color);
        }
    }

    public void SpawnModel(int modelID)
    {
        map.SetColor(interactionObjectsSettings.GetCurrColorId());
        map.SetModel(modelID);
        map.Spawn();
    }

    public void ChangeDisplayType()
    {
        map.BtnChangeDisplayType();
    }
}
