using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 挂在手牌 GameObject 上，使其可以被拖拽到 FieldSlot。
/// 拖拽期间卡牌跟随鼠标移动，松手时由 FieldSlot 决定是否接收。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DraggableCard : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ── 绑定的卡牌数据 ──────────────────────────────────────────
    public AnimalCard Card { get; private set; }

    // ── 内部状态 ───────────────────────────────────────────────
    private RectTransform rectTransform;
    private Canvas        rootCanvas;
    private CanvasGroup   canvasGroup;
    private Transform     originalParent;
    private int           originalSiblingIndex;
    private Vector2       originalAnchoredPosition;
    private Quaternion    originalRotation;

    /// <summary>拖拽结束后是否成功放置（由 FieldSlot 设置为 true）</summary>
    [HideInInspector] public bool WasPlaced = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // 向上找根 Canvas
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.rootCanvas;

        // 保证有 CanvasGroup（用于穿透射线检测）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>由 BattleUI 在生成手牌时调用，绑定卡牌数据</summary>
    public void Initialize(AnimalCard card)
    {
        Card = card;
    }

    // ── 拖拽事件 ───────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        WasPlaced = false;

        // 记录初始状态，用于拖拽失败时复位
        originalParent         = rectTransform.parent;
        originalSiblingIndex   = rectTransform.GetSiblingIndex();
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalRotation       = rectTransform.localRotation;

        // 移到根 Canvas 最顶层，保证拖拽时显示在其他 UI 之上
        if (rootCanvas != null)
            rectTransform.SetParent(rootCanvas.transform, true);

        rectTransform.SetAsLastSibling();

        // 让射线穿透此卡，这样 FieldSlot 才能收到 OnDrop
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) return;

        // 将屏幕坐标转为 Canvas 内的局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!WasPlaced)
        {
            // 放置失败，复位到手牌区域
            rectTransform.SetParent(originalParent, false);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.localRotation    = originalRotation;
        }
        // 放置成功时，BattleUI 会在 FieldManager.OnFieldChanged 中重建 UI，此 GameObject 将被销毁
    }
}
