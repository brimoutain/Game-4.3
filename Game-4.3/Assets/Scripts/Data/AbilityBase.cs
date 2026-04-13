using UnityEngine;

/// <summary>
/// 所有动物技能的基类（ScriptableObject）。
/// 在 Project 窗口右键 → Create → Game/Ability 创建具体技能资产，
/// 然后拖入 AnimalData.skill 字段即可生效。
/// </summary>
public abstract class AbilityBase : ScriptableObject
{
    [Header("技能描述（显示用）")]
    [TextArea(1, 3)]
    public string description;

    /// <summary>动物上场时触发（从手牌放入槽位的瞬间）</summary>
    public virtual void OnPlay(AbilityContext ctx) { }

    /// <summary>动物攻击完怪物后触发（怪物已扣血，但死亡尚未结算）</summary>
    public virtual void OnAttack(AbilityContext ctx) { }

    /// <summary>动物 HP 降至 0，从场上移除前触发</summary>
    public virtual void OnDeath(AbilityContext ctx) { }

    /// <summary>
    /// 玩家点击 Finish（结束回合）时、战斗结算前触发。
    /// 仅在 HasSurvivedOneTurn == true 时（即非初次部署回合）调用。
    /// </summary>
    public virtual void OnTurnEnd(AbilityContext ctx) { }
}
