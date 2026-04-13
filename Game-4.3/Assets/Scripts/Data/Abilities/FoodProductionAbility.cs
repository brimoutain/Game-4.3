using UnityEngine;

/// <summary>
/// 【产粮】每回合结束时产出 1 点食物。
/// 初次部署的回合不触发（HasSurvivedOneTurn 由 AnimalCard 自动管理）。
/// 在 Project 右键 → Create → Game/Ability/FoodProduction 创建资产。
/// </summary>
[CreateAssetMenu(fileName = "FoodProductionAbility", menuName = "Game/Ability/FoodProduction", order = 4)]
public class FoodProductionAbility : AbilityBase
{
    [Header("每回合产出食物量")]
    public int foodProduction = 1;

    public override void OnTurnEnd(AbilityContext ctx)
    {
        ctx.Resource.AddFood(foodProduction);
        Debug.Log($"[Ability-产粮] {ctx.Self.CardName} 产出 {foodProduction} 点食物");
    }
}
