using System;
using SepCore.Definition;
using UnityEngine;

namespace SepCore.Entity
{
    /// <summary>
    /// 敌人队伍实体数据。
    /// 包含敌人队伍配置 ID 和威胁等级。
    /// </summary>
    [Serializable]
    public sealed class EnemyPartyData : EntityDataBase
    {
        [SerializeField] private int _enemyPartyId;
        [SerializeField] private EnemyPartyThreatLevel _threatLevel;

        public EnemyPartyData(
            int entityId,
            string assetName,
            Vector3 position,
            int enemyPartyId,
            EnemyPartyThreatLevel threatLevel,
            Quaternion? rotation = null)
            : base(assetName, entityId)
        {
            Position = position;
            Rotation = rotation ?? Quaternion.identity;
            _enemyPartyId = enemyPartyId;
            _threatLevel = threatLevel;
        }

        /// <summary>
        /// 敌人队伍配置 ID（对应 EnemyPartyConfig.Id）
        /// </summary>
        public int EnemyPartyId => _enemyPartyId;

        /// <summary>
        /// 敌人队伍威胁等级
        /// </summary>
        public EnemyPartyThreatLevel ThreatLevel => _threatLevel;
    }
}
