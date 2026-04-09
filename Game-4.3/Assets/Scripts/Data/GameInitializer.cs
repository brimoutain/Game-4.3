using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public DeckManager deckManager;
    [Header("猎豹卡牌数")]
    public int LieBaoCardCount=2;
    [Header("大象卡牌数")]
    public int DaXiangCardCount = 1;
    [Header("虫子卡牌数")]
    public int ChongziCardCount = 5;
    [Header("骆驼卡牌数")]
    public int LuotuoCardCount=2;


    void Start()
    {
        // 创建所有动物卡牌
        List<AnimalCard> starterDeck = new List<AnimalCard>();


        // 猎豹 x2
        for (int i = 0; i < LieBaoCardCount; i++)
            starterDeck.Add(new AnimalCard("猎豹", 1, 3, 2));

        // 大象 x1
        for (int i = 0; i < DaXiangCardCount; i++)
            starterDeck.Add(new AnimalCard("大象", 5, 0, 3));

        // 虫子 x5
        for (int i = 0; i < ChongziCardCount; i++)
            starterDeck.Add(new AnimalCard("虫子", 1, 1, 1));

        // 骆驼 x2
        for (int i = 0; i < LuotuoCardCount; i++)
            starterDeck.Add(new AnimalCard("骆驼", 3, 1, 1));

        //鱼
        //starterDeck.Add(new AnimalCard("鱼", 0, 1, 0));

        int DogCardCount=30-LieBaoCardCount-DaXiangCardCount-ChongziCardCount-LuotuoCardCount;
        // 狗（不可放归）x?
        for (int i = 0; i < DogCardCount; i++)
            starterDeck.Add(new AnimalCard("狗", 1, 1, 1));

        deckManager.initialDeck = starterDeck;
        deckManager.InitializeDeck();
    }
}