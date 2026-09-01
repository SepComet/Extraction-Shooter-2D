namespace SepCore.Battle
{
    /// <summary>
    /// 探索层恢复接口。
    /// 碰撞方主动把 BattleEncounter 交给 RunBattleCoordinator；协调器不扫描场景寻找敌人。
    /// ApplyBattleReturn 必须按 EncounterId 精确操作触发敌人，不能按 EnemyConfigId 批量处理同类敌人。
    /// </summary>
    public interface IExplorationBattleBridge
    {
        /// <summary>
        /// 执行探索恢复计划。
        /// </summary>
        /// <param name="plan">由 RunBattleCoordinator 生成的恢复计划。</param>
        void ApplyBattleReturn(BattleReturnPlan plan);
    }
}