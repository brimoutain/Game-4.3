using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场上管理：摆放动物到指定槽位、撤下动物（回手牌）、血量追踪
/// 依赖：HandManager, ResourceManager, GameConfig
/// </summary>
public class FieldManager : MonoBehaviour
{
    public const int SlotCount = 4;

    public static FieldManager Instance { get; private set; }

    [Header("依赖组件")]
    [SerializeField] private HandManager     handManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameConfig      gameConfig;

    /// <summary>槽位数组（索引 0-3），null 表示该槽空置</summary>
    private AnimalCard[] slots = new AnimalCard[SlotCount];

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

    // ── 放置动物（槽位版） ─────────────────────────────────────

    /// <summary>
    /// 将手牌中的一张动物卡放入指定槽位。
    /// 成功返回 true；食物不足、槽位已占用或无效槽位返回 false。
    /// </summary>
    public bool PlaceAnimalInSlot(AnimalCard card, int slotIndex)
    {
        if (card == null) return false;

        if (slotIndex < 0 || slotIndex >= SlotCount)
        {
            Debug.LogWarning($"[FieldManager] 槽位索引 {slotIndex} 越界");
            return false;
        }

        if (slots[slotIndex] != null)
        {
            Debug.LogWarning($"[FieldManager] 槽位 {slotIndex} 已被 {slots[slotIndex].CardName} 占用");
            return false;
        }

        if (!resourceManager.ConsumeFood(card.FoodCost))
        {
            Debug.LogWarning($"[FieldManager] 食物不足，无法放置 {card.CardName}（需要 {card.FoodCost}）");
            return false;
        }

        if (!handManager.PlayCard(card, false))
        {
            resourceManager.AddFood(card.FoodCost); // 回滚
            return false;
        }

        card.CurrentHp = card.MaxHp;
        card.HasSurvivedOneTurn = false; // 重置，确保初次部署回合不触发维护费/产粮
        slots[slotIndex] = card;

        Debug.Log($"[FieldManager] {card.CardName} 放入槽位 {slotIndex}");
        OnFieldChanged?.Invoke();

        // 触发上场技能
        AbilityContext ctx = new AbilityContext(card, null, this, null, resourceManager, slotIndex);
        card.OnPlay(ctx);

        return true;
    }

    /// <summary>
    /// 兼容旧接口：放到第一个空槽位（供非槽位调用路径使用）。
    /// </summary>
    public bool PlaceAnimal(AnimalCard card)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null)
                return PlaceAnimalInSlot(card, i);
        }
        Debug.LogWarning($"[FieldManager] 场地已满，无法放置 {card?.CardName}");
        return false;
    }

    // ── 撤回 ──────────────────────────────────────────────────

    /// <summary>将场上的动物撤回手牌（主动撤回，不消耗资源）。</summary>
    public bool RecallAnimal(AnimalCard card)
    {
        int idx = FindSlotOf(card);
        if (idx < 0)
        {
            Debug.LogWarning($"[FieldManager] {card?.CardName} 不在场上，无法撤回");
            return false;
        }

        slots[idx] = null;
        handManager.ReturnToHand(card);
        Debug.Log($"[FieldManager] {card.CardName} 从槽位 {idx} 撤回手牌");
        OnFieldChanged?.Invoke();
        return true;
    }

    // ── 死亡移除 ──────────────────────────────────────────────

    /// <summary>从场上移除动物（死亡时由 CombatCalculator 调用，不返回手牌）。</summary>
    public void RemoveAnimalFromField(AnimalCard card)
    {
        int idx = FindSlotOf(card);
        if (idx >= 0)
        {
            slots[idx] = null;
            Debug.Log($"[FieldManager] {card.CardName} 从槽位 {idx} 被移出（死亡）");
            OnFieldChanged?.Invoke();
        }
    }

    // ── 查询 ──────────────────────────────────────────────────

    /// <summary>获取当前场上动物列表（只读副本，不含 null）</summary>
    public List<AnimalCard> GetField()
    {
        var list = new List<AnimalCard>();
        foreach (var card in slots)
            if (card != null) list.Add(card);
        return list;
    }

    /// <summary>获取完整槽位数组副本（含 null，共 SlotCount 个）</summary>
    public AnimalCard[] GetSlots()
    {
        return (AnimalCard[])slots.Clone();
    }

    /// <summary>获取指定槽位的动物（null = 空）</summary>
    public AnimalCard GetSlot(int index) =>
        (index >= 0 && index < SlotCount) ? slots[index] : null;

    /// <summary>场上动物数量（不含空槽）</summary>
    public int FieldCount
    {
        get
        {
            int count = 0;
            foreach (var c in slots) if (c != null) count++;
            return count;
        }
    }

    /// <summary>清空所有槽位（战斗结束时可选调用）</summary>
    public void ClearField()
    {
        for (int i = 0; i < SlotCount; i++)
            slots[i] = null;
        OnFieldChanged?.Invoke();
    }

    /// <summary>更新指定动物的当前 HP（由 CombatCalculator 在结算后调用）</summary>
    public void UpdateAnimalHp(AnimalCard card, int newHp)
    {
        int idx = FindSlotOf(card);
        if (idx >= 0)
        {
            card.CurrentHp = Mathf.Max(0, newHp);
            OnFieldChanged?.Invoke();
        }
    }

    // ── 内部工具 ──────────────────────────────────────────────

    private int FindSlotOf(AnimalCard card)
    {
        if (card == null) return -1;
        for (int i = 0; i < SlotCount; i++)
            if (slots[i] == card) return i;
        return -1;
    }
}
