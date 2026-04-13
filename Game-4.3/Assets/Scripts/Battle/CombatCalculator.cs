using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗结算（槽位对位版）：
/// 按槽位 0-3 一一对应 Monster 的 SlotIndex：
///   - 动物有 / 怪物有 → 互相攻击
///   - 动物有 / 怪物无 → 动物什么也不做
///   - 动物无 / 怪物有 → 怪物直接攻击方舟
/// 依赖：FieldManager, ArkHealthSystem, ResourceManager
/// </summary>
public class CombatCalculator : MonoBehaviour
{
    [Header("依赖组件")]
    [SerializeField] private FieldManager     fieldManager;
    [SerializeField] private ArkHealthSystem  arkHealthSystem;
    [SerializeField] private ResourceManager  resourceManager;

    // 构建 AbilityContext 的快捷方法
    private AbilityContext MakeContext(AnimalCard animal, Monster target, int slot) =>
        new AbilityContext(animal, target, fieldManager, arkHealthSystem, resourceManager, slot);

    // ── 结算入口 ───────────────────────────────────────────────

    /// <summary>
    /// 执行一次完整的槽位对位战斗结算。
    /// 返回 true 表示战斗可继续，false 表示方舟已死亡。
    /// </summary>
    public bool ResolveCombat(List<Monster> monsters)
    {
        if (monsters == null) monsters = new List<Monster>();

        AnimalCard[] animalSlots = fieldManager.GetSlots(); // 长度固定为 FieldManager.SlotCount

        // 按槽位逐一结算
        for (int slot = 0; slot < FieldManager.SlotCount; slot++)
        {
            AnimalCard animal  = animalSlots[slot];
            Monster    monster = FindMonsterInSlot(monsters, slot);

            bool hasAnimal  = animal  != null && animal.CurrentHp  > 0;
            bool hasMonster = monster != null && monster.CurrentHp > 0;

            if (hasAnimal && hasMonster)
            {
                // 互相攻击
                ResolveAnimalVsMonster(animal, monster, slot);
            }
            else if (!hasAnimal && hasMonster)
            {
                // 怪物正对空槽，直接打方舟
                ResolveMonsterAttacksArk(monster);
                if (arkHealthSystem.IsDead()) break;
            }
            // 动物对空槽：什么也不发生
        }

        // 结算死亡
        ResolveMonsterDeaths(monsters);
        ResolveAnimalDeaths(animalSlots);

        return !arkHealthSystem.IsDead();
    }

    // ── 内部阶段 ───────────────────────────────────────────────

    private void ResolveAnimalVsMonster(AnimalCard animal, Monster monster, int slot)
    {
        // Step A：动物攻击怪物
        int animalDmg = animal.Attack;
        monster.CurrentHp -= animalDmg;
        Debug.Log($"[Combat] {animal.CardName} 攻击 {monster.MonsterName}，" +
                  $"造成 {animalDmg} 伤害（怪物剩余 {Mathf.Max(0, monster.CurrentHp)} HP）");

        // Step A+：触发 OnAttack 技能
        animal.OnAttack(MakeContext(animal, monster, slot));

        // Step B：怪物存活才反击
        if (monster.CurrentHp > 0)
        {
            int monsterDmg = monster.Attack;
            animal.CurrentHp -= monsterDmg;
            Debug.Log($"[Combat] {monster.MonsterName} 反击 {animal.CardName}，" +
                      $"造成 {monsterDmg} 伤害（动物剩余 {Mathf.Max(0, animal.CurrentHp)} HP）");
        
            // ⭐ 动物受到伤害后，刷新 UI
            RefreshFieldSlotUI(slot);
        }
        else
        {
            Debug.Log($"[Combat] {monster.MonsterName} 被一击击倒，无法反击");
        }
    }

    /// <summary>
    /// 刷新指定槽位的 UI 显示
    /// </summary>
    private void RefreshFieldSlotUI(int slotIndex)
    {
        // 通过 BattleUI 或直接找到 FieldSlot 更新
        BattleUI.Instance?.RefreshFieldSlot(slotIndex);
    }

    /// <summary>怪物正对空槽，直接攻击方舟</summary>
    private void ResolveMonsterAttacksArk(Monster monster)
    {
        arkHealthSystem.TakeDamage(monster.Attack);
        Debug.Log($"[Combat] {monster.MonsterName} 正对空槽，攻击方舟造成 {monster.Attack} 伤害" +
                  $"（方舟剩余 {arkHealthSystem.GetCurrentHp()} HP）");
    }

    /// <summary>结算怪物死亡：给予食物奖励</summary>
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

    /// <summary>结算动物死亡：触发 OnDeath 技能后从场上移除</summary>
    private void ResolveAnimalDeaths(AnimalCard[] animalSlots)
    {
        for (int i = 0; i < animalSlots.Length; i++)
        {
            AnimalCard animal = animalSlots[i];
            if (animal != null && animal.CurrentHp <= 0)
            {
                Debug.Log($"[Combat] {animal.CardName} 被毁，移出槽位 {i}");
                // 触发死亡技能（如：遗粮返还食物）
                animal.OnDeath(MakeContext(animal, null, i));
                fieldManager.RemoveAnimalFromField(animal);
            }
        }
    }

    // ── 工具方法 ──────────────────────────────────────────────

    /// <summary>在怪物列表中找到占据指定槽位且存活的怪物</summary>
    private static Monster FindMonsterInSlot(List<Monster> monsters, int slotIndex)
    {
        foreach (Monster m in monsters)
            if (m != null && m.SlotIndex == slotIndex)
                return m;
        return null;
    }
}