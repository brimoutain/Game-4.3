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

    [Header("特殊技能")]
    [Tooltip("留空表示无特殊技能")]
    public AbilityBase skill;

    [HideInInspector]
    public GameObject cardPrefab;
}
