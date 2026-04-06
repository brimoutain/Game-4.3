//using System;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 战斗总控：战斗初始化、回合切换、胜利/失败判定
///// 依赖：CombatCalculator, ArkHealthSystem
///// </summary>
//public class BattleController : MonoBehaviour
//{
//    // ── 单例 ──────────────────────────────────────────────────
//    public static BattleController Instance { get; private set; }

//    // ── 外部依赖 ───────────────────────────────────────────────
//    [Header("依赖组件")]
//    [SerializeField] private CombatCalculator combatCalculator;
//    [SerializeField] private ArkHealthSystem  arkHealthSystem;
//    [SerializeField] private HandManager      handManager;
//    [SerializeField] private FieldManager     fieldManager;
//    [SerializeField] private ResourceManager  resourceManager;

//    // ── 状态 ──────────────────────────────────────────────────
//    private List<Monster> currentMonsters = new List<Monster>();
//    private int  turnNumber   = 0;
//    private bool battleActive = false;

//    // ── 事件（供 BattleUI 等订阅）─────────────────────────────
//    public static event Action          OnTurnStart;
//    public static event Action          OnTurnEnd;
//    public static event Action<bool>    OnBattleOver;   // true = 胜利

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

//    // ── 公开接口 ───────────────────────────────────────────────

//    /// <summary>开始一场战斗，传入本关卡的怪物列表</summary>
//    public void StartBattle(List<Monster> monsters)
//    {
//        if (monsters == null || monsters.Count == 0)
//        {
//            Debug.LogWarning("[BattleController] StartBattle: 怪物列表为空！");
//            return;
//        }

//        currentMonsters = new List<Monster>(monsters);
//        turnNumber      = 0;
//        battleActive    = true;

//        Debug.Log($"[BattleController] 战斗开始，共 {currentMonsters.Count} 只怪物");

//        // 初始化各子系统
//        resourceManager.ResetFood();
//        handManager.DrawInitialHand();

//        StartTurn();
//    }

//    /// <summary>结束当前回合（由玩家/UI 调用）</summary>
//    public void EndTurn()
//    {
//        if (!battleActive) return;

//        Debug.Log($"[BattleController] 回合 {turnNumber} 结束");
//        OnTurnEnd?.Invoke();

//        // 执行战斗结算
//        bool battleContinues = combatCalculator.ResolveCombat(currentMonsters);

//        // 检查方舟是否存活
//        if (arkHealthSystem.IsDead())
//        {
//            EndBattle(false);
//            return;
//        }

//        // 检查所有怪物是否已被击败
//        currentMonsters.RemoveAll(m => m.CurrentHp <= 0);
//        if (currentMonsters.Count == 0)
//        {
//            EndBattle(true);
//            return;
//        }

//        if (!battleContinues)
//        {
//            EndBattle(false);
//            return;
//        }

//        // 继续下一回合
//        StartTurn();
//    }

//    /// <summary>获取当前场上怪物列表（只读副本）</summary>
//    public List<Monster> GetCurrentMonsters()
//    {
//        return new List<Monster>(currentMonsters);
//    }

//    /// <summary>当前回合编号（从 1 开始）</summary>
//    public int GetTurnNumber() => turnNumber;

//    /// <summary>战斗是否进行中</summary>
//    public bool IsBattleActive() => battleActive;

//    // ── 内部逻辑 ───────────────────────────────────────────────

//    private void StartTurn()
//    {
//        turnNumber++;
//        Debug.Log($"[BattleController] 第 {turnNumber} 回合开始");

//        // 回合开始抽牌
//        handManager.DrawForTurn();

//        OnTurnStart?.Invoke();
//    }

//    private void EndBattle(bool victory)
//    {
//        battleActive = false;
//        string result = victory ? "胜利" : "失败";
//        Debug.Log($"[BattleController] 战斗结束 —— {result}");

//        // 回合结束弃牌
//        handManager.DiscardHand();

//        OnBattleOver?.Invoke(victory);
//    }
//}
