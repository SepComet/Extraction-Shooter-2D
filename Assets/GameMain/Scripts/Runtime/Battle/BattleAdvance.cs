using System.Collections.Generic;
using SepCore.Definition;

namespace SepCore.Battle
{
    /// <summary>
    /// 单个效果对单个目标产生的数值或状态变化。
    /// </summary>
    public sealed class BattleActionEffectRecord
    {
        /// <summary>
        /// 目标单位。
        /// </summary>
        public readonly int TargetUnitId;

        /// <summary>
        /// 效果结算前的 HP。
        /// </summary>
        public readonly int BeforeHp;

        /// <summary>
        /// 效果结算后的 HP。
        /// </summary>
        public readonly int AfterHp;

        /// <summary>
        /// 效果结算前的 MP。
        /// </summary>
        public readonly int BeforeMp;

        /// <summary>
        /// 效果结算后的 MP。
        /// </summary>
        public readonly int AfterMp;

        /// <summary>
        /// 状态变化列表；无状态变化时为空。
        /// </summary>
        public readonly IReadOnlyList<BattleStatusChangeRecord> StatusChanges;

        public BattleActionEffectRecord(int targetUnitId, int beforeHp, int afterHp, int beforeMp, int afterMp,
            IReadOnlyList<BattleStatusChangeRecord> statusChanges)
        {
            TargetUnitId = targetUnitId;
            BeforeHp = beforeHp;
            AfterHp = afterHp;
            BeforeMp = beforeMp;
            AfterMp = afterMp;
            StatusChanges = statusChanges != null ? Copy(statusChanges) : new BattleStatusChangeRecord[0];
        }

        private static BattleStatusChangeRecord[] Copy(IReadOnlyList<BattleStatusChangeRecord> source)
        {
            BattleStatusChangeRecord[] copy = new BattleStatusChangeRecord[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    /// <summary>
    /// 状态变化记录。
    /// </summary>
    public sealed class BattleStatusChangeRecord
    {
        /// <summary>
        /// 状态类型。
        /// </summary>
        public readonly BattleStatusType StatusType;

        /// <summary>
        /// 变化后的剩余持续次数；状态被移除时为 0。
        /// </summary>
        public readonly int RemainingRounds;

        public BattleStatusChangeRecord(BattleStatusType statusType, int remainingRounds)
        {
            StatusType = statusType;
            RemainingRounds = remainingRounds;
        }
    }

    /// <summary>
    /// 一次行动产生的有序行动记录。
    /// 首版 UI 立即消费并显示；未来动画系统可以逐条等待播放，不需要修改战斗规则内核。
    /// </summary>
    public sealed class BattleActionRecord
    {
        /// <summary>
        /// 行动者。
        /// </summary>
        public readonly int ActorUnitId;

        /// <summary>
        /// 指令类型，对应配表 BattleActionType。
        /// </summary>
        public readonly BattleActionType CommandType;

        /// <summary>
        /// 攻击或技能配置 ID；逃跑为 0。
        /// </summary>
        public readonly int ActionConfigId;

        /// <summary>
        /// 按结算顺序排列的效果记录。
        /// </summary>
        public readonly IReadOnlyList<BattleActionEffectRecord> Effects;

        public BattleActionRecord(int actorUnitId, BattleActionType commandType, int actionConfigId,
            IReadOnlyList<BattleActionEffectRecord> effects)
        {
            ActorUnitId = actorUnitId;
            CommandType = commandType;
            ActionConfigId = actionConfigId;
            Effects = effects != null ? Copy(effects) : new BattleActionEffectRecord[0];
        }

        private static BattleActionEffectRecord[] Copy(IReadOnlyList<BattleActionEffectRecord> source)
        {
            BattleActionEffectRecord[] copy = new BattleActionEffectRecord[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    /// <summary>
    /// 战斗推进返回值。
    /// </summary>
    public sealed class BattleAdvance
    {
        /// <summary>
        /// 本次推进产生的有序行动记录列表。
        /// </summary>
        public readonly IReadOnlyList<BattleActionRecord> Records;

        /// <summary>
        /// 推进结束后的只读战斗快照。
        /// </summary>
        public readonly BattleSnapshot Snapshot;

        /// <summary>
        /// 流程状态。
        /// </summary>
        public readonly BattleFlowState FlowState;

        /// <summary>
        /// 仅在 Completed 时存在，否则为 null。
        /// </summary>
        public readonly BattleResult Result;

        public BattleAdvance(IReadOnlyList<BattleActionRecord> records, BattleSnapshot snapshot,
            BattleFlowState flowState, BattleResult result)
        {
            Records = records != null ? Copy(records) : new BattleActionRecord[0];
            Snapshot = snapshot;
            FlowState = flowState;
            Result = result;
        }

        private static BattleActionRecord[] Copy(IReadOnlyList<BattleActionRecord> source)
        {
            BattleActionRecord[] copy = new BattleActionRecord[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}