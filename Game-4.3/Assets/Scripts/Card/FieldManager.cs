using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场上管理：摆放动物、撤下动物（回手牌）、血量追踪
/// 依赖：HandManager, ResourceManager, GameConfig
/// </summary>
public class FieldManager : MonoBehaviour
{
    public static FieldManager Instance { get; private set; }

    [Header("依赖组件")]
    [SerializeField] private HandManager     handManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameConfig      gameConfig;

    /// <summary>场上的动物（含运行时 HP）</summary>
    private List<AnimalCard> field = new List<AnimalCard>();

    public static event System.Action OnFieldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 将手牌中的一张动物卡摆放到场上。
    /// 成功返回 true；食物不足或场地已满返回 false。
    /// </summary>
    public bool PlaceAnimal(AnimalCard card)
    {
        if (card == null) return false;

        int maxField = gameConfig != null ? gameConfig.maxFieldSize : 5;
        if (field.Count >= maxField)
        {
            Debug.LogWarning($"[FieldManager] 场地已满（{maxField}），无法放置 {card.CardName}");
            return false;
        }

        if (!resourceManager.ConsumeFood(card.FoodCost))
        {
            Debug.LogWarning($"[FieldManager] 食物不足，无法放置 {card.CardName}（需要 {card.FoodCost}）");
            return false;
        }

        if (!handManager.PlayCard(card))
        {
            resourceManager.AddFood(card.FoodCost);
            return false;
        }

        card.CurrentHp = card.MaxHp;

        field.Add(card);
        Debug.Log($"[FieldManager] {card.CardName} 上场（场地 {field.Count}/{maxField}）");
        OnFieldChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 将场上的动物撤回手牌（主动撤回，不消耗资源）。
    /// </summary>
    public bool RecallAnimal(AnimalCard card)
    {
        if (!field.Contains(card))
        {
            Debug.LogWarning($"[FieldManager] {card?.CardName} 不在场上，无法撤回");
            return false;
        }

        field.Remove(card);
        handManager.ReturnToHand(card);
        Debug.Log($"[FieldManager] {card.CardName} 从场上撤回手牌");
        OnFieldChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 从场上移除动物（死亡时由 CombatCalculator 调用，不返回手牌）。
    /// </summary>
    public void RemoveAnimalFromField(AnimalCard card)
    {
        if (field.Remove(card))
        {
            Debug.Log($"[FieldManager] {card.CardName} 被移出场地（死亡）");
            OnFieldChanged?.Invoke();
        }
    }

    /// <summary>获取当前场上动物列表（只读副本）</summary>
    public List<AnimalCard> GetField()
    {
        return new List<AnimalCard>(field);
    }

    /// <summary>场上动物数量</summary>
    public int FieldCount => field.Count;

    /// <summary>清空场地（战斗结束时可选调用）</summary>
    public void ClearField()
    {
        field.Clear();
        OnFieldChanged?.Invoke();
    }

    /// <summary>更新指定动物的当前 HP（由 CombatCalculator 在结算后调用）</summary>
    public void UpdateAnimalHp(AnimalCard card, int newHp)
    {
        if (field.Contains(card))
        {
            card.CurrentHp = Mathf.Max(0, newHp);
            OnFieldChanged?.Invoke();
        }
    }
}
