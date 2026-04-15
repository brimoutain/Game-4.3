using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FieldSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("槽位编号 (0-3)")]
    [SerializeField] private int slotIndex = 0;

    [Header("视觉反馈")]
    [SerializeField] private bool showVisualFeedback = false;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color hoverColor = new Color(0.4f, 1f, 0.4f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(0.6f, 0.6f, 0.6f, 0.25f);

    public AnimalCard OccupiedCard { get; private set; }
    private GameObject occupiedCardGO;
    private Image bgImage;
    
    // 延迟获取的标志
    private bool isInitialized = false;

    private void Awake()
    {
        bgImage = GetComponent<Image>();
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

    // 确保单例存在的方法
    private bool EnsureManagers()
    {
        if (FieldManager.Instance == null)
        {
            Debug.LogWarning($"[FieldSlot] FieldManager.Instance 尚未初始化，等待...");
            return false;
        }
        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning($"[FieldSlot] ResourceManager.Instance 尚未初始化，等待...");
            return false;
        }
        return true;
    }

    private void OnFieldChanged()
    {
        if (!EnsureManagers()) return;

        AnimalCard current = FieldManager.Instance.GetSlot(slotIndex);

        if (current == null && occupiedCardGO != null)
        {
            Destroy(occupiedCardGO);
            occupiedCardGO = null;
            OccupiedCard = null;
        }
        else
        {
            OccupiedCard = current;
        }

        RefreshVisual();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!EnsureManagers()) return;
        
        if (eventData.pointerDrag == null) return;

        DraggableCard draggable = eventData.pointerDrag.GetComponent<DraggableCard>();
        if (draggable == null || draggable.Card == null) return;

        if (OccupiedCard != null)
        {
            Debug.LogWarning($"[FieldSlot] 槽位 {slotIndex} 已有 {OccupiedCard.CardName}");
            RefreshVisual();
            return;
        }

        AnimalCard card = draggable.Card;
        bool success = FieldManager.Instance.PlaceAnimalInSlot(card, slotIndex);

        if (success)
        {
            draggable.WasPlaced = true;
            OccupiedCard = card;

            GameObject cardGO = draggable.gameObject;
            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            RectTransform slotRect = GetComponent<RectTransform>();

            cardGO.transform.SetParent(slotRect, false);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.localRotation = Quaternion.identity;
            cardRect.SetAsFirstSibling();

            draggable.enabled = false;

            if (cardGO.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 1f;

            occupiedCardGO = cardGO;
            Debug.Log($"[FieldSlot] {card.CardName} 吸附到槽位 {slotIndex}");
            
            UpdateCardDisplay();
        }
        else
        {
            Debug.LogWarning($"[FieldSlot] 放置 {card.CardName} 到槽位 {slotIndex} 失败");
        }

        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showVisualFeedback && OccupiedCard == null && bgImage != null)
            bgImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RefreshVisual();
    }

    public void ClearSlot()
    {
        OccupiedCard = null;
        RefreshVisual();
    }

    public void SetOccupied(AnimalCard card)
    {
        OccupiedCard = card;
        RefreshVisual();
    }

    public int SlotIndex => slotIndex;

    public void UpdateCardDisplay()
    {
        if (occupiedCardGO == null || OccupiedCard == null) return;
    
        SetCardText(occupiedCardGO, "CardHp", $"{OccupiedCard.CurrentHp}/{OccupiedCard.MaxHp}");
        SetCardText(occupiedCardGO, "CardAtk", $"{OccupiedCard.Attack}");
    }

    private void RefreshVisual()
    {
        if (bgImage == null) return;
        bgImage.color = showVisualFeedback
            ? (OccupiedCard != null ? occupiedColor : normalColor)
            : Color.clear;
    }

    private void SetCardText(GameObject cardGO, string childName, string value)
    {
        Transform child = cardGO.transform.Find(childName);
        if (child == null) return;
    
        TMPro.TextMeshProUGUI tmp = child.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = value;
    }
}