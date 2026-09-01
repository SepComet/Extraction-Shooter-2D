using GameFramework.Debugger;
using SepCore.Battle;
using SepCore.CustomComponent;
using SepCore.Definition;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.Debugger
{
    /// <summary>
    /// 战斗壳层调试入口（仅供 Editor/Development Build，正式构建不注册）。
    /// 模拟探索层：缺少单局状态时用配表角色与固定种子初始化临时状态，
    /// 然后通过 RunBattleCoordinator 以固定调试遭遇开始/结束战斗，不依赖尚未实现的地图敌人。
    /// </summary>
    public class BattleDebuggerWindow : IDebuggerWindow
    {
        private const int DebugRunSeed = 240829;

        public void Initialize(params object[] args)
        {
        }

        public void Shutdown()
        {
        }

        public void OnEnter()
        {
        }

        public void OnLeave()
        {
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void OnDraw()
        {
            GUILayout.Label("<b>Battle Shell</b>");
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Open", GUILayout.Height(30)))
                {
                    OpenShell();
                }

                if (GUILayout.Button("Close", GUILayout.Height(30)))
                {
                    GameEntry.RunBattle.EndDebugBattle();
                }
            }
            GUILayout.EndHorizontal();

            DrawRunState();
        }

        private static void DrawRunState()
        {
            GUILayout.Label("<b>Run State</b>");
            GUILayout.BeginVertical("box");
            {
                TurnBattleComponent runBattle = GameEntry.TurnBattle;
                if (runBattle == null)
                {
                    GUILayout.Label("TurnBattleComponent missing.");
                    return;
                }

                GUILayout.Label("RunElapsedMs: " + runBattle.RunElapsedMs.ToString());
                GUILayout.Label("IsTimerPaused: " + runBattle.IsTimerPaused);
                GUILayout.Label("IsExplorationPaused: " + runBattle.IsExplorationPaused);
                GUILayout.Label("IsBattleActive: " + runBattle.IsBattleActive);
            }
            GUILayout.EndVertical();
        }

        private static void OpenShell()
        {
            InitDebugRunStateIfNeeded();

            // 固定调试遭遇：EncounterId=1，敌人队伍预设 1，普通战斗
            bool started = GameEntry.RunBattle.TryStartBattle(new BattleEncounter(1, 1, false));
            if (!started)
            {
                Log.Warning("Battle debug shell can not start, maybe a battle is already active.");
            }
        }

        private static void InitDebugRunStateIfNeeded()
        {
            if (GameEntry.Random.Random == null)
            {
                GameEntry.Random.BeginRun(DebugRunSeed);
            }

            if (GameEntry.TurnBattle.Players.Count == 0)
            {
                GameEntry.TurnBattle.ReplacePlayers(BuildDebugPlayers());
            }
        }

        private static List<RunPlayerState> BuildDebugPlayers()
        {
            List<RunPlayerState> players = new List<RunPlayerState>();
            GlobalConfig global = GameEntry.Luban.Global != null ? GameEntry.Luban.Global.Data : null;
            if (global == null)
            {
                Log.Warning("Can not build debug players without global config.");
                return players;
            }

            int order = 1;
            foreach (int characterId in global.NewGameCharacterIds)
            {
                CharacterConfig config = GameEntry.Luban.Get<CharacterConfig>(characterId);
                if (config == null)
                {
                    Log.Warning("Debug character '{0}' missing in config, skipped.", characterId.ToString());
                    continue;
                }

                players.Add(new RunPlayerState
                {
                    CharacterId = characterId,
                    PartyOrder = order++,
                    CurrentHp = config.MaxHp,
                    CurrentMp = config.MaxMp,
                    MaxHp = config.MaxHp,
                    MaxMp = config.MaxMp,
                    Atk = config.Atk,
                    Mat = config.Mat,
                    Speed = config.Speed,
                    AttackActionId = config.AttackActionId,
                    SkillActionId = config.SkillActionId,
                });
            }

            return players;
        }
    }
}