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

    public Sprite portrait;
    public GameObject prefab;

    public int currentHp;
    public bool isDead;

    public string MonsterName => monsterName;
    public int MaxHp => hp;
    public int CurrentHp { get => currentHp; set => currentHp = value; }
    public int Attack => attack;
    public int FoodReward => foodReward;
    public GameObject Prefab => prefab;
    public bool IsDead { get => isDead; set => isDead = value; }

    // ??????
    public Monster(string name, int health, int dmg, int reward, Sprite portraitSprite = null, GameObject monsterPrefab = null)
    {
        monsterName = name;
        hp = health;
        attack = dmg;
        foodReward = reward;
        portrait = portraitSprite;
        prefab = monsterPrefab;
        currentHp = health;
        isDead = false;
    }
}
