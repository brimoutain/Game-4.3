using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the starting deck for the current battle.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Serializable]
    public class AnimalCountEntry
    {
        public AnimalData data;
        [Min(0)] public int count = 1;
    }

    public DeckManager deckManager;

    [Header("关卡配置（优先使用）")]
    [Tooltip("赋值后优先从 LevelConfig 读取牌库和放生记录，留空则使用下方 Shared Deck")]
    public LevelConfig levelConfig;

    [Header("Shared Deck")]
    [Tooltip("Fill each animal count once. Released animals will be excluded automatically.")]
    public List<AnimalCountEntry> sharedAnimalCounts = new List<AnimalCountEntry>();

    [Tooltip("Reset released-animal records before building the deck.")]
    public bool resetSharedReleaseRecordsOnLoad;

    [Header("Legacy Scene Fallback")]
    [SerializeField, HideInInspector] private AnimalData lieBaoData;
    [SerializeField, HideInInspector] private AnimalData daXiangData;
    [SerializeField, HideInInspector] private AnimalData chongZiData;
    [SerializeField, HideInInspector] private AnimalData luoTuoData;
    [SerializeField, HideInInspector] private AnimalData gouData;
    [SerializeField, HideInInspector] private AnimalData yuData;
    [SerializeField, HideInInspector] private int LieBaoCardCount = 2;
    [SerializeField, HideInInspector] private int DaXiangCardCount = 1;
    [SerializeField, HideInInspector] private int ChongziCardCount = 5;
    [SerializeField, HideInInspector] private int LuotuoCardCount = 2;

    private void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();

        if (deckManager != null)
        {
            Debug.Log("成功获取 DeckManager");
        }
        else
        {
            Debug.LogError("未找到 DeckManager");
        }

        if (resetSharedReleaseRecordsOnLoad)
            deckManager.ResetReleaseRecords();

        deckManager.initialDeck = BuildStarterDeck();
        deckManager.InitializeDeck();
    }

    private List<AnimalCard> BuildStarterDeck()
    {
        // 优先从 LevelConfig 读取放生记录并写入 DeckManager
        if (levelConfig != null)
            ApplyLevelReleasedAnimals();

        HashSet<string> releasedAnimals = GetReleasedAnimals();

        // 优先使用 LevelConfig 的牌库配置
        if (levelConfig != null && levelConfig.animalCounts != null && levelConfig.animalCounts.Count > 0)
            return BuildLevelConfigDeck(releasedAnimals);

        if (sharedAnimalCounts != null && sharedAnimalCounts.Count > 0)
            return BuildSharedDeck(releasedAnimals);

        return BuildLegacyDeck(releasedAnimals);
    }

    private void ApplyLevelReleasedAnimals()
    {
        if (deckManager == null || levelConfig.releasedAnimals == null) return;
        foreach (AnimalData animal in levelConfig.releasedAnimals)
        {
            if (animal == null || string.IsNullOrEmpty(animal.animalName)) continue;
            if (!deckManager.releasedAnimals.Contains(animal.animalName))
                deckManager.releasedAnimals.Add(animal.animalName);
        }
    }

    private List<AnimalCard> BuildLevelConfigDeck(HashSet<string> releasedAnimals)
    {
        var starterDeck = new List<AnimalCard>();
        foreach (var entry in levelConfig.animalCounts)
        {
            if (entry == null || entry.data == null || entry.count <= 0) continue;
            if (releasedAnimals.Contains(entry.data.animalName)) continue;
            AddCards(starterDeck, entry.data, entry.count);
        }
        return starterDeck;
    }

    private HashSet<string> GetReleasedAnimals()
    {
        var released = new HashSet<string>();
        if (deckManager == null || deckManager.releasedAnimals == null)
            return released;

        foreach (string animalName in deckManager.releasedAnimals)
        {
            if (!string.IsNullOrEmpty(animalName))
                released.Add(animalName);
        }

        return released;
    }

    private List<AnimalCard> BuildSharedDeck(HashSet<string> releasedAnimals)
    {
        var starterDeck = new List<AnimalCard>();

        foreach (AnimalCountEntry entry in sharedAnimalCounts)
        {
            if (entry == null || entry.data == null || entry.count <= 0)
                continue;
            if (releasedAnimals.Contains(entry.data.animalName))
                continue;

            AddCards(starterDeck, entry.data, entry.count);
        }

        return starterDeck;
    }

    private List<AnimalCard> BuildLegacyDeck(HashSet<string> releasedAnimals)
    {
        var starterDeck = new List<AnimalCard>();

        AddCardsIfNotReleased(starterDeck, lieBaoData, LieBaoCardCount, releasedAnimals);
        AddCardsIfNotReleased(starterDeck, daXiangData, DaXiangCardCount, releasedAnimals);
        AddCardsIfNotReleased(starterDeck, chongZiData, ChongziCardCount, releasedAnimals);
        AddCardsIfNotReleased(starterDeck, luoTuoData, LuotuoCardCount, releasedAnimals);

        int dogCardCount = 30 - LieBaoCardCount - DaXiangCardCount - ChongziCardCount - LuotuoCardCount;
        AddCardsIfNotReleased(starterDeck, gouData, dogCardCount, releasedAnimals);

        return starterDeck;
    }

    private static void AddCardsIfNotReleased(List<AnimalCard> deck, AnimalData template, int count, HashSet<string> releasedAnimals)
    {
        if (template == null || count <= 0)
            return;
        if (releasedAnimals != null && releasedAnimals.Contains(template.animalName))
            return;

        AddCards(deck, template, count);
    }

    private static void AddCards(List<AnimalCard> deck, AnimalData template, int count)
    {
        if (template == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
            deck.Add(new AnimalCard(template));
    }
}
