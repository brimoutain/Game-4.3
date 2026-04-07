using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Monster
{
    public string monsterName;   // 怪物名称
    public int hp;               // 生命值
    public int attack;           // 攻击力
    public int foodReward;       // 食物奖励

    // 构造函数
    public Monster(string name, int health, int dmg, int reward)
    {
        monsterName = name;
        hp = health;
        attack = dmg;
        foodReward = reward;
    }
}