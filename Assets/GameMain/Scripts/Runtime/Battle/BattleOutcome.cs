namespace SepCore.Battle
{
    /// <summary>
    /// 战斗结束结果，四种结果互斥。
    /// </summary>
    public enum BattleOutcome
    {
        /// <summary>
        /// 所有敌人阵亡。
        /// </summary>
        Victory,

        /// <summary>
        /// 所有玩家成功逃跑，且没有玩家阵亡。
        /// </summary>
        AllEscaped,

        /// <summary>
        /// 至少一名玩家逃跑，其余仍在战斗中的玩家全部阵亡。
        /// </summary>
        PartialEscapeDefeat,

        /// <summary>
        /// 所有玩家阵亡，没有任何玩家成功逃跑。
        /// </summary>
        TotalDefeat,
    }
}