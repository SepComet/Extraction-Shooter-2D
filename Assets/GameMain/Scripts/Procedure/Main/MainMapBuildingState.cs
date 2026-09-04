using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using SepCore.Battle;
using SepCore.Definition;
using SepCore.Entity;
using SepCore.Exploration;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.Procedure
{
    /// <summary>
    /// 主流程状态：地图构建。
    /// 负责启动单局探索计时、初始化出战角色状态、加载地图定义、确定性生成物资点/敌人分布、实例化场景实体，并准备单局探索环境。
    /// </summary>
    public sealed class MainMapBuildingState : FsmState<ProcedureMain>
    {
        private CancellationTokenSource _cts;

        protected override void OnEnter(IFsm<ProcedureMain> fsm)
        {
            base.OnEnter(fsm);
            Log.Info("[ProcedureMain] Entering MainMapBuildingState...");

            _cts = new CancellationTokenSource();
            BuildMapAsync(fsm, _cts.Token).Forget();
        }

        protected override void OnLeave(IFsm<ProcedureMain> fsm, bool isShutdown)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            base.OnLeave(fsm, isShutdown);
        }

        private async UniTaskVoid BuildMapAsync(IFsm<ProcedureMain> fsm, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 开始单局探索计时与状态管理（重置单局探索时间）
                GameEntry.TurnBattle.BeginRun();

                // 2. 获取单局难度与随机源
                DifficultyTier difficulty = DifficultyTier.Tier1;
                if (GameEntry.Save.Data?.loadout != null)
                {
                    difficulty = GameEntry.Save.Data.loadout.difficultyId;
                }
                fsm.Owner.Difficulty = difficulty;

                if (GameEntry.Random.Random == null)
                {
                    // 若未经过大厅战备输入种子直接进入场景调试，使用当前时间戳兜底初始化本局随机源
                    int fallbackSeed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
                    Log.Warning("RandomComponent is not initialized. Using fallback seed '{0}'.", fallbackSeed);
                    GameEntry.Random.BeginRun(fallbackSeed);
                }

                // 3. 构建并装配本局出战角色状态（包含装备加成）
                List<PlayerUnitState> partyPlayers = PlayerPartyBuilder.Build(GameEntry.Save.Data, GameEntry.Luban.Tables);
                GameEntry.TurnBattle.ReplacePlayers(partyPlayers);
                Log.Info("[ProcedureMain] Initialized {0} party players for exploration.", partyPlayers.Count);

                // 4. 确保实体组存在
                EnsureEntityGroup("Map");
                EnsureEntityGroup("ResourcePoint");
                EnsureEntityGroup("Enemy");
                EnsureEntityGroup("Player");

                // 5. 执行地图构建
                MapBuilder builder = new MapBuilder(difficulty, GameEntry.Random.Random);
                MapBuildResult buildResult = await builder.BuildAsync();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                fsm.Owner.BuildResult = buildResult;
                fsm.Owner.ExtractionPoint = TileToWorldPosition2D(buildResult.ExtractionPoint);
                fsm.Owner.PlayerSpawnPoint = TileToWorldPosition2D(buildResult.PlayerSpawnPoint);

                GlobalConfig global = GameEntry.Luban.Global?.Data;
                if (global == null)
                {
                    Log.Error("[ProcedureMain] GlobalConfig is missing from Luban tables.");
                    return;
                }

                // 6. 实例化撤离点实体（预制体资产名取自 GlobalConfig.EvacuatePointEntity）
                string evacAssetName = global.EvacuatePointEntity;
                if (string.IsNullOrEmpty(evacAssetName))
                {
                    Log.Error("[ProcedureMain] EvacuatePointEntity is not configured in GlobalConfig.");
                }
                else
                {
                    int evacEntityId = GameEntry.Entity.SerialId();
                    Vector3 evacPos = TileToWorldPosition(buildResult.ExtractionPoint);
                    EvacuatePointData evacData = new EvacuatePointData(evacEntityId, evacAssetName, evacPos);
                    GameEntry.Entity.ShowEntity<EvacuatePointLogic>(evacData, "Map", Constant.AssetPriority.SceneAsset);
                }

                // 7. 实例化资源点实体（预制体资产名取自 ResourcePointConfig.Prefab）
                foreach (ResourcePointBuildData resPoint in buildResult.ResourcePoints)
                {
                    ResourcePointConfig resConfig = GameEntry.Luban.Get<ResourcePointConfig>(resPoint.ResourcePointId);
                    if (resConfig == null || string.IsNullOrEmpty(resConfig.Prefab))
                    {
                        Log.Error("[ProcedureMain] ResourcePointConfig '{0}' or its Prefab is missing.", resPoint.ResourcePointId);
                        continue;
                    }

                    int resEntityId = GameEntry.Entity.SerialId();
                    string assetName = resConfig.Prefab;
                    Vector3 resPos = TileToWorldPosition(resPoint.Position);
                    ResourcePointData resData = new ResourcePointData(
                        resEntityId, assetName, resPos, resPoint.ResourcePointId, resPoint.ItemIds);
                    GameEntry.Entity.ShowEntity<ResourcePointLogic>(resData, "ResourcePoint", Constant.AssetPriority.SceneAsset);
                }

                // 8. 实例化敌人队伍实体（预制体资产名根据威胁等级取自 GlobalConfig）
                foreach (EnemyPointBuildData enemyPoint in buildResult.EnemyPoints)
                {
                    if (enemyPoint.ThreatLevel == EnemyPartyThreatLevel.None)
                    {
                        continue;
                    }

                    string assetName = GetEnemyPartyAssetName(global, enemyPoint.ThreatLevel);
                    if (string.IsNullOrEmpty(assetName))
                    {
                        Log.Error("[ProcedureMain] Enemy party entity asset name is missing in GlobalConfig for threat level '{0}'.",
                            enemyPoint.ThreatLevel);
                        continue;
                    }

                    int enemyEntityId = GameEntry.Entity.SerialId();
                    Vector3 enemyPos = TileToWorldPosition(enemyPoint.Position);
                    EnemyPartyData enemyData = new EnemyPartyData(
                        enemyEntityId, assetName, enemyPos, enemyPoint.EnemyPartyId, enemyPoint.ThreatLevel);
                    GameEntry.Entity.ShowEntity<EnemyPartyLogic>(enemyData, "Enemy", Constant.AssetPriority.EnemyAsset);
                }

                Log.Info("[ProcedureMain] Map build completed. Resource points: {0}, Enemies: {1}. Switching to ExplorationBattleState.",
                    buildResult.ResourcePoints.Count, buildResult.EnemyPoints.Count);

                ChangeState<MainExplorationBattleState>(fsm);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Log.Error("[ProcedureMain] Failed to build map with error: {0}", ex);
            }
        }

        /// <summary>
        /// 根据威胁等级从全局配置中获取对应的敌人队伍实体资产名称。
        /// </summary>
        public static string GetEnemyPartyAssetName(GlobalConfig global, EnemyPartyThreatLevel threatLevel)
        {
            if (global == null)
            {
                return null;
            }

            switch (threatLevel)
            {
                case EnemyPartyThreatLevel.Low:
                    return global.LowThreatEnemyPartyEntity;
                case EnemyPartyThreatLevel.Middle:
                    return global.MiddleThreatEnemyPartyEntity;
                case EnemyPartyThreatLevel.High:
                    return global.HighThreatEnemyPartyEntity;
                default:
                    return null;
            }
        }

        private static void EnsureEntityGroup(string groupName)
        {
            if (!GameEntry.Entity.HasEntityGroup(groupName))
            {
                GameEntry.Entity.AddEntityGroup(groupName, 60f, 32, 60f, 0);
            }
        }

        /// <summary>
        /// Tile 格子中心偏移（Tile 坐标原点在左下角，加 0.5 偏移对齐到网格中心）。
        /// </summary>
        private const float TileCenterOffset = 0.5f;

        private static Vector3 TileToWorldPosition(Vector2 tilePos)
        {
            return new Vector3(tilePos.x + TileCenterOffset, tilePos.y + TileCenterOffset, 0f);
        }

        private static Vector2 TileToWorldPosition2D(Vector2 tilePos)
        {
            return new Vector2(tilePos.x + TileCenterOffset, tilePos.y + TileCenterOffset);
        }
    }
}
