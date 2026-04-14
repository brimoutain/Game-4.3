using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages hand draw, discard and play flow.
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameConfig gameConfig;

    [Header("Bonus Card")]
    [SerializeField] private AnimalData fishCardData;
    [SerializeField] private bool giveFishOnInitialDraw = true;
    [SerializeField] private bool giveFishOnTurnDraw = true;

    public static event System.Action OnHandChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        // 自动绑定持久化的 DeckManager 单例（跨场景加载时 Inspector 赋值可能失效）
        if (deckManager == null)
            deckManager = DeckManager.Instance;
    }


    private void Start()
    {
        // Start 时再次检查，因为 DeckManager 可能在 Awake 之后才初始化
        if (deckManager == null)
            deckManager = DeckManager.Instance;
    }

    public void DrawInitialHand()
    {
        if (deckManager == null)
            return;

        if (giveFishOnInitialDraw)
            AddBonusFishCard();

        DrawRandom(3);
        OnHandChanged?.Invoke();
    }

    public void DrawForTurn()
    {
        if (deckManager == null)
            return;

        int drawCount = gameConfig != null ? gameConfig.cardsPerTurn : 2;
        bool addedFish = giveFishOnTurnDraw && AddBonusFishCard();
        DrawRandom(addedFish ? drawCount - 1 : drawCount);
        OnHandChanged?.Invoke();
    }

    public void RecycleHandForNextTurn()
    {
        if (deckManager == null)
            return;

        string fishName = GetFishKindName();
        bool returnedAnyNonFish = false;

        for (int i = deckManager.handCards.Count - 1; i >= 0; i--)
        {
            AnimalCard card = deckManager.handCards[i];
            if (card == null)
                continue;

            deckManager.handCards.RemoveAt(i);
            if (IsFishCard(card, fishName))
                continue;

            deckManager.drawPile.Add(card);
            returnedAnyNonFish = true;
        }

        if (returnedAnyNonFish)
            deckManager.Shuffle(deckManager.drawPile);

        OnHandChanged?.Invoke();
    }

    public void DiscardHand()
    {
        if (deckManager == null)
            return;

        string fishName = GetFishKindName();
        for (int i = deckManager.handCards.Count - 1; i >= 0; i--)
        {
            AnimalCard card = deckManager.handCards[i];
            if (card == null)
                continue;

            deckManager.handCards.RemoveAt(i);
            if (IsFishCard(card, fishName))
                continue;

            deckManager.discardPile.Add(card);
        }

        OnHandChanged?.Invoke();
    }

    public bool PlayCard(AnimalCard card, bool notifyHandChanged = true)
    {
        if (deckManager == null || !deckManager.handCards.Remove(card))
            return false;

        if (notifyHandChanged)
            OnHandChanged?.Invoke();
        return true;
    }

    public void ReturnToHand(AnimalCard card)
    {
        if (card == null || deckManager == null)
            return;

        deckManager.ReturnToHand(card);
        OnHandChanged?.Invoke();
    }

    public List<AnimalCard> GetHand()
    {
        if (deckManager == null)
            return new List<AnimalCard>();

        return new List<AnimalCard>(deckManager.handCards);
    }

    public int HandCount => deckManager != null ? deckManager.handCards.Count : 0;

    private bool AddBonusFishCard()
    {
        if (fishCardData == null)
            return false;

        int maxHandSize = gameConfig != null ? gameConfig.maxHandSize : 7;
        if (deckManager.handCards.Count >= maxHandSize)
            return false;

        string fishName = GetFishKindName();
        if (!string.IsNullOrEmpty(fishName) && deckManager.handCards.Exists(card => IsFishCard(card, fishName)))
            return false;

        deckManager.handCards.Add(new AnimalCard(fishCardData));
        return true;
    }

    private void DrawRandom(int count)
    {
        if (deckManager == null)
            return;

        int maxHandSize = gameConfig != null ? gameConfig.maxHandSize : 7;
        string fishName = GetFishKindName();
        for (int i = 0; i < count; i++)
        {
            if (deckManager.handCards.Count >= maxHandSize)
                break;

            AnimalCard drawn = deckManager.DrawCardExcept(fishName);
            if (drawn == null)
                break;
        }
    }

    private string GetFishKindName()
    {
        return fishCardData != null ? fishCardData.animalName : string.Empty;
    }

    private static bool IsFishCard(AnimalCard card, string fishName)
    {
        return card != null && !string.IsNullOrEmpty(fishName) && card.KindName == fishName;
    }
}
