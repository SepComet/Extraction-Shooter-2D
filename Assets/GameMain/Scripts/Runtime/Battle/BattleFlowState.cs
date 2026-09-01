namespace SepCore.Battle
{
    /// <summary>
    /// 战斗流程状态。
    /// </summary>
    public enum BattleFlowState
    {
        /// <summary>
        /// 等待当前玩家行动者提交指令。
        /// </summary>
        AwaitingPlayerCommand,

        /// <summary>
        /// 战斗完成，结果已产出。
        /// </summary>
        Completed,
    }
}