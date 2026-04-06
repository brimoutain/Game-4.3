//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 手牌管理：抽牌（1鱼+3随机）、回合结束弃牌入堆
///// 依赖：DeckManager, GameConfig
///// </summary>
//public class HandManager : MonoBehaviour
//{
//    // ── 单例 ──────────────────────────────────────────────────
//    public static HandManager Instance { get; private set; }

//    // ── 外部依赖 ───────────────────────────────────────────────
//    [Header("依赖组件")]
//    [SerializeField] private DeckManager deckManager;
//    [SerializeField] private GameConfig  gameConfig;

//    // ── 状态 ──────────────────────────────────────────────────
//    private List<AnimalCard> hand = new List<AnimalCard>();

//    // ── 事件 ──────────────────────────────────────────────────
//    public static event System.Action OnHandChanged;

//    // ── 生命周期 ───────────────────────────────────────────────
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//    }

//    // ── 公开接口 ───────────────────────────────────────────────

//    /// <summary>战斗开始时的初始摸牌（1张鱼类 + 3张随机）</summary>
//    public void DrawInitialHand()
//    {
//        hand.Clear();

//        // 强制摸 1 张鱼类卡
//        AnimalCard fish = deckManager.DrawFishCard();
//        if (fish != null)
//        {
//            AddToHand(fish);
//            Debug.Log($"[HandManager] 摸入鱼类卡：{fish.CardName}");
//        }
//        else
//        {
//            Debug.LogWarning("[HandManager] 牌堆中没有鱼类卡！");
//        }

//        // 再摸 3 张随机卡
//        DrawRandom(3);

//        OnHandChanged?.Invoke();
//    }

//    /// <summary>每回合开始时根据 GameConfig 抽牌数摸牌</summary>
//    public void DrawForTurn()
//    {
//        int drawCount = gameConfig != null ? gameConfig.DrawPerTurn : 2;
//        DrawRandom(drawCount);
//        OnHandChanged?.Invoke();
//    }

//    /// <summary>回合结束时将手牌全部弃入弃牌堆</summary>
//    public void DiscardHand()
//    {
//        foreach (AnimalCard card in hand)
//        {
//            deckManager.Discard(card);
//        }
//        Debug.Log($"[HandManager] 弃手牌 {hand.Count} 张");
//        hand.Clear();
//        OnHandChanged?.Invoke();
//    }

//    /// <summary>将一张手牌打出（上场时移出手牌，不入弃牌堆）</summary>
//    public bool PlayCard(AnimalCard card)
//    {
//        if (hand.Remove(card))
//        {
//            Debug.Log($"[HandManager] 打出：{card.CardName}");
//            OnHandChanged?.Invoke();
//            return true;
//        }
//        Debug.LogWarning($"[HandManager] 手牌中找不到：{card?.CardName}");
//        return false;
//    }

//    /// <summary>将卡牌放回手牌（动物从场地撤回时调用）</summary>
//    public void ReturnToHand(AnimalCard card)
//    {
//        if (card == null) return;
//        AddToHand(card);
//        Debug.Log($"[HandManager] {card.CardName} 返回手牌");
//        OnHandChanged?.Invoke();
//    }

//    /// <summary>获取手牌列表（只读副本）</summary>
//    public List<AnimalCard> GetHand()
//    {
//        return new List<AnimalCard>(hand);
//    }

//    /// <summary>手牌数量</summary>
//    public int HandCount => hand.Count;

//    // ── 内部逻辑 ───────────────────────────────────────────────

//    private void DrawRandom(int count)
//    {
//        int maxHandSize = gameConfig != null ? gameConfig.MaxHandSize : 7;
//        for (int i = 0; i < count; i++)
//        {
//            if (hand.Count >= maxHandSize)
//            {
//                Debug.Log($"[HandManager] 手牌已满（{maxHandSize}），停止摸牌");
//                break;
//            }

//            AnimalCard drawn = deckManager.DrawCard();
//            if (drawn == null)
//            {
//                Debug.Log("[HandManager] 牌堆已空，无法继续摸牌");
//                break;
//            }
//            AddToHand(drawn);
//            Debug.Log($"[HandManager] 摸牌：{drawn.CardName}（手牌 {hand.Count}/{maxHandSize}）");
//        }
//    }

//    private void AddToHand(AnimalCard card)
//    {
//        hand.Add(card);
//    }
//}
