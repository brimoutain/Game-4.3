//using UnityEngine;

///// <summary>
///// 资源管理：食物槽、初始3食物、消耗检查、击败获得食物
///// 依赖：GameConfig
///// </summary>
//public class ResourceManager : MonoBehaviour
//{
//    // ── 单例 ──────────────────────────────────────────────────
//    public static ResourceManager Instance { get; private set; }

//    // ── 外部依赖 ───────────────────────────────────────────────
//    [Header("依赖组件")]
//    [SerializeField] private GameConfig gameConfig;

//    // ── 状态 ──────────────────────────────────────────────────
//    private int currentFood;
//    private int maxFood;

//    // ── 事件 ──────────────────────────────────────────────────
//    /// <summary>食物数量变化时触发，参数为 (当前食物, 最大食物)</summary>
//    public static event System.Action<int, int> OnFoodChanged;

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

//    private void Start()
//    {
//        ResetFood();
//    }

//    // ── 公开接口 ───────────────────────────────────────────────

//    /// <summary>重置为初始食物（每场战斗开始时调用）</summary>
//    public void ResetFood()
//    {
//        int initial = gameConfig != null ? gameConfig.InitialFood : 3;
//        maxFood     = gameConfig != null ? gameConfig.MaxFood     : 10;
//        currentFood = initial;
//        Debug.Log($"[ResourceManager] 食物重置：{currentFood}/{maxFood}");
//        OnFoodChanged?.Invoke(currentFood, maxFood);
//    }

//    /// <summary>
//    /// 消耗指定数量的食物。
//    /// 食物充足返回 true；不足返回 false，食物不减少。
//    /// </summary>
//    public bool ConsumeFood(int amount)
//    {
//        if (amount <= 0) return true;

//        if (currentFood < amount)
//        {
//            Debug.LogWarning($"[ResourceManager] 食物不足：需要 {amount}，当前 {currentFood}");
//            return false;
//        }

//        currentFood -= amount;
//        Debug.Log($"[ResourceManager] 消耗食物 {amount}，剩余 {currentFood}/{maxFood}");
//        OnFoodChanged?.Invoke(currentFood, maxFood);
//        return true;
//    }

//    /// <summary>增加食物（击败怪物或其他途径）</summary>
//    public void AddFood(int amount)
//    {
//        if (amount <= 0) return;

//        currentFood = Mathf.Min(currentFood + amount, maxFood);
//        Debug.Log($"[ResourceManager] 获得食物 {amount}，当前 {currentFood}/{maxFood}");
//        OnFoodChanged?.Invoke(currentFood, maxFood);
//    }

//    /// <summary>获取当前食物数量</summary>
//    public int GetFood() => currentFood;

//    /// <summary>获取食物上限</summary>
//    public int GetMaxFood() => maxFood;

//    /// <summary>检查是否有足够食物（不消耗）</summary>
//    public bool HasEnoughFood(int amount) => currentFood >= amount;
//}
