using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameConfig
{
    public int startingArkHp = 10;      // 方舟初始血量
    public int startingFood = 3;        // 每场战斗初始食物
    public int cardsPerTurn = 4;        // 每回合抽牌数
    public int maxDeckSize = 30;        // 牌堆上限（防止无限循环）

}
