using UnityEngine;

/// <summary>
/// 【维护费】每回合结束时消耗 1 点食物维持在场。
/// 食物不足时动物自动退场（HP 置 0，由 BattleController 移除）。
/// 初次部署的回合不触发（HasSurvivedOneTurn 由 AnimalCard 自动管理）。
/// 在 Project 右键 → Create → Game/Ability/FoodUpkeep 创建资产。
/// </summary>
[CreateAssetMenu(fileName = "FoodUpkeepAbility", menuName = "Game/Ability/FoodUpkeep", order = 3)]
public class FoodUpkeepAbility : AbilityBase
{
    [Header("每回合维护费")]
    public int upkeepCost = 1;

    public override void OnTurnEnd(AbilityContext ctx)
    {
        bool paid = ctx.Resource.ConsumeFood(upkeepCost);

        if (paid)
        {
            Debug.Log($"[Ability-维护费] {ctx.Self.CardName} 支付 {upkeepCost} 食物维持在场");
        }
        else
        {
            // 食物不足，将 HP 置 0 触发退场流程
            ctx.Self.CurrentHp = 0;
            Debug.Log($"[Ability-维护费] {ctx.Self.CardName} 食物不足，自动退场");
        }
    }
}
