using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 某一关的牌库配置：每种动物几张，在 Project 里做多个 .asset（L1、L2…）。
/// 菜单：Create → Game → Level Deck Config
/// </summary>
[CreateAssetMenu(fileName = "LevelDeck", menuName = "Game/Level Deck Config", order = 1)]
public class LevelDeckConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public AnimalData data;
        [Min(0)] public int count = 1;
    }

    [Tooltip("按顺序加入牌库；同一种 AnimalData 可写多行或一行 count>1")]
    public List<Entry> entries = new List<Entry>();

    [Header("可选：凑满总张数")]
    [Tooltip(">0 时：在 entries 加完后若总张数仍不足，用 padAnimal 一直补到该数量（例如用狗补满 30）")]
    public int padToTotal;

    public AnimalData padAnimal;

    [Header("进关时")]
    [Tooltip("勾选则先清空放归记录再洗牌（本关牌库与大地图放归无关时用）")]
    public bool resetReleaseRecordsOnLoad;

    public List<AnimalCard> BuildDeck()
    {
        var list = new List<AnimalCard>();
        if (entries != null)
        {
            foreach (Entry e in entries)
            {
                if (e?.data == null || e.count <= 0) continue;
                for (int i = 0; i < e.count; i++)
                    list.Add(new AnimalCard(e.data));
            }
        }

        if (padToTotal > 0 && padAnimal != null)
        {
            while (list.Count < padToTotal)
                list.Add(new AnimalCard(padAnimal));
        }

        return list;
    }
}
