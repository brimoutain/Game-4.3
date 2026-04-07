using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimalCard
{
    public string animalName;    // 动物名称
    public int hp;               // 生命值
    public int attack;           // 攻击力
    public int foodCost;         // 食物消耗

    // 构造函数
    public AnimalCard(string name, int health, int dmg, int cost)
    {
        animalName = name;
        hp = health;
        attack = dmg;
        foodCost = cost;
    }

    // 技能函数
    public void OnPlay()
    {
        // 上场时触发的技能
    }

    public void OnAttack()
    {
        // 攻击时触发的技能
    }

    public void OnDeath()
    {
        // 死亡时触发的技能
    }
}