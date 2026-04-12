using System;
using UnityEngine;

/// <summary>
/// 牌堆中的一张牌实例：引用共享的 <see cref="AnimalData"/>，同种动物多张牌指向同一 Data。
/// 仅 <see cref="currentHp"/> 为局内可变状态。
/// </summary>
[Serializable]
public class AnimalCard
{
    public AnimalData data;

    /// <summary>场上战斗用当前 HP；上场时会重置为 Data.hp</summary>
    public int currentHp;

    /// <summary>与放归、抽「鱼」等逻辑一致的种类名</summary>
    public string KindName => data != null ? data.animalName : string.Empty;

    public string CardName => KindName;
    public int MaxHp => data != null ? data.hp : 0;
    public int CurrentHp { get => currentHp; set => currentHp = value; }
    public int Attack => data != null ? data.attack : 0;
    public int FoodCost => data != null ? data.foodCost : 0;

    public AnimalCard()
    {
    }

    public AnimalCard(AnimalData template)
    {
        data = template;
        if (data != null)
            currentHp = data.hp;
    }

    public void OnPlay()
    {
    }

    public void OnAttack()
    {
    }

    public void OnDeath()
    {
    }
}
