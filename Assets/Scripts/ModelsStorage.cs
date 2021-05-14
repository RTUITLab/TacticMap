using UnityEngine;

[CreateAssetMenu(menuName = "TacticMap/Models Storage")]
public class ModelsStorage : ScriptableObject
{
    [Header("Storage")]
    [SerializeField] private GameObject[] models;

    public GameObject[] GetModels()
    {
        return models;
    }
}
