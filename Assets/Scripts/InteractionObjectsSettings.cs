using UnityEngine;

[CreateAssetMenu(menuName = "TacticMap/Interaction Objects Settings")]
public class InteractionObjectsSettings : ScriptableObject
{
    [SerializeField] private Color _standardColor;
    [SerializeField] private Color[] Colors;
    private int currColorNum = 0;

    public Color standardColor
    {
        get
        {
            return _standardColor;
        }
    }

    public Color GetColor(int colorId)
    {
        return Colors[colorId];
    }

    public Color GetCurrColor()
    {
        return Colors[currColorNum];
    }

    public int GetCurrColorId()
    {
        return currColorNum;
    }

    public Color GetNextColor()
    {
        currColorNum = (currColorNum + 1) % Colors.Length;
        return Colors[currColorNum];
    }
}
