//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

///// <summary>
///// 战斗界面总控：更新血量/食物/手牌/场上/怪物显示
///// 依赖：BattleController, CombatCalculator, ArkHealthSystem,
/////        HandManager, FieldManager, ResourceManager
///// </summary>
//public class BattleUI : MonoBehaviour
//{
//    // ── 单例 ──────────────────────────────────────────────────
//    public static BattleUI Instance { get; private set; }

//    // ── Inspector 绑定 ─────────────────────────────────────────
//    [Header("── 方舟血量 ──")]
//    [SerializeField] private TextMeshProUGUI arkHpText;
//    [SerializeField] private Slider          arkHpSlider;

//    [Header("── 食物 ──")]
//    [SerializeField] private TextMeshProUGUI foodText;
//    [SerializeField] private Slider          foodSlider;

//    [Header("── 回合信息 ──")]
//    [SerializeField] private TextMeshProUGUI turnText;
//    [SerializeField] private Button          endTurnButton;

//    [Header("── 消息提示 ──")]
//    [SerializeField] private TextMeshProUGUI messageText;
//    [SerializeField] private float           messageDuration = 2f;

//    [Header("── 手牌区 ──")]
//    [SerializeField] private Transform       handContainer;   // 手牌卡牌的父节点
//    [SerializeField] private GameObject      handCardPrefab;  // 手牌卡牌预制体

//    [Header("── 场地区 ──")]
//    [SerializeField] private Transform       fieldContainer;  // 场上卡牌的父节点
//    [SerializeField] private GameObject      fieldCardPrefab; // 场上卡牌预制体

//    [Header("── 怪物区 ──")]
//    [SerializeField] private Transform       monsterContainer;  // 怪物的父节点
//    [SerializeField] private GameObject      monsterPrefab;     // 怪物 UI 预制体

//    // ── 外部依赖（场景引用）────────────────────────────────────
//    [Header("── 系统依赖 ──")]
//    [SerializeField] private BattleController battleController;
//    [SerializeField] private ArkHealthSystem  arkHealthSystem;
//    [SerializeField] private HandManager      handManager;
//    [SerializeField] private FieldManager     fieldManager;
//    [SerializeField] private ResourceManager  resourceManager;

//    // ── 内部状态 ───────────────────────────────────────────────
//    private Coroutine messageClearCoroutine;

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

//    private void OnEnable()
//    {
//        // 订阅系统事件
//        ArkHealthSystem.OnHpChanged      += OnArkHpChanged;
//        ResourceManager.OnFoodChanged     += OnFoodChanged;
//        HandManager.OnHandChanged         += RefreshHandUI;
//        FieldManager.OnFieldChanged       += RefreshFieldUI;
//        BattleController.OnTurnStart      += OnTurnStart;
//        BattleController.OnTurnEnd        += OnTurnEnd;
//        BattleController.OnBattleOver     += OnBattleOver;
//    }

//    private void OnDisable()
//    {
//        ArkHealthSystem.OnHpChanged      -= OnArkHpChanged;
//        ResourceManager.OnFoodChanged     -= OnFoodChanged;
//        HandManager.OnHandChanged         -= RefreshHandUI;
//        FieldManager.OnFieldChanged       -= RefreshFieldUI;
//        BattleController.OnTurnStart      -= OnTurnStart;
//        BattleController.OnTurnEnd        -= OnTurnEnd;
//        BattleController.OnBattleOver     -= OnBattleOver;
//    }

//    private void Start()
//    {
//        // 绑定结束回合按钮
//        if (endTurnButton != null)
//            endTurnButton.onClick.AddListener(OnEndTurnClicked);

//        RefreshUI();
//    }

//    // ── 公开接口（供 A 侧调用）────────────────────────────────

//    /// <summary>刷新所有 UI 面板</summary>
//    public void RefreshUI()
//    {
//        RefreshArkHp();
//        RefreshFood();
//        RefreshHandUI();
//        RefreshFieldUI();
//        RefreshMonsterUI();
//        RefreshTurnUI();
//    }

//    /// <summary>在屏幕上显示提示信息，duration 秒后自动消失</summary>
//    public void ShowMessage(string msg, float duration = -1f)
//    {
//        if (messageText == null) return;

//        messageText.text    = msg;
//        messageText.enabled = true;

