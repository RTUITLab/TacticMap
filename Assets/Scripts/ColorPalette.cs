using UnityEngine;

[CreateAssetMenu(menuName = "TacticMap/Color Palette")]
public class ColorPalette: ScriptableObject
{
    [SerializeField] private Color _standardColor;
    [SerializeField] private Color[] Colors;

    public Color standardColor
    {
        get
        {
            return _standardColor;
        }
    }

    public Color[] GetColors()
    {
        return Colors;
    }
}
