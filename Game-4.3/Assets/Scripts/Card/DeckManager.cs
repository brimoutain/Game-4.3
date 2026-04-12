using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Initial Deck")]
    public List<AnimalCard> initialDeck;

    [Header("Runtime Piles")]
    public List<AnimalCard> drawPile;
    public List<AnimalCard> discardPile;
    public List<AnimalCard> handCards;

    [Header("Released Animals")]
    public List<string> releasedAnimals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (initialDeck == null) initialDeck = new List<AnimalCard>();
        if (drawPile == null) drawPile = new List<AnimalCard>();
        if (discardPile == null) discardPile = new List<AnimalCard>();
        if (handCards == null) handCards = new List<AnimalCard>();
        if (releasedAnimals == null) releasedAnimals = new List<string>();
    }

    private void Start()
    {
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        handCards.Clear();

        foreach (AnimalCard card in initialDeck)
        {
            if (card == null || card.data == null)
                continue;
            if (releasedAnimals.Contains(card.KindName))
                continue;

            drawPile.Add(card);
        }

        Shuffle(drawPile);
    }

    public void Shuffle(List<AnimalCard> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AnimalCard temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public AnimalCard DrawCard()
    {
        return DrawCardExcept(null);
    }

    public AnimalCard DrawCardExcept(string excludedKindName)
    {
        if (!EnsureDrawPileAvailable())
            return null;

        int cardIndex = FindDrawIndex(excludedKindName);
        if (cardIndex < 0)
        {
            if (discardPile.Count == 0)
                return null;

            ReshuffleDiscard();
            cardIndex = FindDrawIndex(excludedKindName);
            if (cardIndex < 0)
                return null;
        }

        AnimalCard drawnCard = drawPile[cardIndex];
        drawPile.RemoveAt(cardIndex);
        handCards.Add(drawnCard);
        return drawnCard;
    }

    public List<AnimalCard> DrawCards(int count)
    {
        var drawnCards = new List<AnimalCard>();
        for (int i = 0; i < count; i++)
        {
            AnimalCard card = DrawCard();
            if (card == null)
                break;

            drawnCards.Add(card);
        }

        return drawnCards;
    }

    public void DiscardHand()
    {
        discardPile.AddRange(handCards);
        handCards.Clear();
    }

    public void DiscardCard(AnimalCard card)
    {
        if (!handCards.Contains(card))
            return;

        handCards.Remove(card);
        discardPile.Add(card);
    }

    public void ReturnToHand(AnimalCard card)
    {
        if (!handCards.Contains(card))
            handCards.Add(card);
    }

    public void ReleaseAnimal(string animalName, int locationId)
    {
        if (!releasedAnimals.Contains(animalName))
            releasedAnimals.Add(animalName);

        RemoveAnimalFromAllPiles(animalName);
        Debug.Log($"Released {animalName} at location {locationId}");
    }

    public void OnAnimalDeath(AnimalCard card)
    {
        if (handCards.Contains(card))
            handCards.Remove(card);
    }

    public int GetHandCount()
    {
        return handCards.Count;
    }

    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }

    public void ResetReleaseRecords()
    {
        releasedAnimals.Clear();
    }

    private bool EnsureDrawPileAvailable()
    {
        if (drawPile.Count > 0)
            return true;
        if (discardPile.Count == 0)
            return false;

        ReshuffleDiscard();
        return drawPile.Count > 0;
    }

    private int FindDrawIndex(string excludedKindName)
    {
        if (string.IsNullOrEmpty(excludedKindName))
            return drawPile.Count > 0 ? 0 : -1;

        for (int i = 0; i < drawPile.Count; i++)
        {
            AnimalCard card = drawPile[i];
            if (card == null)
                continue;
            if (card.KindName == excludedKindName)
                continue;

            return i;
        }

        return -1;
    }

    private void ReshuffleDiscard()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }

    private void RemoveAnimalFromAllPiles(string animalName)
    {
        drawPile.RemoveAll(card => card != null && card.KindName == animalName);
        discardPile.RemoveAll(card => card != null && card.KindName == animalName);
        handCards.RemoveAll(card => card != null && card.KindName == animalName);
    }
}
