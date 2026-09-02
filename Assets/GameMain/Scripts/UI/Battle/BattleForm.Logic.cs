using System.Collections.Generic;
using SepCore.Battle;
using SepCore.Definition;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 战斗界面逻辑（手写 partial，与自动生成的 BattleForm.cs 合并）。
    /// UI 只读取 BattleViewState / BattleStep / BattleResult，不持有 BattleRuntime 或 BattleUnit；
    /// 玩家指令经 GameEntry.TurnBattle.SubmitCommand 提交，推进结果统一回填界面。
    /// 我方卡片按玩家单位数显示/隐藏；敌人槽和回合顺序槽按当前战局从模板动态实例化。
    /// </summary>
    public partial class BattleForm : UGuiForm
    {
        private BattleResult _result;
        private int _displayedRound;
        private readonly List<BattleTurnSlotItem> _turnSlots = new List<BattleTurnSlotItem>();
        private readonly List<int> _turnSlotUnitIds = new List<int>();
        private readonly List<BattleEnemySlotItem> _enemySlots = new List<BattleEnemySlotItem>();

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // M1 只开放攻击；道具、技能、逃跑在后续里程碑接入
            View.itemButton.interactable = false;
            View.skillButton.interactable = false;
            View.escapeButton.interactable = false;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // UIForm 实例复用：每次打开都重新注册按钮监听（OnClose 会移除）
            View.attackButton.onClick.AddListener(OnAttackButtonClick);
            View.skillButton.onClick.AddListener(OnSkillButtonClick);
            View.itemButton.onClick.AddListener(OnItemButtonClick);
            View.escapeButton.onClick.AddListener(OnEscapeButtonClick);

            if (GameEntry.TurnBattle != null)
            {
                GameEntry.TurnBattle.SetStepListener(ApplyStep);
            }

            _result = null;
            _displayedRound = 0;
            _turnSlots.Clear();
            _turnSlotUnitIds.Clear();
            _enemySlots.Clear();
            Refresh(GameEntry.TurnBattle != null ? GameEntry.TurnBattle.GetViewState() : null);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (GameEntry.TurnBattle != null)
            {
                GameEntry.TurnBattle.SetStepListener(null);
            }

            View.attackButton.onClick.RemoveListener(OnAttackButtonClick);
            View.skillButton.onClick.RemoveListener(OnSkillButtonClick);
            View.itemButton.onClick.RemoveListener(OnItemButtonClick);
            View.escapeButton.onClick.RemoveListener(OnEscapeButtonClick);

            base.OnClose(isShutdown, userData);
        }

        private void OnAttackButtonClick()
        {
            if (GameEntry.TurnBattle == null || !GameEntry.TurnBattle.IsBattleActive)
            {
                return;
            }

            BattleViewState view = GameEntry.TurnBattle.GetViewState();
            if (view == null || view.CurrentActorUnitId == 0)
            {
                return;
            }

            BattleUnitView actor = FindUnit(view, view.CurrentActorUnitId);
            if (actor == null || actor.Faction != BattleFaction.Player)
            {
                return;
            }

            int attackActionId = FindAttackActionId(view);
            int targetUnitId = FindFirstActiveEnemy(view);
            if (attackActionId == 0 || targetUnitId == 0)
            {
                return;
            }

            BattleStep step = GameEntry.TurnBattle.SubmitCommand(new BattleCommand(
                actor.BattleUnitId, BattleActionType.Attack, attackActionId,
                new List<int> { targetUnitId }));
            ApplyStep(step);
        }

        private void ApplyStep(BattleStep step)
        {
            if (step == null)
            {
                return;
            }

            if (step.Result != null)
            {
                _result = step.Result;
            }

            Refresh(step.View);
        }

        private void Refresh(BattleViewState view)
        {
            if (view == null)
            {
                return;
            }

            RefreshPlayerCards(view);
            RefreshEnemySlots(view);
            RefreshTurnSlots(view);
            RefreshActionPanel(view);
        }

        private void RefreshPlayerCards(BattleViewState view)
        {
            int index = 0;
            foreach (BattleUnitView unit in view.Units)
            {
                if (unit.Faction != BattleFaction.Player)
                {
                    continue;
                }

                BattleActorCardItem card = GetPlayerCard(index);
                if (card != null)
                {
                    card.gameObject.SetActive(true);
                    card.SetUnit(unit, unit.BattleUnitId == view.CurrentActorUnitId);
                }

                index++;
            }

            for (int i = index; i < 4; i++)
            {
                BattleActorCardItem card = GetPlayerCard(i);
                if (card != null)
                {
                    card.gameObject.SetActive(false);
                }
            }
        }

        private void RefreshEnemySlots(BattleViewState view)
        {
            BattleEnemySlotItem template = View.battleEnemySlotTemplate;
            if (template == null || View.enemySlotsRoot == null)
            {
                Log.Warning("BattleForm enemy slots are not configured.");
                return;
            }

            List<BattleUnitView> enemies = new List<BattleUnitView>();
            foreach (BattleUnitView unit in view.Units)
            {
                if (unit.Faction == BattleFaction.Enemy)
                {
                    enemies.Add(unit);
                }
            }

            // 敌人数量在一场战斗内固定：数量变化（新开战斗/规模不同）时才重建，之后原地复用
            if (_enemySlots.Count != enemies.Count)
            {
                template.gameObject.SetActive(false);
                ClearSlots(View.enemySlotsRoot, template.transform);
                _enemySlots.Clear();
                foreach (BattleUnitView unit in enemies)
                {
                    BattleEnemySlotItem slot = Instantiate(template, View.enemySlotsRoot);
                    slot.gameObject.SetActive(true);
                    _enemySlots.Add(slot);
                }
            }

            for (int i = 0; i < _enemySlots.Count; i++)
            {
                _enemySlots[i].SetEnemy(enemies[i], enemies[i].BattleUnitId == view.CurrentActorUnitId);
            }
        }

        private void RefreshTurnSlots(BattleViewState view)
        {
            BattleTurnSlotItem template = View.battleTurnSlotTemplate;
            if (template == null || View.turnSlotsRoot == null)
            {
                Log.Warning("BattleForm turn slots are not configured.");
                return;
            }

            if (view.RoundNumber != _displayedRound ||
                (view.CurrentActorUnitId != 0 && !_turnSlotUnitIds.Contains(view.CurrentActorUnitId)))
            {
                RebuildTurnSlots(view, template);
                _displayedRound = view.RoundNumber;
                return;
            }

            // 同轮内只移动当前行动者高亮，不隐藏已行动单位
            for (int i = 0; i < _turnSlots.Count; i++)
            {
                _turnSlots[i].SetCurrentActor(_turnSlotUnitIds[i] == view.CurrentActorUnitId);
            }
        }

        /// <summary>
        /// 新一轮开始时刷新本轮顺序：从当前行动者开始的剩余单位按调度优先级排列，
        /// 当前行动者高亮；本轮内已行动单位保持可见。
        /// 数量与上一轮相同时复用槽对象，只重建单位映射与内容；
        /// 先制第一轮只有玩家候选，敌人进入行动阶段时（当前行动者不在列表）自动重建为剩余敌人。
        /// </summary>
        private void RebuildTurnSlots(BattleViewState view, BattleTurnSlotItem template)
        {
            List<BattleUnitView> units = new List<BattleUnitView>();
            foreach (int unitId in view.RemainingTurnOrder)
            {
                BattleUnitView unit = FindUnit(view, unitId);
                if (unit != null)
                {
                    units.Add(unit);
                }
            }

            if (_turnSlots.Count != units.Count)
            {
                template.gameObject.SetActive(false);
                ClearSlots(View.turnSlotsRoot, template.transform);
                _turnSlots.Clear();
                foreach (BattleUnitView unit in units)
                {
                    BattleTurnSlotItem slot = Instantiate(template, View.turnSlotsRoot);
                    slot.gameObject.SetActive(true);
                    _turnSlots.Add(slot);
                }
            }

            _turnSlotUnitIds.Clear();
            for (int i = 0; i < _turnSlots.Count; i++)
            {
                _turnSlotUnitIds.Add(units[i].BattleUnitId);
                _turnSlots[i].SetTurnSlot(units[i], units[i].BattleUnitId == view.CurrentActorUnitId);
            }
        }

        private void RefreshActionPanel(BattleViewState view)
        {
            if (_result != null)
            {
                View.currentActorText.text = GetOutcomeText(_result.Outcome);
                View.attackButton.interactable = false;
                return;
            }

            BattleUnitView actor = FindUnit(view, view.CurrentActorUnitId);
            bool playerTurn = actor != null && actor.Faction == BattleFaction.Player;
            View.currentActorText.text = string.Format("第 {0} 轮  {1} 行动",
                view.RoundNumber, actor != null ? BattleUnitViewHelper.GetDisplayName(actor) : string.Empty);
            View.attackButton.interactable = playerTurn;
        }

        private static string GetOutcomeText(BattleOutcome outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.Victory:
                    return "胜利！";
                case BattleOutcome.AllEscaped:
                    return "全员逃跑";
                case BattleOutcome.PartialEscapeDefeat:
                    return "部分逃跑，战斗失败";
                case BattleOutcome.TotalDefeat:
                    return "全员阵亡，单局失败";
                default:
                    return string.Empty;
            }
        }

        private static int FindAttackActionId(BattleViewState view)
        {
            foreach (int actionId in view.AvailableActionIds)
            {
                BattleActionConfig action = GameEntry.Luban.Get<BattleActionConfig>(actionId);
                if (action != null && action.ActionType == BattleActionType.Attack)
                {
                    return actionId;
                }
            }

            return 0;
        }

        private static int FindFirstActiveEnemy(BattleViewState view)
        {
            foreach (BattleUnitView unit in view.Units)
            {
                if (unit.Faction == BattleFaction.Enemy && !unit.IsDefeated && !unit.IsEscaped)
                {
                    return unit.BattleUnitId;
                }
            }

            return 0;
        }

        private static BattleUnitView FindUnit(BattleViewState view, int unitId)
        {
            foreach (BattleUnitView unit in view.Units)
            {
                if (unit.BattleUnitId == unitId)
                {
                    return unit;
                }
            }

            return null;
        }

        private BattleActorCardItem GetPlayerCard(int index)
        {
            switch (index)
            {
                case 0:
                    return View.playerCard1;
                case 1:
                    return View.playerCard2;
                case 2:
                    return View.playerCard3;
                case 3:
                    return View.playerCard4;
                default:
                    return null;
            }
        }

        private static void ClearSlots(RectTransform root, Transform template)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child == template)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        private void OnSkillButtonClick()
        {
            // M3 接入
        }

        private void OnItemButtonClick()
        {
            // 道具首版禁用，不产生指令
        }

        private void OnEscapeButtonClick()
        {
            // M5 接入
        }
    }
}