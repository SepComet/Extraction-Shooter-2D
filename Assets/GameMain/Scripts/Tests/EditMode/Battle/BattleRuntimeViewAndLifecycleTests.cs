using System.Collections.Generic;
using NUnit.Framework;
using SepCore.Battle;
using SepCore.CustomComponent;
using SepCore.Definition;

namespace SepCore.Tests
{
    [TestFixture]
    public class BattleRuntimeViewAndLifecycleTests
    {
        [Test]
        public void BuildViewState_ReflectsUnitsAndTurnOrder()
        {
            BattleRuntime runtime = TestBattle.Create1v1(TestBattle.Player());

            BattleViewState view = runtime.BuildViewState();

            Assert.AreEqual(1, view.RoundNumber);
            Assert.AreEqual(1, view.CurrentActorUnitId);
            Assert.AreEqual(2, view.Units.Count);
            CollectionAssert.AreEqual(new List<int> { 1, 2 }, view.RemainingTurnOrder);
            CollectionAssert.AreEqual(new List<int> { 1, 101 }, view.AvailableActionIds);

            BattleUnitView playerView = view.Units[0];
            Assert.AreEqual(BattleFactionType.Player, playerView.Faction);
            Assert.AreEqual(1001, playerView.ConfigId);
            Assert.AreEqual(1, playerView.PartyOrder);
            Assert.AreEqual(120, playerView.CurrentHp);
            Assert.AreEqual(40, playerView.CurrentMp);
            Assert.AreEqual(12, playerView.Speed);
            Assert.False(playerView.IsDefeated);
            Assert.False(playerView.IsEscaped);
            Assert.IsEmpty(playerView.States);

            BattleUnitView enemyView = view.Units[1];
            Assert.AreEqual(BattleFactionType.Enemy, enemyView.Faction);
            Assert.AreEqual(3001, enemyView.ConfigId);
            Assert.AreEqual(50, enemyView.CurrentHp);
            Assert.False(enemyView.IsDefeated);
        }

        [Test]
        public void BuildViewState_AfterPlayerAction_HighlightsEnemyAndKeepsAllUnits()
        {
            BattleRuntime runtime = TestBattle.Create1v1(TestBattle.Player());
            runtime.SubmitCommand(TestBattle.Attack(1, 2));

            BattleViewState view = runtime.BuildViewState();

            Assert.AreEqual(2, view.CurrentActorUnitId);
            // 同轮内列表仍包含全部单位（已行动玩家不被隐藏）
            CollectionAssert.AreEqual(new List<int> { 2 }, view.RemainingTurnOrder);
            Assert.AreEqual(2, view.Units.Count);
            Assert.AreEqual(36, view.Units[1].CurrentHp);
        }

        [Test]
        public void BuildViewState_AfterCompletion_HasNoActor()
        {
            BattleRuntime runtime = TestBattle.Create1v1(TestBattle.Player());
            int guard = 0;
            while (runtime.Result == null && guard++ < 200)
            {
                BattleUnit actor = runtime.CurrentActor;
                if (actor != null && actor.Faction == BattleFactionType.Player)
                {
                    int targetUnitId = 0;
                    foreach (BattleUnit unit in runtime.Units)
                    {
                        if (unit.Faction == BattleFactionType.Enemy && BattleRuntime.IsActive(unit))
                        {
                            targetUnitId = unit.UnitId;
                            break;
                        }
                    }

                    runtime.SubmitCommand(TestBattle.Attack(actor.UnitId, targetUnitId));
                }
                else
                {
                    runtime.AdvanceEnemyTurn();
                }
            }

            BattleViewState view = runtime.BuildViewState();

            Assert.AreEqual(0, view.CurrentActorUnitId);
            Assert.IsEmpty(view.RemainingTurnOrder);
            Assert.IsEmpty(view.AvailableActionIds);
        }

        [Test]
        public void SequentialBattles_DoNotInheritState()
        {
            // 第一场打到玩家残血
            BattleRuntime first = TestBattle.Create1v1(TestBattle.Player());
            first.SubmitCommand(TestBattle.Attack(1, 2));
            first.AdvanceEnemyTurn();
            Assert.AreEqual(112, first.GetUnit(1).CurrentHp);

            // 第二场用同一份输入重新创建：应从满值开局
            BattleRuntime second = TestBattle.Create1v1(TestBattle.Player());
            Assert.AreEqual(120, second.GetUnit(1).CurrentHp);
            Assert.AreEqual(50, second.GetUnit(2).CurrentHp);
            Assert.AreEqual(1, second.RoundNumber);
            Assert.AreEqual(1, second.CurrentActorUnitId);
            Assert.False(second.IsCompleted);
        }

        [Test]
        public void SequentialBattles_UseIndependentActedSets()
        {
            BattleRuntime first = TestBattle.Create1v1(TestBattle.Player());
            first.SubmitCommand(TestBattle.Attack(1, 2));
            Assert.IsTrue(first.ActedUnitIds.Contains(1));

            BattleRuntime second = TestBattle.Create1v1(TestBattle.Player());
            Assert.IsEmpty(second.ActedUnitIds);
            Assert.AreEqual(1, second.CurrentActorUnitId);
        }
    }
}