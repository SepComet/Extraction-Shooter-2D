using System.Collections.Generic;
using SepCore.Definition;

namespace SepCore.Battle
{
    /// <summary>
    /// 单位剩余状态快照。
    /// </summary>
    public sealed class BattleStatusSnapshot
    {
        /// <summary>
        /// 状态类型。
        /// </summary>
        public readonly BattleStatusType StatusType;

        /// <summary>
        /// 剩余持续次数（行动机会）；重复施加取较大值，移除后为 0。
        /// </summary>
        public readonly int RemainingRounds;

        public BattleStatusSnapshot(BattleStatusType statusType, int remainingRounds)
        {
            StatusType = statusType;
            RemainingRounds = remainingRounds;
        }
    }

    /// <summary>
    /// 单个战斗单位的只读视图。
    /// </summary>
    public sealed class BattleUnitSnapshot
    {
        /// <summary>
        /// 本场战斗单位的唯一运行时标识；重复配置的敌人拥有不同 ID。
        /// </summary>
        public readonly int BattleUnitId;

        /// <summary>
        /// 阵营。
        /// </summary>
        public readonly BattleFaction Faction;

        /// <summary>
        /// 配置标识：玩家为 CharacterId，敌人为 EnemyConfigId。
        /// </summary>
        public readonly int ConfigId;

        /// <summary>
        /// 同阵营同速度的最终并列规则顺序。
        /// </summary>
        public readonly int PartyOrder;

        /// <summary>
        /// 当前 HP。
        /// </summary>
        public readonly int CurrentHp;

        /// <summary>
        /// HP 上限。
        /// </summary>
        public readonly int MaxHp;

        /// <summary>
        /// 当前 MP。
        /// </summary>
        public readonly int CurrentMp;

        /// <summary>
        /// MP 上限。
        /// </summary>
        public readonly int MaxMp;

        /// <summary>
        /// 攻击。
        /// </summary>
        public readonly int Atk;

        /// <summary>
        /// 魔力。
        /// </summary>
        public readonly int Mat;

        /// <summary>
        /// 当前速度。
        /// </summary>
        public readonly int Speed;

        /// <summary>
        /// 是否阵亡。
        /// </summary>
        public readonly bool IsDefeated;

        /// <summary>
        /// 是否已逃跑。
        /// </summary>
        public readonly bool IsEscaped;

        /// <summary>
        /// 剩余状态列表。
        /// </summary>
        public readonly IReadOnlyList<BattleStatusSnapshot> Statuses;

        /// <summary>
        /// 可用行动配置 ID 列表。
        /// </summary>
        public readonly IReadOnlyList<int> AvailableActionIds;

        public BattleUnitSnapshot(int battleUnitId, BattleFaction faction, int configId, int partyOrder,
            int currentHp, int maxHp, int currentMp, int maxMp, int atk, int mat, int speed,
            bool isDefeated, bool isEscaped, IReadOnlyList<BattleStatusSnapshot> statuses,
            IReadOnlyList<int> availableActionIds)
        {
            BattleUnitId = battleUnitId;
            Faction = faction;
            ConfigId = configId;
            PartyOrder = partyOrder;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            CurrentMp = currentMp;
            MaxMp = maxMp;
            Atk = atk;
            Mat = mat;
            Speed = speed;
            IsDefeated = isDefeated;
            IsEscaped = isEscaped;
            Statuses = statuses != null ? Copy(statuses) : new BattleStatusSnapshot[0];
            AvailableActionIds = availableActionIds != null ? Copy(availableActionIds) : new int[0];
        }

        private static BattleStatusSnapshot[] Copy(IReadOnlyList<BattleStatusSnapshot> source)
        {
            BattleStatusSnapshot[] copy = new BattleStatusSnapshot[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static int[] Copy(IReadOnlyList<int> source)
        {
            int[] copy = new int[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }

    /// <summary>
    /// 只读战斗快照，是 UI 和敌人策略唯一允许读取的战斗状态。
    /// 每次推进都创建逻辑上的只读快照，不把可修改的内部列表或战斗单位引用暴露给调用方。
    /// </summary>
    public sealed class BattleSnapshot
    {
        /// <summary>
        /// 当前轮次，从 1 开始。
        /// </summary>
        public readonly int RoundNumber;

        /// <summary>
        /// 当前是否为先制战斗第一轮。
        /// </summary>
        public readonly bool IsPreemptiveRound;

        /// <summary>
        /// 当前行动者；战斗完成时为 0。
        /// </summary>
        public readonly int CurrentActorUnitId;

        /// <summary>
        /// 全部单位的只读视图，保持稳定显示顺序。
        /// </summary>
        public readonly IReadOnlyList<BattleUnitSnapshot> Units;

        /// <summary>
        /// 本轮从当前行动者开始的剩余运行时单位 ID；速度变化后重建。
        /// </summary>
        public readonly IReadOnlyList<int> RemainingTurnOrder;

        /// <summary>
        /// 流程状态。
        /// </summary>
        public readonly BattleFlowState FlowState;

        public BattleSnapshot(int roundNumber, bool isPreemptiveRound, int currentActorUnitId,
            IReadOnlyList<BattleUnitSnapshot> units, IReadOnlyList<int> remainingTurnOrder,
            BattleFlowState flowState)
        {
            RoundNumber = roundNumber;
            IsPreemptiveRound = isPreemptiveRound;
            CurrentActorUnitId = currentActorUnitId;
            Units = units != null ? Copy(units) : new BattleUnitSnapshot[0];
            RemainingTurnOrder = remainingTurnOrder != null ? Copy(remainingTurnOrder) : new int[0];
            FlowState = flowState;
        }

        private static BattleUnitSnapshot[] Copy(IReadOnlyList<BattleUnitSnapshot> source)
        {
            BattleUnitSnapshot[] copy = new BattleUnitSnapshot[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static int[] Copy(IReadOnlyList<int> source)
        {
            int[] copy = new int[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}