using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Handles battle UI refresh for hand cards, monsters, turn info and resources.
/// </summary>
public class BattleUI : MonoBehaviour
{
    private const string RuntimeMonsterContainerName = "MonsterRuntimeContainer";

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
    [SerializeField] private Vector2 handArcStartPosition = new Vector2(-100f, -25f);
    [SerializeField] private Vector2 handArcEndPosition = new Vector2(120f, -25f);
    [SerializeField] private float handArcRise = 40f;
    [SerializeField] private float handArcRotationMultiplier = -0.55f;

    [Header("Card Display")]
    [Tooltip("When enabled, the prefab art is used directly and TMP text fields are not overwritten.")]
    [SerializeField] private bool fullCardArtOnly = true;

    [Header("Monsters")]
    [SerializeField] private RectTransform monsterContainer;
    [Tooltip("Falls back to handCardPrefab when empty.")]
    [FormerlySerializedAs("monsterPrefab")]
    [SerializeField] private GameObject monsterCardPrefab;

    [Header("Monster Slots")]
    [SerializeField] private bool useMonsterSlotLayout = true;
    [SerializeField] private Vector2[] monsterSlotPositions = new Vector2[]
    {
        new Vector2(-270f, 110f),
        new Vector2(-90f, 110f),
        new Vector2(90f, 110f),
        new Vector2(270f, 110f)
    };

