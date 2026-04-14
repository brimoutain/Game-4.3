using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts the battle by reading monster waves from the active level config.
/// </summary>
public class BattleStarter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleController battleController;
    [SerializeField] private GameInitializer gameInitializer;
    [SerializeField] private MonsterDataConfig monsterDataConfig;

    [Header("Start")]
    [SerializeField] private bool autoStartOnSceneLoad = true;

    private bool hasStarted;

    private void Awake()
    {
            battleController = FindObjectOfType<BattleController>();
            gameInitializer = FindObjectOfType<GameInitializer>();
    }

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
            Debug.LogWarning("[BattleStarter] No monsters configured in LevelConfig.");
            return;
        }

        hasStarted = true;
        battleController.StartBattle(runtimeMonsters);
    }

    private List<Monster> BuildMonsters()
    {
        var result = new List<Monster>();

        if (gameInitializer == null || gameInitializer.levelConfig == null)
        {
            Debug.LogError("[BattleStarter] GameInitializer or its levelConfig is missing.");
            return result;
        }

        if (monsterDataConfig == null)
        {
            Debug.LogError("[BattleStarter] monsterDataConfig is not assigned.");
            return result;
        }

        foreach (LevelConfig.MonsterWaveEntry entry in gameInitializer.levelConfig.monsters)
        {
            if (entry == null)
                continue;

            Monster monster = monsterDataConfig.CreateMonster(entry.type, entry.spawnTurn);
            if (monster != null)
                result.Add(monster);
        }

        return result;
    }
}
