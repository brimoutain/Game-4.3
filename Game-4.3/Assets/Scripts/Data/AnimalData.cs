using UnityEngine;

/// <summary>
/// Shared data for one animal type.
/// </summary>
[CreateAssetMenu(fileName = "NewAnimal", menuName = "Game/Animal Data", order = 0)]
public class AnimalData : ScriptableObject
{
    [Header("Identity")]
    public string animalName;

    [Header("Stats")]
    public int hp = 1;
    public int attack = 1;
    public int foodCost = 1;

    [Header("Card UI")]
    [Tooltip("Optional portrait used by generic card prefabs.")]
    public Sprite portrait;

    [HideInInspector]
    public GameObject cardPrefab;
}
