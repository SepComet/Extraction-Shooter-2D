using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.CustomComponent
{
    /// <summary>
    /// 单局战斗组件。
    /// 负责本局临时角色状态、计时、探索暂停标志和战斗占用的最小状态。
    /// 本局种子与共享随机源由独立的 RandomComponent 持有，战斗只沿用，不创建私有随机源。
    /// 战斗协调（构建请求、打开 UI、回写结果）由独立的 RunBattleCoordinator 负责。
    /// </summary>
    public class TurnBattleComponent : GameFrameworkComponent
    {
        private readonly List<RunPlayerState> _players = new List<RunPlayerState>();
        private float _elapsedMs;
        private bool _timerPaused;
        private bool _explorationPaused;
        private bool _battleActive;

        /// <summary>
        /// 获取本局临时角色状态列表（只读）。
        /// </summary>
        public IReadOnlyList<RunPlayerState> Players => _players;

        /// <summary>
        /// 获取本局已流逝时间（毫秒）。
        /// </summary>
        public long RunElapsedMs => (long)_elapsedMs;

        /// <summary>
        /// 获取单局计时是否已暂停。
        /// </summary>
        public bool IsTimerPaused => _timerPaused;

        /// <summary>
        /// 获取探索更新是否已暂停。
        /// 战斗期间为 true，地图更新统一以此作为门禁。
        /// </summary>
        public bool IsExplorationPaused => _explorationPaused;

        /// <summary>
        /// 获取当前是否存在战斗占用。
        /// 战斗重复进入由此标志拒绝。
        /// </summary>
        public bool IsBattleActive => _battleActive;

        /// <summary>
        /// 开始一局新的单局：重置全部临时状态。
        /// </summary>
        public void BeginRun()
        {
            _players.Clear();
            _elapsedMs = 0;
            _timerPaused = false;
            _explorationPaused = false;
            _battleActive = false;
        }

        /// <summary>
        /// 结束当前单局，清空全部临时状态。
        /// </summary>
        public void EndRun()
        {
            _players.Clear();
            _elapsedMs = 0;
            _timerPaused = false;
            _explorationPaused = false;
            _battleActive = false;
        }

        /// <summary>
        /// 用指定状态替换本局临时角色列表，保持战备顺序。
        /// </summary>
        /// <param name="players">新的临时角色状态，可为空。</param>
        public void ReplacePlayers(IEnumerable<RunPlayerState> players)
        {
            _players.Clear();
            if (players != null)
            {
                _players.AddRange(players);
            }
        }

        /// <summary>
        /// 预留战斗占用，拒绝同一帧或未结束战斗的重复进入。
        /// </summary>
        /// <returns>预留成功返回 true；已存在战斗占用时返回 false。</returns>
        public bool TryReserveBattle()
        {
            if (_battleActive)
            {
                return false;
            }

            _battleActive = true;
            return true;
        }

        /// <summary>
        /// 释放战斗占用。
        /// </summary>
        public void ReleaseBattle()
        {
            _battleActive = false;
        }

        /// <summary>
        /// 设置探索更新暂停状态。
        /// </summary>
        /// <param name="paused">是否暂停探索更新。</param>
        public void SetExplorationPaused(bool paused)
        {
            _explorationPaused = paused;
        }

        /// <summary>
        /// 设置单局计时暂停状态。
        /// </summary>
        /// <param name="paused">是否暂停单局计时。</param>
        public void SetTimerPaused(bool paused)
        {
            _timerPaused = paused;
        }

        private void Update()
        {
            if (_timerPaused)
            {
                return;
            }

            _elapsedMs += Time.deltaTime * 1000f;
        }
    }
}
