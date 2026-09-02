using SepCore.Definition;

namespace SepCore.Battle
{
    /// <summary>
    /// 单场战斗中的状态，只存在于 BattleRuntime。
    /// 同类状态不叠层，重复施加保留较长的剩余行动机会次数。
    /// </summary>
    internal sealed class BattleStatus
    {
        /// <summary>
        /// 状态类型。
        /// </summary>
        public BattleStatusType Type;

        /// <summary>
        /// 剩余持续次数（行动机会）。
        /// </summary>
        public int RemainingRounds;

        public BattleStatus(BattleStatusType type, int remainingRounds)
        {
            Type = type;
            RemainingRounds = remainingRounds;
        }
    }
}
