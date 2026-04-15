using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Coordinates battle lifecycle, turn flow, and win/loss checks.
/// </summary>
public class BattleController : MonoBehaviour
{
    private const int MaxMonsterSlots = 4;


    [Header("Dependencies")]
    [SerializeField] private CombatCalculator combatCalculator;
    [SerializeField] private ArkHealthSystem arkHealthSystem;
    [SerializeField] private HandManager handManager;
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private ResourceManager resourceManager;

    private List<Monster> currentMonsters = new List<Monster>();
    private List<Monster> pendingMonsters = new List<Monster>();
    private int turnNumber;
    private bool battleActive;

    public static event Action OnTurnStart;
    public static event Action OnTurnEnd;
    public static event Action<bool> OnBattleOver;



    public void StartBattle(List<Monster> monsters)
    {
        if (monsters == null || monsters.Count == 0)
        {
            Debug.LogWarning("[BattleController] StartBattle: monster list is empty.");
            return;
        }
        
        if (fieldManager != null)
        {
            fieldManager.ClearField();
            Debug.Log("[BattleController] 已清空上一场场地");
        }

        currentMonsters.Clear();
        pendingMonsters = new List<Monster>(monsters);
        foreach (Monster monster in pendingMonsters)
        {
            if (monster != null)
                monster.SlotIndex = -1;
        }
        turnNumber = 0;
        battleActive = true;
        
        

        Debug.Log($"[BattleController] Battle started with {pendingMonsters.Count} configured monsters.");

        resourceManager.ResetFood();
        handManager.DrawInitialHand();

        StartTurn(false);
    }

    public void EndTurn()
    {
        if (!battleActive)
            return;

        Debug.Log($"[BattleController] Turn {turnNumber} ended.");
        OnTurnEnd?.Invoke();
        handManager?.RecycleHandForNextTurn();

        // 触发场上动物的回合结束技能（维护费/产出食物等），初次部署回合自动跳过
        ProcessFieldAbilitiesOnTurnEnd();

        bool battleContinues = combatCalculator.ResolveCombat(currentMonsters);

        if (arkHealthSystem.IsDead())
        {
            EndBattle(false);
            return;
        }

        currentMonsters.RemoveAll(monster => monster == null || monster.CurrentHp <= 0);
        if (currentMonsters.Count == 0 && pendingMonsters.Count == 0)
        {
            EndBattle(true);
            return;
        }

        if (!battleContinues)
        {
            EndBattle(false);
            return;
        }

        StartTurn(true);
    }

    public List<Monster> GetCurrentMonsters()
    {
        return new List<Monster>(currentMonsters);
    }

    public int GetTurnNumber() => turnNumber;

    public bool IsBattleActive() => battleActive;

    private void StartTurn(bool drawCardsAtTurnStart)
    {
        turnNumber++;
        Debug.Log($"[BattleController] Turn {turnNumber} started.");

        if (drawCardsAtTurnStart)
            handManager.DrawForTurn();

        SpawnMonstersForCurrentTurn();
        OnTurnStart?.Invoke();
    }

    private void SpawnMonstersForCurrentTurn()
    {
        if (pendingMonsters == null || pendingMonsters.Count == 0)
            return;

        for (int i = 0; i < pendingMonsters.Count; i++)
        {
            Monster monster = pendingMonsters[i];
            if (monster == null || monster.SpawnTurn > turnNumber)
                continue;

            int slotIndex = FindNextAvailableSlot();
            if (slotIndex < 0)
                break;

            monster.SlotIndex = slotIndex;
            currentMonsters.Add(monster);
            pendingMonsters.RemoveAt(i);
            i--;

            Debug.Log($"[BattleController] Monster {monster.MonsterName} spawned on turn {turnNumber} in slot {slotIndex}.");
        }
    }

    private int FindNextAvailableSlot()
    {
        for (int slotIndex = 0; slotIndex < MaxMonsterSlots; slotIndex++)
        {
            bool occupied = currentMonsters.Exists(monster => monster != null && monster.SlotIndex == slotIndex);
            if (!occupied)
                return slotIndex;
        }

        return -1;
    }

    /// <summary>
    /// 遍历场上全部动物，逐一触发 OnTurnEnd 技能。
    /// AnimalCard.OnTurnEnd 内部会自动跳过初次部署回合。
    /// 如果某个动物在技能触发后被退场（HP为0或食物不足退场），从场上移除。
    /// </summary>
    private void ProcessFieldAbilitiesOnTurnEnd()
    {
        AnimalCard[] slots = fieldManager.GetSlots();
        for (int i = 0; i < slots.Length; i++)
        {
            AnimalCard animal = slots[i];
            if (animal == null || animal.CurrentHp <= 0) continue;

            AbilityContext ctx = new AbilityContext(
                animal, null, fieldManager, arkHealthSystem, resourceManager, i);

            animal.OnTurnEnd(ctx);

            // 技能可能将动物标记为退场（HP<=0），移除之
            if (animal.CurrentHp <= 0)
            {
                Debug.Log($"[BattleController] {animal.CardName} 回合结束后退场");
                fieldManager.RemoveAnimalFromField(animal);
            }
        }
    }

    private void EndBattle(bool victory)
    {
        battleActive = false;
        Debug.Log(victory
            ? "[BattleController] Battle ended in victory."
            : "[BattleController] Battle ended in defeat.");

        handManager.DiscardHand();
        OnBattleOver?.Invoke(victory);
        if (!victory)
        {
            OnBattleDefeat();
        }
    }
    
    public void OnBattleDefeat()
    {
        if (!battleActive) return;

        SceneManager.LoadScene("Fail");
    }
 
}
