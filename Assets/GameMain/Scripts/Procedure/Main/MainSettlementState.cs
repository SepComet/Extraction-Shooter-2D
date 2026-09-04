using System;
using System.Collections.Generic;
using GameFramework.Fsm;
using SepCore.Definition;
using UnityGameFramework.Runtime;

namespace SepCore.Procedure
{
    /// <summary>
    /// 主流程状态：撤离结算。
    /// 负责执行局内与局外物品结算、更新角色装备、追加 RunRecord 历史并写盘，重置战斗组件与随机源，最后返回大厅。
    /// </summary>
    public sealed class MainSettlementState : FsmState<ProcedureMain>
    {
        protected override void OnEnter(IFsm<ProcedureMain> fsm)
        {
            base.OnEnter(fsm);
            Log.Info("[ProcedureMain] Entering MainSettlementState...");

            RunResultType outcome = fsm.Owner.PendingOutcome ?? RunResultType.TimedOut;
            ExecuteSettlement(fsm.Owner, outcome);
        }

        private void ExecuteSettlement(ProcedureMain procedureMain, RunResultType outcome)
        {
            SaveData save = GameEntry.Save.Data;
            if (save != null)
            {
                // 1. 死亡/超时/主动退出时，已穿戴装备随角色丢失（保留保险箱内容）
                if (outcome != RunResultType.Extracted && save.characters != null)
                {
                    for (int i = 0; i < save.characters.Count; i++)
                    {
                        CharacterSave c = save.characters[i];
                        c.weaponItemId = 0;
                        c.armorItemId = 0;
                        save.characters[i] = c;
                    }
                }

                // 2. 追加本局历史结算记录
                long endedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long startedAt = procedureMain.RunStartTimeUtcMs > 0 ? procedureMain.RunStartTimeUtcMs : endedAt;
                long seed = GameEntry.Random.Seed;
                DifficultyTier difficulty = procedureMain.Difficulty;

                if (save.runHistory == null)
                {
                    save.runHistory = new List<RunRecord>();
                }

                save.runHistory.Add(new RunRecord(outcome, difficulty, seed, startedAt, endedAt));

                // 3. 写入磁盘
                GameEntry.Save.Save();
                Log.Info("[ProcedureMain] Run record saved to disk. Outcome: {0}, Difficulty: {1}, Seed: {2}.",
                    outcome, difficulty, seed);
            }

            // 4. 清理战斗组件单局临时状态与计时器
            GameEntry.TurnBattle.EndRun();

            // 5. 清理本局共享随机源
            GameEntry.Random.EndRun();

            // 6. 返回大厅（若开启了自动返回）
            if (procedureMain.AutoReturnToMenu)
            {
                procedureMain.ReturnToMenu();
            }
        }
    }
}
