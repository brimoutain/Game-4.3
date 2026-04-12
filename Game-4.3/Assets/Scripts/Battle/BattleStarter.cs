using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a monster list from inspector data and starts the battle.
/// </summary>
public class BattleStarter : MonoBehaviour
{
    [Serializable]
    public class MonsterEntry
    {
        public string monsterName;
        public int hp = 1;
        public int attack = 1;
        public int foodReward = 1;
        public Sprite portrait;
        public GameObject prefab;
    }

    [Header("Dependencies")]
    [SerializeField] private BattleController battleController;

    [Header("Start")]
    [SerializeField] private bool autoStartOnSceneLoad = true;

    [Header("Monsters")]
    [SerializeField] private List<MonsterEntry> monsters = new List<MonsterEntry>();

    private bool hasStarted;

    private IEnumerator Start()
    {
        if (autoStartOnSceneLoad)
        {
            yield return null;
            StartConfiguredBattle();
        }
    }

    public void StartConfiguredBattle()
    {
        if (hasStarted)
            return;

        if (battleController == null)
        {
            Debug.LogError("[BattleStarter] battleController is not assigned.");
            return;
        }

        List<Monster> runtimeMonsters = BuildMonsters();
        if (runtimeMonsters.Count == 0)
        {
            Debug.LogWarning("[BattleStarter] No monsters configured.");
            return;
        }

        hasStarted = true;
        battleController.StartBattle(runtimeMonsters);
    }

    private List<Monster> BuildMonsters()
    {
        var result = new List<Monster>();

        foreach (MonsterEntry entry in monsters)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.monsterName))
                continue;

            result.Add(new Monster(
                entry.monsterName,
                entry.hp,
                entry.attack,
                entry.foodReward,
                entry.portrait,
                entry.prefab));
        }

        return result;
    }
}
