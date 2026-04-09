using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("牌组配置")]
    public List<AnimalCard> initialDeck;        // 初始牌组（在Inspector中配置）

    [Header("运行时状态")]
    public List<AnimalCard> drawPile;           // 抽牌堆
    public List<AnimalCard> discardPile;        // 弃牌堆
    public List<AnimalCard> handCards;           // 手牌

    [Header("放归记录")]
    public List<string> releasedAnimals;         // 已被放归的动物名称列表

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeDeck();
    }

    /// <summary>
    /// 初始化牌堆（战斗开始时调用）
    /// </summary>
    public void InitializeDeck()
    {
        // 清空所有牌堆
        drawPile.Clear();
        discardPile.Clear();
        handCards.Clear();

        // 复制初始牌组到抽牌堆（排除已被放归的动物）
        foreach (var card in initialDeck)
        {
            if (!releasedAnimals.Contains(card.animalName))
            {
                drawPile.Add(card);
            }
        }

        // 洗牌
        Shuffle(drawPile);
    }

    /// <summary>
    /// 洗牌（Fisher-Yates算法）
    /// </summary>
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

    /// <summary>
    /// 抽一张牌
    /// </summary>
    public AnimalCard DrawCard()
    {
        if (drawPile.Count == 0)
        {
            // 抽牌堆没牌时，将弃牌堆洗入
            if (discardPile.Count > 0)
            {
                ReshuffleDiscard();
            }
            else
            {
                Debug.Log("没有牌可抽了！");
                return null;
            }
        }

        AnimalCard drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        handCards.Add(drawnCard);
        return drawnCard;
    }

    /// <summary>
    /// 抽多张牌（每回合开始调用）
    /// </summary>
    public List<AnimalCard> DrawCards(int count)
    {
        List<AnimalCard> drawnCards = new List<AnimalCard>();
        for (int i = 0; i < count; i++)
        {
            AnimalCard card = DrawCard();
            if (card != null)
            {
                drawnCards.Add(card);
            }
        }
        return drawnCards;
    }

    /// <summary>
    /// 将弃牌堆洗入抽牌堆
    /// </summary>
    void ReshuffleDiscard()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
        Debug.Log("弃牌堆已洗入抽牌堆");
    }

    /// <summary>
    /// 将手牌放入弃牌堆（回合结束时调用）
    /// </summary>
    public void DiscardHand()
    {
        discardPile.AddRange(handCards);
        handCards.Clear();
    }

    /// <summary>
    /// 弃掉单张牌
    /// </summary>
    public void DiscardCard(AnimalCard card)
    {
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
            discardPile.Add(card);
        }
    }

    /// <summary>
    /// 从场上撤下动物（回到手牌）
    /// </summary>
    public void ReturnToHand(AnimalCard card)
    {
        if (!handCards.Contains(card))
        {
            handCards.Add(card);
        }
    }

    /// <summary>
    /// 放归动物（从整个牌组中永久移除）
    /// </summary>
    public void ReleaseAnimal(string animalName, int locationId)
    {
        // 记录已被放归
        if (!releasedAnimals.Contains(animalName))
        {
            releasedAnimals.Add(animalName);
        }

        // 从所有牌堆中移除该动物
        RemoveAnimalFromAllPiles(animalName);

        Debug.Log($"动物 {animalName} 已在地点 {locationId} 放归，永久移除牌组");
    }

    /// <summary>
    /// 从所有牌堆中移除指定动物
    /// </summary>
    void RemoveAnimalFromAllPiles(string animalName)
    {
        // 从抽牌堆移除
        drawPile.RemoveAll(card => card.animalName == animalName);

        // 从弃牌堆移除
        discardPile.RemoveAll(card => card.animalName == animalName);

        // 从手牌移除
        handCards.RemoveAll(card => card.animalName == animalName);
    }

    /// <summary>
    /// 动物死亡（本次战斗不再登场，但下一场可以）
    /// </summary>
    public void OnAnimalDeath(AnimalCard card)
    {
        // 从手牌或场上移除（根据你的战斗逻辑调用）
        // 注意：放归的动物已永久移除，死亡的动物只是本次战斗不再登场
        if (handCards.Contains(card))
        {
            handCards.Remove(card);
        }
        // 死亡的动物放入一个临时死亡池，下一场战斗会重置
        // 可以简单处理：直接丢弃，但 InitializeDeck 时会重新从 initialDeck 复制
    }

    /// <summary>
    /// 获取手牌数量
    /// </summary>
    public int GetHandCount()
    {
        return handCards.Count;
    }

    /// <summary>
    /// 获取抽牌堆剩余数量
    /// </summary>
    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }

    /// <summary>
    /// 重置放归记录（新游戏时调用）
    /// </summary>
    public void ResetReleaseRecords()
    {
        releasedAnimals.Clear();
    }
}