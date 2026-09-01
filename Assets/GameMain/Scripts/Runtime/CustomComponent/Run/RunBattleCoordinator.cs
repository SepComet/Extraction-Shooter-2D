using System.Collections.Generic;
using SepCore.Battle;
using SepCore.Definition;
using SepCore.UI;
using UnityGameFramework.Runtime;

namespace SepCore.CustomComponent
{
    /// <summary>
    /// 单局战斗协调器（普通类，不挂场景）。
    /// 负责从单局状态构建请求、预留战斗占用、暂停/恢复探索更新与计时、打开/关闭战斗 UI；
    /// 后续里程碑在此创建并校验 BattleController，并消费 BattleResult 生成 BattleReturnPlan。
    /// 不负责回合排序、效果计算和 UI 控件状态。
    /// </summary>
    public sealed class RunBattleCoordinator
    {
        /// <summary>
        /// 尝试开始一场战斗。
        /// 战斗占用已激活时拒绝；请求构建失败时释放占用且不暂停地图。
        /// </summary>
        /// <param name="encounter">探索层创建的遭遇输入。</param>
        /// <returns>是否成功开始。</returns>
        public bool TryStartBattle(BattleEncounter encounter)
        {
            if (encounter == null)
            {
                Log.Error("Can not start battle with null encounter.");
                return false;
            }

            TurnBattleComponent runBattle = GameEntry.TurnBattle;
            if (runBattle == null || runBattle.IsBattleActive)
            {
                Log.Warning("Can not start battle because battle occupancy is already active.");
                return false;
            }

            BattleStartRequest request = BuildStartRequest(encounter);
            if (request == null)
            {
                Log.Warning("Can not start battle because start request is invalid.");
                return false;
            }

            // 预留战斗占用并暂停探索更新与单局计时；
            // 后续里程碑在此创建并校验 BattleController，失败则释放占用且不暂停地图
            runBattle.TryReserveBattle();
            runBattle.SetExplorationPaused(true);
            runBattle.SetTimerPaused(true);

            GameEntry.UI.OpenUIForm(UIFormType.BattleForm);
            Log.Info("Battle started with encounter '{0}'.", encounter.EncounterId);
            return true;
        }

        /// <summary>
        /// 关闭调试壳层并恢复探索更新与单局计时。
        /// M0 调试入口专用；M7 起由 ApplyResult 按战斗结果统一回写与恢复。
        /// </summary>
        public void EndDebugBattle()
        {
            TurnBattleComponent runBattle = GameEntry.TurnBattle;
            if (runBattle == null)
            {
                return;
            }

            UGuiForm form = GameEntry.UI.GetUIForm(UIFormType.BattleForm);
            if (form != null)
            {
                GameEntry.UI.CloseUIForm(form);
            }

            runBattle.SetExplorationPaused(false);
            runBattle.SetTimerPaused(false);
            runBattle.ReleaseBattle();
            Log.Info("Debug battle closed.");
        }

        /// <summary>
        /// 从单局临时角色状态与共享随机源构建启动请求。
        /// 请求创建后视为只读；构建失败返回 null，不消耗随机数。
        /// </summary>
        private static BattleStartRequest BuildStartRequest(BattleEncounter encounter)
        {
            IRunRandomSource random = GameEntry.Random != null ? GameEntry.Random.Random : null;
            if (random == null)
            {
                Log.Warning("Can not start battle because run random source is not initialized.");
                return null;
            }

            List<BattlePlayerInput> players = new List<BattlePlayerInput>();
            foreach (RunPlayerState state in GameEntry.TurnBattle.Players)
            {
                players.Add(new BattlePlayerInput(state.CharacterId, state.PartyOrder,
                    state.CurrentHp, state.CurrentMp, state.MaxHp, state.MaxMp,
                    state.Atk, state.Mat, state.Speed,
                    state.AttackActionId, state.SkillActionId));
            }

            if (players.Count == 0)
            {
                Log.Warning("Can not start battle because no run player state exists.");
                return null;
            }

            return new BattleStartRequest(encounter.EncounterId, players, encounter.EnemyPartyConfigId,
                encounter.IsPreemptive, random);
        }
    }
}