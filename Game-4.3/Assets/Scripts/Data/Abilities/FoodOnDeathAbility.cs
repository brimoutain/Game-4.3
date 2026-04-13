using UnityEngine;

/// <summary>
/// 【遗粮】死亡时返还 1 点食物资源。
/// 在 Project 右键 → Create → Game/Ability/FoodOnDeath 创建资产。
/// </summary>
[CreateAssetMenu(fileName = "FoodOnDeathAbility", menuName = "Game/Ability/FoodOnDeath", order = 2)]
public class FoodOnDeathAbility : AbilityBase
{
    [Header("返还食物量")]
    public int foodRefund = 1;

    public override void OnDeath(AbilityContext ctx)
    {
        ctx.Resource.AddFood(foodRefund);
        Debug.Log($"[Ability-遗粮] {ctx.Self.CardName} 死亡，返还 {foodRefund} 点食物");
    }
}
