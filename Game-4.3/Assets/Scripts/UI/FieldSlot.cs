using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 场上单个槽位。
/// 在 Inspector 中设置 slotIndex（0-3），并拖拽引用 FieldManager / ResourceManager。
/// 需要在 GameObject 上同时挂一个 Image（作为射线检测目标）。
/// </summary>
[RequireComponent(typeof(Image))]
public class FieldSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("槽位编号 (0-3)")]
    [SerializeField] private int slotIndex = 0;

    [Header("依赖")]
    [SerializeField] private FieldManager    fieldManager;
    [SerializeField] private ResourceManager resourceManager;

    [Header("视觉反馈")]
    [SerializeField] private bool showVisualFeedback = false;
    [SerializeField] private Color normalColor    = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color hoverColor     = new Color(0.4f, 1f, 0.4f, 0.35f);
    [SerializeField] private Color occupiedColor  = new Color(0.6f, 0.6f, 0.6f, 0.25f);

    /// <summary>当前占据本槽位的动物卡（null = 空槽）</summary>
    public AnimalCard OccupiedCard { get; private set; }

    /// <summary>当前吸附在本槽位的卡牌 GameObject</summary>
    private GameObject occupiedCardGO;

    private Image bgImage;

    private void Awake()
    {
        bgImage = GetComponent<Image>();

        // 自动查找依赖
        if (fieldManager == null)
            fieldManager = FindObjectOfType<FieldManager>();
        if (resourceManager == null)
            resourceManager = FindObjectOfType<ResourceManager>();
    }
    
    /// <summary>
    /// 更新槽位中卡牌的显示（血量、攻击力）
    /// </summary>
    public void UpdateCardDisplay()
    {
        if (occupiedCardGO == null || OccupiedCard == null) return;
    
        // 更新血量显示
        SetCardText(occupiedCardGO, "CardHp", $"{OccupiedCard.CurrentHp}/{OccupiedCard.MaxHp}");
    
        // 更新攻击力显示
        SetCardText(occupiedCardGO, "CardAtk", $"{OccupiedCard.Attack}");
    }

    /// <summary>
    /// 辅助方法：设置卡牌上的文本
    /// </summary>
    private void SetCardText(GameObject cardGO, string childName, string value)
    {
        Transform child = cardGO.transform.Find(childName);
        if (child == null) return;
    
        TMPro.TextMeshProUGUI tmp = child.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = value;
    }

    private void OnEnable()
    {
        FieldManager.OnFieldChanged += OnFieldChanged;
    }

    private void OnDisable()
    {
        FieldManager.OnFieldChanged -= OnFieldChanged;
    }

    private void Start()
    {
        RefreshVisual();
    }

    private void OnFieldChanged()
    {
        if (fieldManager == null) return;

        AnimalCard current = fieldManager.GetSlot(slotIndex);

        // 槽位数据已清空，但 GO 还在 → 销毁
        if (current == null && occupiedCardGO != null)
        {
            Destroy(occupiedCardGO);
            occupiedCardGO = null;
            OccupiedCard   = null;
        }
        else
        {
            OccupiedCard = current;
        }

        RefreshVisual();
    }

    // ── 拖放接口 ───────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableCard draggable = eventData.pointerDrag.GetComponent<DraggableCard>();
        if (draggable == null || draggable.Card == null) return;

        // 槽位已被占用
        if (OccupiedCard != null)
        {
            Debug.LogWarning($"[FieldSlot] 槽位 {slotIndex} 已有 {OccupiedCard.CardName}，无法放置");
            RefreshVisual();
            return;
        }

        // 尝试放置（数据层）
        AnimalCard card = draggable.Card;
        bool success = fieldManager != null && fieldManager.PlaceAnimalInSlot(card, slotIndex);

        if (success)
        {
            draggable.WasPlaced = true;
            OccupiedCard = card;

            // ── 吸附卡牌 GameObject 到本槽位 ──────────────────
            GameObject cardGO = draggable.gameObject;
            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            RectTransform slotRect = GetComponent<RectTransform>();

            // 设为槽位子物体，居中对齐
            cardGO.transform.SetParent(slotRect, false);
            cardRect.anchorMin        = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax        = new Vector2(0.5f, 0.5f);
            cardRect.pivot            = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.localRotation    = Quaternion.identity;
            cardRect.SetAsFirstSibling(); // 放在槽位背景图之上

            // 禁用拖拽（场上的卡不能再拖走）
            draggable.enabled = false;

            // 恢复完全不透明
            if (cardGO.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 1f;

            occupiedCardGO = cardGO;
            Debug.Log($"[FieldSlot] {card.CardName} 吸附到槽位 {slotIndex}");
        }
        else
        {
            Debug.LogWarning($"[FieldSlot] 放置 {card.CardName} 到槽位 {slotIndex} 失败（食物不足/场地满/已占用）");
        }

        RefreshVisual();
    }

    // ── 悬停高亮 ──────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showVisualFeedback && OccupiedCard == null && bgImage != null)
            bgImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RefreshVisual();
    }

    // ── 公开方法 ───────────────────────────────────────────────

    /// <summary>清空槽位（动物死亡或被撤回时调用）</summary>
    public void ClearSlot()
    {
        OccupiedCard = null;
        RefreshVisual();
    }

    /// <summary>从外部同步占用状态（BattleUI 刷新时调用）</summary>
    /// <summary>从外部同步占用状态（BattleUI 刷新时调用）</summary>
    public void SetOccupied(AnimalCard card)
    {
        OccupiedCard = card;
    
        // 注意：SetOccupied 时 occupiedCardGO 可能为 null（如果是通过数据同步）
        // 这种情况下不需要更新 UI，因为卡牌 GameObject 可能还没有被创建
    
        RefreshVisual();
    }

    public int SlotIndex => slotIndex;

    // ── 内部方法 ───────────────────────────────────────────────

    private void RefreshVisual()
    {
        if (bgImage == null) return;
        bgImage.color = showVisualFeedback
            ? (OccupiedCard != null ? occupiedColor : normalColor)
            : Color.clear;
    }
}