    [Header("Field Slots")]
    [Tooltip("场上4个槽位的 FieldSlot 组件，按 0-3 顺序赋值")]
    [SerializeField] private FieldSlot[] fieldSlots = new FieldSlot[FieldManager.SlotCount];

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
        FieldManager.OnFieldChanged += OnFieldChanged;
        BattleController.OnTurnStart += OnTurnStart;
        BattleController.OnTurnEnd += OnTurnEnd;
        BattleController.OnBattleOver += OnBattleOver;
    }

    private void OnDisable()
    {
        ArkHealthSystem.OnHpChanged -= OnArkHpChanged;
        ResourceManager.OnFoodChanged -= OnFoodChanged;
        HandManager.OnHandChanged -= RefreshHandUI;
        FieldManager.OnFieldChanged -= OnFieldChanged;
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
        RefreshFieldUI();
        RefreshMonsterUI();
        RefreshTurnUI();
    }

    // ── Field 事件处理 ────────────────────────────────────────

    private void OnFieldChanged()
    {
        RefreshHandUI();
        RefreshFieldUI();
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
        RefreshMonsterUI();
        Debug.Log(victory ? "[BattleUI] Battle won" : "[BattleUI] Battle lost");

        if (victory)
            StartCoroutine(LoadMapAfterDelay(1f));
    }

    private IEnumerator LoadMapAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MapUI.passed++;
        Debug.Log($"[BattleUI] Victory! MapUI.passed = {MapUI.passed}, loading Map scene.");
        SceneManager.LoadScene("Map");
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
            SetupDraggable(go, card);
        }

        LayoutHandCardsOnArc();
    }

    private void RefreshFieldUI()
    {
        if (fieldSlots == null || fieldManager == null) return;

        AnimalCard[] slots = fieldManager.GetSlots();
        for (int i = 0; i < fieldSlots.Length && i < slots.Length; i++)
        {
            if (fieldSlots[i] == null) continue;
            fieldSlots[i].SetOccupied(slots[i]);
        }
    }

    private void RefreshMonsterUI()
    {
        RectTransform monsterRoot = ResolveMonsterRoot();
        if (monsterRoot == null)
            return;

        ClearChildren(monsterRoot);

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

            GameObject go = Instantiate(prefab, monsterRoot, false);
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
            ApplyMonsterSlotLayout(go, monster);
            HidePlayButton(go);
        }
    }

    private RectTransform ResolveMonsterRoot()
    {
        if (monsterContainer != null)
            return monsterContainer;

        Transform existing = transform.Find(RuntimeMonsterContainerName);
        if (existing != null)
            return existing as RectTransform;

        GameObject runtimeContainer = new GameObject(RuntimeMonsterContainerName, typeof(RectTransform));
        RectTransform rect = runtimeContainer.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private void ApplyMonsterSlotLayout(GameObject go, Monster monster)
    {
        if (!useMonsterSlotLayout || go == null || monster == null || monsterSlotPositions == null || monsterSlotPositions.Length == 0)
            return;

        RectTransform rect = go.transform as RectTransform;
        if (rect == null)
            return;

        int slotIndex = monster.SlotIndex;
        if (slotIndex < 0 || slotIndex >= monsterSlotPositions.Length)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = monsterSlotPositions[slotIndex];
        rect.localRotation = Quaternion.identity;
        rect.SetSiblingIndex(slotIndex);
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

        Vector2 chord = handArcEndPosition - handArcStartPosition;
        float chordLength = chord.magnitude;
        bool useCurvedArc = childCount > 1 && chordLength > 0.01f && handArcRise > 0.01f;
        Vector2 midpoint = (handArcStartPosition + handArcEndPosition) * 0.5f;
        Vector2 chordDirection = chordLength > 0.01f ? chord / chordLength : Vector2.right;
        Vector2 arcNormal = new Vector2(-chordDirection.y, chordDirection.x);

        float radius = 0f;
        Vector2 circleCenter = midpoint;
        float startAngle = 0f;
        float endAngle = 0f;

        if (useCurvedArc)
        {
            float halfChord = chordLength * 0.5f;
            radius = (halfChord * halfChord) / (2f * handArcRise) + (handArcRise * 0.5f);
            float centerOffset = radius - handArcRise;
            circleCenter = midpoint - arcNormal * centerOffset;
            startAngle = Mathf.Atan2(handArcStartPosition.y - circleCenter.y, handArcStartPosition.x - circleCenter.x);
            endAngle = Mathf.Atan2(handArcEndPosition.y - circleCenter.y, handArcEndPosition.x - circleCenter.x);
        }

        for (int i = 0; i < childCount; i++)
        {
            RectTransform cardRect = handContainer.GetChild(i) as RectTransform;
            if (cardRect == null)
                continue;

            float t = childCount == 1 ? 0.5f : (float)i / (childCount - 1);
            Vector2 anchoredPosition;
            float rotation = 0f;

            if (useCurvedArc)
            {
                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                anchoredPosition = circleCenter + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                float relativeAngle = currentAngle * Mathf.Rad2Deg - 90f;
                rotation = relativeAngle * handArcRotationMultiplier;
            }
            else
            {
                anchoredPosition = Vector2.Lerp(handArcStartPosition, handArcEndPosition, t);
                if (childCount == 1 && handArcRise > 0.01f)
                    anchoredPosition = midpoint + arcNormal * handArcRise;
            }

            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = anchoredPosition;
            cardRect.localRotation = Quaternion.Euler(0f, 0f, rotation);
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

    /// <summary>给手牌 GameObject 添加/初始化 DraggableCard 组件</summary>
    private static void SetupDraggable(GameObject go, AnimalCard card)
    {
        DraggableCard draggable = go.GetComponent<DraggableCard>();
        if (draggable == null)
            draggable = go.AddComponent<DraggableCard>();
        draggable.Initialize(card);

        // 隐藏旧的 PlayBtn（如果 prefab 里还有的话）
        Transform playBtn = go.transform.Find("PlayBtn");
        if (playBtn != null)
            playBtn.gameObject.SetActive(false);
    }

    private static void HidePlayButton(GameObject go)
    {
        Button playButton = go.transform.Find("PlayBtn")?.GetComponent<Button>()
                            ?? go.GetComponent<Button>();
        if (playButton == null) return;
        playButton.onClick.RemoveAllListeners();
        if (playButton.gameObject == go)
            playButton.interactable = false;
        else
            playButton.gameObject.SetActive(false);
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
