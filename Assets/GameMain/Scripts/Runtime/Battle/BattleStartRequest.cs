using System.Collections.Generic;
using SepCore.CustomComponent;

namespace SepCore.Battle
{
    /// <summary>
    /// 战斗启动请求，创建后视为只读。
    /// 战斗开始失败时不得消耗随机数、修改单局状态或暂停状态计数。
    /// </summary>
    public sealed class BattleStartRequest
    {
        /// <summary>
        /// 用于将结果关联回地图敌人实例。
        /// </summary>
        public readonly int EncounterId;

        /// <summary>
        /// 1 到 4 个玩家输入，保持战备顺序。
        /// </summary>
        public readonly IReadOnlyList<BattlePlayerInput> Players;

        /// <summary>
        /// 用于按队伍预设创建 1 到 4 个敌人运行时单位。
        /// </summary>
        public readonly int EnemyPartyConfigId;

        /// <summary>
        /// 是否启用第一轮玩家先制。
        /// </summary>
        public readonly bool IsPreemptive;

        /// <summary>
        /// 当前单局唯一的随机源实例，来自 RandomComponent，不创建战斗私有随机源。
        /// </summary>
        public readonly IRunRandomSource Random;

        public BattleStartRequest(int encounterId, IReadOnlyList<BattlePlayerInput> players,
            int enemyPartyConfigId, bool isPreemptive, IRunRandomSource random)
        {
            EncounterId = encounterId;
            Players = players != null ? Copy(players) : new BattlePlayerInput[0];
            EnemyPartyConfigId = enemyPartyConfigId;
            IsPreemptive = isPreemptive;
            Random = random;
        }

        private static BattlePlayerInput[] Copy(IReadOnlyList<BattlePlayerInput> source)
        {
            BattlePlayerInput[] copy = new BattlePlayerInput[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }
    }
}