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

        if (giveFishOnTurnDraw)
            AddBonusFishCard();

        int drawCount = gameConfig != null ? gameConfig.cardsPerTurn : 2;
        DrawRandom(drawCount);
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

    public bool PlayCard(AnimalCard card)
    {
        if (deckManager == null || !deckManager.handCards.Remove(card))
            return false;

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

    private void AddBonusFishCard()
    {
        if (fishCardData == null)
            return;

        int maxHandSize = gameConfig != null ? gameConfig.maxHandSize : 7;
        if (deckManager.handCards.Count >= maxHandSize)
            return;

        string fishName = GetFishKindName();
        if (!string.IsNullOrEmpty(fishName) && deckManager.handCards.Exists(card => IsFishCard(card, fishName)))
            return;

        deckManager.handCards.Add(new AnimalCard(fishCardData));
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
