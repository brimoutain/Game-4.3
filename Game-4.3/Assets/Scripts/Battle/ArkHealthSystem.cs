using UnityEngine;

/// <summary>
/// 方舟血量系统：扣血、归零判定、战斗间不恢复
/// 依赖：GameConfig
/// </summary>
public class ArkHealthSystem : MonoBehaviour
{
    // ── 单例 ──────────────────────────────────────────────────
    public static ArkHealthSystem Instance { get; private set; }

    // ── 外部依赖 ───────────────────────────────────────────────
    [Header("依赖组件")]
    [SerializeField] private GameConfig gameConfig;

    // ── 状态 ──────────────────────────────────────────────────
    private int currentHp;
    private int maxHp;

    // ── 事件 ──────────────────────────────────────────────────
    public static event System.Action<int, int> OnHpChanged; // (current, max)
    public static event System.Action           OnArkDestroyed;

    // ── 生命周期 ───────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Start 只在对象第一次生成时跑一次；换场景时本体会保留，不会再次初始化，血量在关卡间延续
        InitializeHp();
    }

    // ── 公开接口 ───────────────────────────────────────────────

    /// <summary>使用 GameConfig 的初始值重置血量（仅在游戏开始时调用）</summary>
    public void InitializeHp()
    {
        maxHp     = gameConfig != null ? gameConfig.startingArkHp : 30;
        currentHp = maxHp;
        Debug.Log($"[ArkHealthSystem] 方舟初始化，HP = {currentHp}/{maxHp}");
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    /// <summary>方舟承受伤害（不可为负）</summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        Debug.Log($"[ArkHealthSystem] 方舟受到 {amount} 点伤害，剩余 HP = {currentHp}/{maxHp}");
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
        {
            Debug.Log("[ArkHealthSystem] 方舟已毁！");
            OnArkDestroyed?.Invoke();
        }
    }

    /// <summary>获取当前血量</summary>
    public int GetCurrentHp() => currentHp;

    /// <summary>获取最大血量</summary>
    public int GetMaxHp() => maxHp;

    /// <summary>方舟是否已被摧毁</summary>
    public bool IsDead() => currentHp <= 0;

    // ── 说明：战斗之间不恢复血量 ──────────────────────────────
    // 本类故意不提供 Heal / Restore 接口，以保证战斗间血量不恢复的设计。
    // 若后续需要特殊道具回血，可在此添加带权限检查的 HealByItem() 方法。
}
