using UnityEngine;

/// <summary>
/// 全局怪物数值模板（ScriptableObject，全局唯一）。
/// 在 Project 右键 → Create → Game/Monster Data Config 创建一个资产。
/// BattleStarter 用它把 LevelConfig 里的 MonsterType 转成具体的 Monster 实例。
/// </summary>
[CreateAssetMenu(fileName = "MonsterDataConfig", menuName = "Game/Monster Data Config", order = 11)]
public class MonsterDataConfig : ScriptableObject
{
    [Header("小怪数值")]
    public string smallMonsterName = "小怪";
    public int    smallHp          = 3;
    public int    smallAttack      = 2;
    public int    smallFoodReward  = 1;
    public Sprite smallPortrait;
    public GameObject smallPrefab;

    [Header("大怪数值")]
    public string bigMonsterName   = "大怪";
    public int    bigHp            = 8;
    public int    bigAttack        = 4;
    public int    bigFoodReward    = 2;
    public Sprite bigPortrait;
    public GameObject bigPrefab;

    /// <summary>根据类型生成 Monster 实例（spawnTurn 由调用方传入）</summary>
    public Monster CreateMonster(LevelConfig.MonsterType type, int spawnTurn)
    {
        switch (type)
        {
            case LevelConfig.MonsterType.Big:
                return new Monster(bigMonsterName, bigHp, bigAttack, bigFoodReward,
                                   bigPortrait, bigPrefab, spawnTurn);
            default: // Small
                return new Monster(smallMonsterName, smallHp, smallAttack, smallFoodReward,
                                   smallPortrait, smallPrefab, spawnTurn);
        }
    }
}
