using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗结算：玩家优先攻击、怪物反击、死亡判定、食物获得
/// 依赖：FieldManager, Monster, ResourceManager
/// </summary>
public class CombatCalculator : MonoBehaviour
{
    // ── 外部依赖 ───────────────────────────────────────────────
    [Header("依赖组件")]
    [SerializeField] private FieldManager    fieldManager;
    [SerializeField] private ArkHealthSystem arkHealthSystem;
    [SerializeField] private ResourceManager resourceManager;

    // ── 结算入口 ───────────────────────────────────────────────

    /// <summary>
    /// 执行一次完整的战斗结算流程。
    /// 返回 true 表示战斗可继续，false 表示方舟已死亡。
    /// </summary>
    public bool ResolveCombat(List<Monster> monsters)
    {
        if (monsters == null || monsters.Count == 0) return true;

        List<AnimalCard> animals = fieldManager.GetField();

        // Step 1：玩家动物优先攻击
        PlayerAttackPhase(animals, monsters);

        // Step 2：移除已死亡怪物 & 结算食物奖励
        ResolveMonsterDeaths(monsters);

        // Step 3：存活怪物反击方舟
        if (monsters.Exists(m => m.CurrentHp > 0))
        {
            MonsterCounterAttackPhase(monsters);
        }

        // Step 4：移除已死亡动物
        ResolveAnimalDeaths(animals);

        return !arkHealthSystem.IsDead();
    }

    // ── 内部阶段 ───────────────────────────────────────────────

    /// <summary>玩家场上动物依次攻击怪物（按场地顺序，依次打第一只存活怪）</summary>
    private void PlayerAttackPhase(List<AnimalCard> animals, List<Monster> monsters)
    {
        foreach (AnimalCard animal in animals)
        {
            if (animal == null || animal.CurrentHp <= 0) continue;

            // 找第一只存活的怪物
            Monster target = monsters.Find(m => m.CurrentHp > 0);
            if (target == null) break; // 全部怪物已死

            int dmg = animal.Attack;
            target.CurrentHp -= dmg;

            Debug.Log($"[Combat] {animal.CardName} 攻击 {target.MonsterName}，" +
                      $"造成 {dmg} 点伤害（剩余 {Mathf.Max(0, target.CurrentHp)} HP）");
        }
    }

    /// <summary>结算已死亡怪物：给予食物奖励并记录</summary>
    private void ResolveMonsterDeaths(List<Monster> monsters)
    {
        foreach (Monster m in monsters)
        {
            if (m.CurrentHp <= 0 && !m.IsDead)
            {
                m.IsDead = true;
                resourceManager.AddFood(m.FoodReward);
                Debug.Log($"[Combat] {m.MonsterName} 被击败！获得 {m.FoodReward} 食物");
            }
        }
    }

    /// <summary>存活怪物反击：攻击方舟本体</summary>
    private void MonsterCounterAttackPhase(List<Monster> monsters)
    {
        foreach (Monster m in monsters)
        {
            if (m.CurrentHp <= 0) continue;

            arkHealthSystem.TakeDamage(m.Attack);
            Debug.Log($"[Combat] {m.MonsterName} 反击方舟，造成 {m.Attack} 点伤害" +
                      $"（方舟剩余 {arkHealthSystem.GetCurrentHp()} HP）");

            if (arkHealthSystem.IsDead()) break; // 方舟死亡，不再继续
        }
    }

    /// <summary>移除场上已死亡动物</summary>
    private void ResolveAnimalDeaths(List<AnimalCard> animals)
    {
        for (int i = animals.Count - 1; i >= 0; i--)
        {
            if (animals[i] != null && animals[i].CurrentHp <= 0)
            {
                Debug.Log($"[Combat] {animals[i].CardName} 战死，移出战场");
                fieldManager.RemoveAnimalFromField(animals[i]);
            }
        }
    }
}
