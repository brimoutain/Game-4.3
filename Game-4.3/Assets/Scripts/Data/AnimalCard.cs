using System;
using UnityEngine;

/// <summary>
/// �ƶ��е�һ����ʵ�������ù����� <see cref="AnimalData"/>��ͬ�ֶ��������ָ��ͬһ Data��
/// �� <see cref="currentHp"/> Ϊ���ڿɱ�״̬��
/// </summary>
[Serializable]
public class AnimalCard
{
    public AnimalData data;

    /// <summary>战斗中的当前 HP，上场时重置为 Data.hp</summary>
    public int currentHp;

    /// <summary>
    /// 是否已经历过至少一次回合结束（EndTurn）。
    /// 初次部署的回合 = false，之后每次 EndTurn 后设为 true。
    /// 用于"初次部署不触发"类技能的判断。
    /// </summary>
    public bool HasSurvivedOneTurn;

    /// <summary>��Ź顢�顸�㡹���߼�һ�µ�������</summary>
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

    public void OnPlay(AbilityContext ctx)
    {
        data?.skill?.OnPlay(ctx);
    }

    public void OnAttack(AbilityContext ctx)
    {
        data?.skill?.OnAttack(ctx);
    }

    public void OnDeath(AbilityContext ctx)
    {
        data?.skill?.OnDeath(ctx);
    }

    /// <summary>
    /// 回合结束时调用。
    /// 内部自动跳过初次部署回合（HasSurvivedOneTurn == false 时只标记，不触发技能）。
    /// </summary>
    public void OnTurnEnd(AbilityContext ctx)
    {
        if (!HasSurvivedOneTurn)
        {
            HasSurvivedOneTurn = true; // 标记：下一回合起生效
            return;
        }
        data?.skill?.OnTurnEnd(ctx);
    }
}
