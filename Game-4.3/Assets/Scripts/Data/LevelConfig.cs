using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个关卡的完整配置（ScriptableObject）。
/// 在 Project 右键 → Create → Game/Level Config 创建。
/// 同时被 BattleStarter（读怪物）和 GameInitializer（读牌库/放生）使用。
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config", order = 10)]
public class LevelConfig : ScriptableObject
{
    // ── 基本信息 ──────────────────────────────────────────────

    [Header("关卡信息")]
    public string levelName = "Level 1";

    // ── 怪物配置 ──────────────────────────────────────────────

    [Header("怪物波次（怪物只有两种类型）")]
    public List<MonsterWaveEntry> monsters = new List<MonsterWaveEntry>();

    // ── 牌库配置 ──────────────────────────────────────────────

    [Header("初始牌库")]
    public List<AnimalCountEntry> animalCounts = new List<AnimalCountEntry>();

    [Header("本关卡已放生的动物（这些动物不会出现在牌库中）")]
    public List<AnimalData> releasedAnimals = new List<AnimalData>();

    // ── 内部数据类 ────────────────────────────────────────────

    [Serializable]
    public class MonsterWaveEntry
    {
        [Tooltip("怪物类型")]
        public MonsterType type = MonsterType.Small;

        [Tooltip("第几回合出现（最小为1）")]
        [Min(1)] public int spawnTurn = 1;
    }

    [Serializable]
    public class AnimalCountEntry
    {
        public AnimalData data;
        [Min(0)] public int count = 1;
    }

    // ── 怪物类型定义 ─────────────────────────────────────────

    public enum MonsterType
    {
        Small,  // 小怪
        Big     // 大怪
    }
}
