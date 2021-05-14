using UnityEngine;

[CreateAssetMenu(menuName = "TacticMap/Color Palette")]
public class ColorPalette: ScriptableObject
{
    [Header("Color Palette")]
    [ColorUsage(false)]
    [SerializeField] private Color _standardColor; //Color on catch.
    
    [ColorUsage(false)]
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
