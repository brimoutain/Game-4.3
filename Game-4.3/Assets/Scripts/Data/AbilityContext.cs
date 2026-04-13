/// <summary>
/// 技能触发时的上下文，包含技能运行所需的所有引用。
/// 由 CombatCalculator 在调用 AnimalCard 钩子时构建并传入。
/// </summary>
public class AbilityContext
{
    /// <summary>使用技能的动物卡</summary>
    public AnimalCard Self;

    /// <summary>当前对位的怪物（可为 null，例如 OnPlay / OnDeath 时可能没有对手）</summary>
    public Monster Target;

    /// <summary>场上槽位管理</summary>
    public FieldManager FieldManager;

    /// <summary>方舟血量系统</summary>
    public ArkHealthSystem Ark;

    /// <summary>食物资源管理</summary>
    public ResourceManager Resource;

    /// <summary>自身所在槽位索引（0-3）</summary>
    public int SlotIndex;

    public AbilityContext(
        AnimalCard self,
        Monster target,
        FieldManager fieldManager,
        ArkHealthSystem ark,
        ResourceManager resource,
        int slotIndex)
    {
        Self         = self;
        Target       = target;
        FieldManager = fieldManager;
        Ark          = ark;
        Resource     = resource;
        SlotIndex    = slotIndex;
    }
}
