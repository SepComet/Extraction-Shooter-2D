using System.Collections.Generic;

namespace SepCore.Battle
{
    /// <summary>
    /// 探索恢复计划，由 RunBattleCoordinator 根据战斗结果统一生成，探索层只执行计划。
    /// 探索层不得再次根据玩家 HP 或逃跑数量重判战斗结果。
    /// </summary>
    public sealed class BattleReturnPlan
    {
        /// <summary>
        /// 关联原地图敌人实例。
        /// </summary>
        public readonly int EncounterId;

        /// <summary>
        /// 战斗结果。
        /// </summary>
        public readonly BattleOutcome Outcome;

        /// <summary>
        /// 回写后的玩家临时状态（含 1/1 恢复规则应用后的结果）。
        /// </summary>
        public readonly IReadOnlyList<BattlePlayerResult> Players;

        /// <summary>
        /// 是否移除触发敌人（胜利）。
        /// </summary>
        public readonly bool RemoveEncounterEnemy;

        /// <summary>
        /// 是否保留并恢复触发敌人，下次按完整预设重建（逃跑类结果）。
        /// </summary>
        public readonly bool ResetEncounterEnemy;

        /// <summary>
        /// 是否使用本局随机源结算掉落（胜利）。
        /// </summary>
        public readonly bool ShouldRollDrops;

        /// <summary>
        /// 逃跑类结果的保护时间（毫秒）；其他结果为 0。
        /// </summary>
        public readonly int ProtectionMs;

        /// <summary>
        /// 是否恢复探索更新。
        /// </summary>
        public readonly bool ShouldResumeExploration;

        /// <summary>
        /// 是否以单局失败结束，不返回探索。
        /// </summary>
        public readonly bool EndsRunAsDefeated;

        public BattleReturnPlan(int encounterId, BattleOutcome outcome,
            IReadOnlyList<BattlePlayerResult> players, bool removeEncounterEnemy, bool resetEncounterEnemy,
            bool shouldRollDrops, int protectionMs, bool shouldResumeExploration, bool endsRunAsDefeated)
        {
            EncounterId = encounterId;
            Outcome = outcome;
            Players = players != null ? Copy(players) : new BattlePlayerResult[0];
            RemoveEncounterEnemy = removeEncounterEnemy;
            ResetEncounterEnemy = resetEncounterEnemy;
            ShouldRollDrops = shouldRollDrops;
            ProtectionMs = protectionMs;
            ShouldResumeExploration = shouldResumeExploration;
            EndsRunAsDefeated = endsRunAsDefeated;
        }

        private static BattlePlayerResult[] Copy(IReadOnlyList<BattlePlayerResult> source)
        {
            BattlePlayerResult[] copy = new BattlePlayerResult[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}