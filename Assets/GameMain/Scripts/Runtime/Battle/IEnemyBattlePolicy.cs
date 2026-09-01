using SepCore.CustomComponent;

namespace SepCore.Battle
{
    /// <summary>
    /// 敌人策略接口。
    /// 策略只读取快照并返回指令，不直接修改 BattleSession。
    /// 当前没有可执行行动时，本次行动机会记为跳过，不能形成无限循环。
    /// </summary>
    public interface IEnemyBattlePolicy
    {
        /// <summary>
        /// 为当前敌人行动者决定指令。
        /// </summary>
        /// <param name="snapshot">只读战斗快照。</param>
        /// <param name="actorUnitId">当前获得行动机会的敌人单位。</param>
        /// <param name="random">本局共享随机源。</param>
        BattleCommand Decide(BattleSnapshot snapshot, int actorUnitId, IRunRandomSource random);
    }
}