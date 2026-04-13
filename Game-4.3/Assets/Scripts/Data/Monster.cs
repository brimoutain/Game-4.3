using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Monster
{
    public string monsterName;   // ????????
    public int hp;               // ?????
    public int attack;           // ??????
    public int foodReward;       // 食物奖励

    public GameObject prefab;
    public int spawnTurn = 1;
    public int slotIndex = -1;

    public int currentHp;
    public bool isDead;

    public string MonsterName => monsterName;
    public int MaxHp => hp;
    public int CurrentHp { get => currentHp; set => currentHp = value; }
    public int Attack => attack;
    public int FoodReward => foodReward;
    public GameObject Prefab => prefab;
    public int SpawnTurn => Mathf.Max(1, spawnTurn);
    public int SlotIndex { get => slotIndex; set => slotIndex = value; }
    public bool IsDead { get => isDead; set => isDead = value; }

    // ??????
    public Monster(string name, int health, int dmg, int reward, GameObject monsterPrefab = null, int appearTurn = 1)
    {
        monsterName = name;
        hp = health;
        attack = dmg;
        foodReward = reward;
        prefab = monsterPrefab;
        spawnTurn = Mathf.Max(1, appearTurn);
        currentHp = health;
        isDead = false;
    }
}
