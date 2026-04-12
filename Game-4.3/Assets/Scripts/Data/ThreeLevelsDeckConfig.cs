using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三关牌库：在一份资源里填「第几关、每种动物几张」。Create → Game → Three Levels Deck
/// </summary>
[CreateAssetMenu(fileName = "ThreeLevelsDeck", menuName = "Game/Three Levels Deck", order = 2)]
public class ThreeLevelsDeckConfig : ScriptableObject
{
    [Serializable]
    public class AnimalCountsForLevel
    {
        public int lieBao;
        public int daXiang;
        public int chongZi;
        public int luoTuo;
        public int gou;
        public int yu;
    }

    [Header("动物数据（三关共用这些 SO）")]
    public AnimalData lieBaoData;
    public AnimalData daXiangData;
    public AnimalData chongZiData;
    public AnimalData luoTuoData;
    public AnimalData gouData;
    public AnimalData yuData;

    [Header("第 1 关 — 张数（填 0 表示该关不放）")]
    public AnimalCountsForLevel level1 = new AnimalCountsForLevel();

    [Header("第 2 关 — 张数")]
    public AnimalCountsForLevel level2 = new AnimalCountsForLevel();

    [Header("第 3 关 — 张数")]
    public AnimalCountsForLevel level3 = new AnimalCountsForLevel();

    [Header("进关时")]
    public bool resetReleaseRecordsOnLoad;

    public List<AnimalCard> BuildDeck(int battleLevel)
    {
        AnimalCountsForLevel c = battleLevel switch
        {
            1 => level1,
            2 => level2,
            3 => level3,
            _ => level1
        };

        var list = new List<AnimalCard>();
        Add(list, lieBaoData, c.lieBao);
        Add(list, daXiangData, c.daXiang);
        Add(list, chongZiData, c.chongZi);
        Add(list, luoTuoData, c.luoTuo);
        Add(list, gouData, c.gou);
        Add(list, yuData, c.yu);
        return list;
    }

    static void Add(List<AnimalCard> deck, AnimalData data, int count)
    {
        if (data == null || count <= 0) return;
        for (int i = 0; i < count; i++)
            deck.Add(new AnimalCard(data));
    }
}
