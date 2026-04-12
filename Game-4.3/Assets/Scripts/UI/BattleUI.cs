using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Handles battle UI refresh for hand cards, monsters, turn info and resources.
/// </summary>
public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance { get; private set; }

    [Header("Ark UI")]
    [SerializeField] private TextMeshProUGUI arkHpText;

    [Header("Food UI")]
    [SerializeField] private TextMeshProUGUI foodText;

    [Header("Turn UI")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private Button endTurnButton;

    [Header("Hand")]
    [SerializeField] private RectTransform handContainer;
    [SerializeField] private GameObject handCardPrefab;

    [Header("Hand Arc Layout")]
    [SerializeField] private bool useHandArcLayout = true;
    [SerializeField] private Vector2 handArcLeftmostPosition = new Vector2(400f, 0f);
    [SerializeField] private float handArcDegrees = 140f;
    [SerializeField] private float handArcRadius = 420f;
    [SerializeField] private float handArcCenterYOffset = -260f;
    [SerializeField] private float handArcRotationMultiplier = -0.55f;

    [Header("Card Display")]
    [Tooltip("When enabled, the prefab art is used directly and TMP text fields are not overwritten.")]
    [SerializeField] private bool fullCardArtOnly = true;

    [Header("Monsters")]
    [SerializeField] private RectTransform monsterContainer;
    [Tooltip("Falls back to handCardPrefab when empty.")]
    [FormerlySerializedAs("monsterPrefab")]
    [SerializeField] private GameObject monsterCardPrefab;

    [Header("Dependencies")]
    [SerializeField] private BattleController battleController;
    [SerializeField] private ArkHealthSystem arkHealthSystem;
    [SerializeField] private HandManager handManager;
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private ResourceManager resourceManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        ArkHealthSystem.OnHpChanged += OnArkHpChanged;
        ResourceManager.OnFoodChanged += OnFoodChanged;
        HandManager.OnHandChanged += RefreshHandUI;
        FieldManager.OnFieldChanged += RefreshHandUI;
        BattleController.OnTurnStart += OnTurnStart;
        BattleController.OnTurnEnd += OnTurnEnd;
        BattleController.OnBattleOver += OnBattleOver;
    }

    private void OnDisable()
    {
        ArkHealthSystem.OnHpChanged -= OnArkHpChanged;
        ResourceManager.OnFoodChanged -= OnFoodChanged;
        HandManager.OnHandChanged -= RefreshHandUI;
        FieldManager.OnFieldChanged -= RefreshHandUI;
        BattleController.OnTurnStart -= OnTurnStart;
        BattleController.OnTurnEnd -= OnTurnEnd;
        BattleController.OnBattleOver -= OnBattleOver;
    }

    private void Start()
    {
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);

        RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshArkHp();
        RefreshFood();
        RefreshHandUI();
        RefreshMonsterUI();
        RefreshTurnUI();
    }

    private void OnArkHpChanged(int current, int max)
    {
        RefreshArkHp(current, max);
    }

    private void OnFoodChanged(int current, int max)
    {
        RefreshFood(current, max);
    }

    private void OnTurnStart()
    {
        RefreshTurnUI();
        RefreshMonsterUI();
        SetEndTurnButtonInteractable(true);
    }

    private void OnTurnEnd()
    {
        SetEndTurnButtonInteractable(false);
    }

    private void OnBattleOver(bool victory)
    {
        SetEndTurnButtonInteractable(false);
        Debug.Log(victory ? "[BattleUI] Battle won" : "[BattleUI] Battle lost");
    }

    private void OnEndTurnClicked()
    {
        battleController?.EndTurn();
    }

    private void RefreshArkHp(int current = -1, int max = -1)
    {
        if (arkHealthSystem == null || arkHpText == null)
            return;

        int hp = current >= 0 ? current : arkHealthSystem.GetCurrentHp();
        int maxHp = max >= 0 ? max : arkHealthSystem.GetMaxHp();
        arkHpText.text = $"{hp} / {maxHp}";
    }

    private void RefreshFood(int current = -1, int max = -1)
    {
        if (resourceManager == null || foodText == null)
            return;

        int food = current >= 0 ? current : resourceManager.GetFood();
        int maxFood = max >= 0 ? max : resourceManager.GetMaxFood();
        foodText.text = $"{food} / {maxFood}";
    }

    private void RefreshTurnUI()
    {
        if (turnText != null && battleController != null)
            turnText.text = $"{battleController.GetTurnNumber()}";
    }

    private void RefreshHandUI()
    {
        if (handContainer == null)
            return;

        ClearChildren(handContainer);

        if (handManager == null)
            return;

        foreach (AnimalCard card in handManager.GetHand())
        {
            GameObject prefab = ResolveHandPrefab(card);
            if (prefab == null)
            {
                Debug.LogWarning($"[BattleUI] No prefab for hand card: {card?.CardName}");
                continue;
            }

            GameObject go = Instantiate(prefab, handContainer, false);
            if (!fullCardArtOnly)
            {
                ApplyCardTexts(
                    go,
                    card.CardName,
                    $"{card.CurrentHp}/{card.MaxHp}",
                    $"{card.Attack}",
                    $"{card.FoodCost}");
            }

            EnsureVisibleImages(go);
            SetPortraitOnCardImage(go, card.data != null ? card.data.portrait : null);
            SetupPlayButton(go, card);
        }

        LayoutHandCardsOnArc();
    }

    private void RefreshMonsterUI()
    {
        if (monsterContainer == null)
            return;

        ClearChildren(monsterContainer);

        if (battleController == null)
            return;

        foreach (Monster monster in battleController.GetCurrentMonsters())
        {
            GameObject prefab = ResolveMonsterPrefab(monster);
            if (prefab == null)
            {
                Debug.LogWarning($"[BattleUI] No prefab for monster: {monster?.MonsterName}");
                continue;
            }

            GameObject go = Instantiate(prefab, monsterContainer, false);
            if (!fullCardArtOnly)
            {
                ApplyCardTexts(
                    go,
                    monster.MonsterName,
                    $"{Mathf.Max(0, monster.CurrentHp)}/{monster.MaxHp}",
                    $"{monster.Attack}",
                    $"{monster.FoodReward}");
            }

            EnsureVisibleImages(go);
            SetPortraitOnCardImage(go, monster.portrait);
            HidePlayButton(go);
        }
    }

    private GameObject ResolveHandPrefab(AnimalCard card)
    {
        if (card != null && card.data != null && card.data.cardPrefab != null)
            return card.data.cardPrefab;

        return handCardPrefab;
    }

    private GameObject ResolveMonsterPrefab(Monster monster)
    {
        if (monster != null && monster.Prefab != null)
            return monster.Prefab;

        if (monsterCardPrefab != null)
            return monsterCardPrefab;

        return handCardPrefab;
    }

    private void LayoutHandCardsOnArc()
    {
        if (!useHandArcLayout || handContainer == null)
            return;

        int childCount = handContainer.childCount;
        if (childCount == 0)
            return;

        float halfArc = handArcDegrees * 0.5f;
        float step = childCount > 1 ? handArcDegrees / (childCount - 1) : 0f;
        float leftmostRadians = (-halfArc) * Mathf.Deg2Rad;
        Vector2 leftmostOffset = new Vector2(
            Mathf.Sin(leftmostRadians) * handArcRadius,
            handArcCenterYOffset + (Mathf.Cos(leftmostRadians) * handArcRadius - handArcRadius));

        for (int i = 0; i < childCount; i++)
        {
            RectTransform cardRect = handContainer.GetChild(i) as RectTransform;
            if (cardRect == null)
                continue;

            float angle = childCount == 1 ? 0f : (-halfArc + step * i);
            float radians = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(radians) * handArcRadius;
            float y = handArcCenterYOffset + (Mathf.Cos(radians) * handArcRadius - handArcRadius);
            Vector2 anchoredPosition = new Vector2(x, y) - leftmostOffset + handArcLeftmostPosition;

            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = anchoredPosition;
            cardRect.localRotation = Quaternion.Euler(0f, 0f, angle * handArcRotationMultiplier);
            cardRect.SetSiblingIndex(i);
        }
    }

    private static void ApplyCardTexts(GameObject go, string name, string hp, string atk, string foodLine)
    {
        SetText(go, "CardName", name);
        SetText(go, "CardHp", hp);
        SetText(go, "CardAtk", atk);
        SetText(go, "CardFood", foodLine);
    }

    private static void SetPortraitOnCardImage(GameObject go, Sprite sprite)
    {
        if (sprite == null)
            return;

        Image image = null;
        Transform child = go.transform.Find("CardImage");
        if (child != null)
            image = child.GetComponent<Image>();
        if (image == null)
            image = go.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = true;
    }

    private static void EnsureVisibleImages(GameObject go)
    {
        Image[] images = go.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image == null || image.sprite == null)
                continue;

            Color color = image.color;
            if (color.a <= 0f)
            {
                color.a = 1f;
                image.color = color;
            }

            if (!image.enabled)
                image.enabled = true;

            if (!image.gameObject.activeSelf)
                image.gameObject.SetActive(true);
        }
    }

    private void SetupPlayButton(GameObject go, AnimalCard card)
    {
        Button playButton = ResolvePlayButton(go);
        if (playButton == null)
            return;

        if (playButton.gameObject != go)
            playButton.gameObject.SetActive(true);

        playButton.interactable = resourceManager != null && resourceManager.HasEnoughFood(card.FoodCost);
        AnimalCard captured = card;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => OnPlayCardClicked(captured));
    }

    private static Button ResolvePlayButton(GameObject go)
    {
        Button child = FindButton(go, "PlayBtn");
        if (child != null)
            return child;

        return go.GetComponent<Button>();
    }

    private static void HidePlayButton(GameObject go)
    {
        Button playButton = ResolvePlayButton(go);
        if (playButton == null)
            return;

        playButton.onClick.RemoveAllListeners();
        if (playButton.gameObject == go)
            playButton.interactable = false;
        else
            playButton.gameObject.SetActive(false);
    }

    private void OnPlayCardClicked(AnimalCard card)
    {
        if (fieldManager == null)
            return;

        if (!fieldManager.PlaceAnimal(card))
            Debug.LogWarning($"[BattleUI] Failed to place card: {card.CardName}");
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);
    }

    private static void SetText(GameObject go, string childName, string value)
    {
        Transform child = go.transform.Find(childName);
        if (child == null)
            return;

        TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = value;
    }

    private static Button FindButton(GameObject go, string childName)
    {
        Transform child = go.transform.Find(childName);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private void SetEndTurnButtonInteractable(bool interactable)
    {
        if (endTurnButton != null)
            endTurnButton.interactable = interactable;
    }
}
