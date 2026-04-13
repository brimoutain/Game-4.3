using UnityEngine;

/// <summary>
/// 示例技能：【毒刺】
/// 攻击后给对面怪物附加 poisonDamage 点额外伤害（模拟中毒）。
/// 在 Project 右键 → Create → Game/Ability/Poison 创建此资产。
/// </summary>
[CreateAssetMenu(fileName = "PoisonAbility", menuName = "Game/Ability/Poison", order = 1)]
public class PoisonAbility : AbilityBase
{
    [Header("毒伤")]
    [Tooltip("攻击命中后额外造成的毒伤")]
    public int poisonDamage = 2;

    public override void OnAttack(AbilityContext ctx)
    {
        if (ctx.Target == null || ctx.Target.CurrentHp <= 0) return;

        ctx.Target.CurrentHp -= poisonDamage;
        Debug.Log($"[Ability-毒刺] {ctx.Self.CardName} 的毒刺对 {ctx.Target.MonsterName}" +
                  $" 造成 {poisonDamage} 点毒伤（剩余 {Mathf.Max(0, ctx.Target.CurrentHp)} HP）");
    }
}