//        if (messageClearCoroutine != null)
//            StopCoroutine(messageClearCoroutine);

//        float d = duration > 0f ? duration : messageDuration;
//        messageClearCoroutine = StartCoroutine(ClearMessageAfter(d));
//    }

//    // ── 事件回调 ───────────────────────────────────────────────

//    private void OnArkHpChanged(int current, int max)
//    {
//        RefreshArkHp(current, max);
//    }

//    private void OnFoodChanged(int current, int max)
//    {
//        RefreshFood(current, max);
//    }

//    private void OnTurnStart()
//    {
//        RefreshTurnUI();
//        RefreshMonsterUI();
//        SetEndTurnButtonInteractable(true);
//        ShowMessage($"第 {battleController.GetTurnNumber()} 回合开始");
//    }

//    private void OnTurnEnd()
//    {
//        SetEndTurnButtonInteractable(false);
//    }

//    private void OnBattleOver(bool victory)
//    {
//        SetEndTurnButtonInteractable(false);
//        ShowMessage(victory ? "✦ 战斗胜利！✦" : "✦ 方舟已毁… ✦", 5f);
//    }

//    private void OnEndTurnClicked()
//    {
//        battleController?.EndTurn();
//    }

//    // ── 局部刷新 ───────────────────────────────────────────────

//    private void RefreshArkHp(int current = -1, int max = -1)
//    {
//        if (arkHealthSystem == null) return;
//        int hp    = current >= 0 ? current : arkHealthSystem.GetCurrentHp();
//        int maxHp = max     >= 0 ? max     : arkHealthSystem.GetMaxHp();

//        if (arkHpText   != null) arkHpText.text     = $"方舟 HP: {hp} / {maxHp}";
//        if (arkHpSlider != null)
//        {
//            arkHpSlider.maxValue = maxHp;
//            arkHpSlider.value    = hp;
//        }
//    }

//    private void RefreshFood(int current = -1, int max = -1)
//    {
//        if (resourceManager == null) return;
//        int food    = current >= 0 ? current : resourceManager.GetFood();
//        int maxFood = max     >= 0 ? max     : resourceManager.GetMaxFood();

//        if (foodText   != null) foodText.text     = $"食物: {food} / {maxFood}";
//        if (foodSlider != null)
//        {
//            foodSlider.maxValue = maxFood;
//            foodSlider.value    = food;
//        }
//    }

//    private void RefreshTurnUI()
//    {
//        if (turnText != null && battleController != null)
//            turnText.text = $"回合: {battleController.GetTurnNumber()}";
//    }

//    private void RefreshHandUI()
//    {
//        if (handContainer == null || handCardPrefab == null) return;

//        // 清空旧卡牌 UI
//        ClearChildren(handContainer);

//        if (handManager == null) return;
//        List<AnimalCard> hand = handManager.GetHand();
//        foreach (AnimalCard card in hand)
//        {
//            GameObject go = Instantiate(handCardPrefab, handContainer);
//            SetupAnimalCardUI(go, card, isInHand: true);
//        }
//    }

//    private void RefreshFieldUI()
//    {
//        if (fieldContainer == null || fieldCardPrefab == null) return;

//        ClearChildren(fieldContainer);

//        if (fieldManager == null) return;
//        List<AnimalCard> field = fieldManager.GetField();
//        foreach (AnimalCard card in field)
//        {
//            GameObject go = Instantiate(fieldCardPrefab, fieldContainer);
//            SetupAnimalCardUI(go, card, isInHand: false);
//        }
//    }

//    private void RefreshMonsterUI()
//    {
//        if (monsterContainer == null || monsterPrefab == null) return;

//        ClearChildren(monsterContainer);

//        if (battleController == null) return;
//        List<Monster> monsters = battleController.GetCurrentMonsters();
//        foreach (Monster m in monsters)
//        {
//            GameObject go = Instantiate(monsterPrefab, monsterContainer);
//            SetupMonsterUI(go, m);
//        }
//    }

//    // ── UI 元素初始化 ──────────────────────────────────────────

