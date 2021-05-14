using UnityEngine;

public class ItemMenu : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private ItemButton[] itemButtons;
    [SerializeField] private ColorPalette colorPalette;
    private int currColorNum = 0;

    private void Awake()
    {
        ChangeColor(colorPalette.GetColors()[currColorNum]);
    }

    public void ChangeColor()
    { 
        ChangeColor(GetNextColor());
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
        map.SetColor(currColorNum);
        map.SetModel(modelID);
        map.Spawn();
    }

    public void ChangeDisplayType()
    {
        map.BtnChangeDisplayType();
    }

    public Color GetNextColor()
    {
        currColorNum = (currColorNum + 1) % colorPalette.GetColors().Length;
        return colorPalette.GetColors()[currColorNum];
    }
}
