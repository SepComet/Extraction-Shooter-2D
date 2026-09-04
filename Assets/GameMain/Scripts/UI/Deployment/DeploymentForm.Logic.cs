using System;
using System.Collections.Generic;
using SepCore.Base;
using SepCore.Definition;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 战备出战面板逻辑（手写 partial，与自动生成的 DeploymentForm.cs 合并）。
    /// 负责难度切换、种子设定、出战信息显示，并在点击开始时直接构造出战角色列表并触发单局探索。
    /// </summary>
    public partial class DeploymentForm : UGuiForm
    {
        private bool _listenersBound = false;

        public void Refresh()
        {
            EnsureListenersBound();
            RefreshUI();
        }

        private void EnsureListenersBound()
        {
            if (_listenersBound)
            {
                return;
            }

            _listenersBound = true;

            if (View.beginRunButton != null)
            {
                View.beginRunButton.onClick.AddListener(OnBeginRunButtonClick);
            }

            if (View.randomizeButton != null)
            {
                View.randomizeButton.onClick.AddListener(OnRandomizeButtonClick);
            }

            if (View.tier1Toggle != null)
            {
                View.tier1Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnDifficultyToggleChanged(DifficultyTier.Tier1);
                });
            }

            if (View.tier2Toggle != null)
            {
                View.tier2Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnDifficultyToggleChanged(DifficultyTier.Tier2);
                });
            }

            if (View.tier3Toggle != null)
            {
                View.tier3Toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnDifficultyToggleChanged(DifficultyTier.Tier3);
                });
            }
        }

        private void RefreshUI()
        {
            GlobalConfig global = GameEntry.Luban.Global?.Data;
            if (global != null && View.timeLimitText != null)
            {
                int minutes = global.RunTimeLimitMs / 60000;
                int seconds = (global.RunTimeLimitMs % 60000) / 1000;
                View.timeLimitText.text = $"{minutes:00}:{seconds:00}";
            }

            SaveData save = GameEntry.Save.Data;
            DifficultyTier difficulty = save?.loadout?.difficultyId ?? DifficultyTier.Tier1;
            SetDifficultyToggle(difficulty);

            if (View.seedInput != null && string.IsNullOrEmpty(View.seedInput.text))
            {
                int randomSeed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
                View.seedInput.text = randomSeed.ToString();
            }

            int characterCount = save?.characters?.Count ?? 0;
            if (View.partySizeText != null)
            {
                int deployedCount = Math.Min(Math.Max(characterCount, 1), 4);
                View.partySizeText.text = $"{deployedCount}/4";
            }
        }

        private void SetDifficultyToggle(DifficultyTier difficulty)
        {
            switch (difficulty)
            {
                case DifficultyTier.Tier2:
                    if (View.tier2Toggle != null) View.tier2Toggle.isOn = true;
                    break;
                case DifficultyTier.Tier3:
                    if (View.tier3Toggle != null) View.tier3Toggle.isOn = true;
                    break;
                default:
                    if (View.tier1Toggle != null) View.tier1Toggle.isOn = true;
                    break;
            }
        }

        private void OnDifficultyToggleChanged(DifficultyTier difficulty)
        {
            SaveData save = GameEntry.Save.Data;
            if (save != null)
            {
                if (save.loadout == null)
                {
                    save.loadout = new LoadoutSave();
                }

                save.loadout.difficultyId = difficulty;
            }
        }

        private void OnRandomizeButtonClick()
        {
            int seed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
            if (View.seedInput != null)
            {
                View.seedInput.text = seed.ToString();
            }
        }

        private void OnBeginRunButtonClick()
        {
            SaveData save = GameEntry.Save.Data;
            if (save == null)
            {
                Log.Error("SaveData is null, cannot start run.");
                return;
            }

            if (save.loadout == null)
            {
                save.loadout = new LoadoutSave();
            }

            // 1. 直接构造出战列表（取当前存档拥有的角色前 1~4 人）
            List<int> partyIds = new List<int>();
            if (save.characters != null && save.characters.Count > 0)
            {
                for (int i = 0; i < Math.Min(save.characters.Count, 4); i++)
                {
                    partyIds.Add(save.characters[i].characterId);
                }
            }
            else
            {
                GlobalConfig global = GameEntry.Luban.Global?.Data;
                if (global?.NewGameCharacterIds != null)
                {
                    for (int i = 0; i < Math.Min(global.NewGameCharacterIds.Count, 4); i++)
                    {
                        partyIds.Add(global.NewGameCharacterIds[i]);
                    }
                }
            }

            save.loadout.partyCharacterIds = partyIds.ToArray();

            // 2. 获取难度配置
            DifficultyTier difficulty = DifficultyTier.Tier1;
            if (View.tier3Toggle != null && View.tier3Toggle.isOn)
            {
                difficulty = DifficultyTier.Tier3;
            }
            else if (View.tier2Toggle != null && View.tier2Toggle.isOn)
            {
                difficulty = DifficultyTier.Tier2;
            }

            save.loadout.difficultyId = difficulty;

            // 3. 读取种子并播种共享随机源
            int seed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
            if (View.seedInput != null && int.TryParse(View.seedInput.text, out int inputSeed))
            {
                seed = inputSeed;
            }

            GameEntry.Random.BeginRun(seed);

            // 4. 写盘保存战备配置
            GameEntry.Save.Save();

            Log.Info("[DeploymentForm] Starting run with {0} characters, difficulty: {1}, seed: {2}.",
                partyIds.Count, difficulty, seed);

            // 5. 抛出开始单局事件，由 ProcedureMenu 接收并切场景进入单局
            GameEntry.Event.Fire(this, StartRunEventArgs.Create());
        }
    }
}