//    /// <summary>
//    /// 初始化动物卡牌 UI。
//    /// 预制体中应包含：
//    ///   - "CardName"  (TextMeshProUGUI) → 名称
//    ///   - "CardHp"    (TextMeshProUGUI) → HP
//    ///   - "CardAtk"   (TextMeshProUGUI) → 攻击
//    ///   - "CardFood"  (TextMeshProUGUI) → 食物消耗
//    ///   - "PlayBtn"   (Button)          → 上场按钮（手牌时显示）
//    ///   - "RecallBtn" (Button)          → 撤回按钮（场上时显示）
//    /// </summary>
//    private void SetupAnimalCardUI(GameObject go, AnimalCard card, bool isInHand)
//    {
//        SetText(go, "CardName", card.CardName);
//        SetText(go, "CardHp",   $"HP: {card.CurrentHp}/{card.MaxHp}");
//        SetText(go, "CardAtk",  $"ATK: {card.Attack}");
//        SetText(go, "CardFood", $"食物: {card.FoodCost}");

//        // 手牌：显示「上场」按钮
//        Button playBtn   = FindButton(go, "PlayBtn");
//        Button recallBtn = FindButton(go, "RecallBtn");

//        if (playBtn != null)
//        {
//            playBtn.gameObject.SetActive(isInHand);
//            if (isInHand)
//            {
//                playBtn.interactable = resourceManager != null &&
//                                       resourceManager.HasEnoughFood(card.FoodCost);
//                AnimalCard captured = card;
//                playBtn.onClick.RemoveAllListeners();
//                playBtn.onClick.AddListener(() => OnPlayCardClicked(captured));
//            }
//        }

//        if (recallBtn != null)
//        {
//            recallBtn.gameObject.SetActive(!isInHand);
//            if (!isInHand)
//            {
//                AnimalCard captured = card;
//                recallBtn.onClick.RemoveAllListeners();
//                recallBtn.onClick.AddListener(() => OnRecallCardClicked(captured));
//            }
//        }
//    }

//    /// <summary>
//    /// 初始化怪物 UI。
//    /// 预制体中应包含：
//    ///   - "MonsterName" (TextMeshProUGUI) → 名称
//    ///   - "MonsterHp"   (TextMeshProUGUI) → HP
//    ///   - "MonsterAtk"  (TextMeshProUGUI) → 攻击
//    ///   - "HpSlider"    (Slider)          → HP 条
//    /// </summary>
//    private void SetupMonsterUI(GameObject go, Monster m)
//    {
//        SetText(go, "MonsterName", m.MonsterName);
//        SetText(go, "MonsterHp",  $"HP: {Mathf.Max(0, m.CurrentHp)}/{m.MaxHp}");
//        SetText(go, "MonsterAtk", $"ATK: {m.Attack}");

//        Slider hpBar = go.GetComponentInChildren<Slider>(true);
//        if (hpBar != null)
//        {
//            hpBar.maxValue = m.MaxHp;
//            hpBar.value    = Mathf.Max(0, m.CurrentHp);
//        }
//    }

//    // ── 按钮事件 ───────────────────────────────────────────────

//    private void OnPlayCardClicked(AnimalCard card)
//    {
//        if (fieldManager == null) return;
//        bool success = fieldManager.PlaceAnimal(card);
//        if (!success)
//            ShowMessage($"无法放置 {card.CardName}（食物不足或场地已满）");
//    }

//    private void OnRecallCardClicked(AnimalCard card)
//    {
//        fieldManager?.RecallAnimal(card);
//    }

//    // ── 辅助方法 ───────────────────────────────────────────────

//    private static void ClearChildren(Transform parent)
//    {
//        for (int i = parent.childCount - 1; i >= 0; i--)
//            Destroy(parent.GetChild(i).gameObject);
//    }

//    private static void SetText(GameObject go, string childName, string value)
//    {
//        Transform t = go.transform.Find(childName);
//        if (t == null) return;
//        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
//        if (tmp != null) tmp.text = value;
//    }

//    private static Button FindButton(GameObject go, string childName)
//    {
//        Transform t = go.transform.Find(childName);
//        return t != null ? t.GetComponent<Button>() : null;
//    }

//    private void SetEndTurnButtonInteractable(bool interactable)
//    {
//        if (endTurnButton != null)
//            endTurnButton.interactable = interactable;
//    }

//    private System.Collections.IEnumerator ClearMessageAfter(float seconds)
//    {
//        yield return new WaitForSeconds(seconds);
//        if (messageText != null) messageText.enabled = false;
//        messageClearCoroutine = null;
//    }
//}
