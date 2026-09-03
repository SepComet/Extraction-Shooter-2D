using System.Collections.Generic;
using NUnit.Framework;
using SepCore.Battle;
using SepCore.CustomComponent;
using SepCore.Definition;

namespace SepCore.Tests
{
    [TestFixture]
    public class BattleRuntimeStunAndSpeedTests
    {
        [Test]
        public void Stun_UnactedTarget_SkipsCurrentRoundAction()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            // 玩家速度 20 > 敌人 8，玩家先手
            PlayerUnitState player = TestBattle.Player(speed: 20, partyOrder: 1, characterId: 1001, skillActionId: 104);
            BattleRuntime runtime = TestBattle.Create1v1(player, config);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);

            // 技能 104：SingleEnemy，Stun(1)，MpCost = 10
            BattleStep step = runtime.SubmitCommand(TestBattle.Skill(1, 104, 2));

            // 施加事件 + 跳过事件；敌人本轮机会被消耗，状态移除，进入第 2 轮玩家行动
            Assert.AreEqual(2, step.Events.Count);
            Assert.AreEqual(BattleStateType.Stun, step.Events[0].StatusType);
            Assert.AreEqual(1, step.Events[0].StatusRemainingRounds);
            Assert.AreEqual(BattleStateType.Stun, step.Events[1].StatusType);
            Assert.AreEqual(0, step.Events[1].StatusRemainingRounds);
            Assert.AreEqual(2, step.Events[1].ActorUnitId);

            Assert.AreEqual(2, runtime.RoundNumber);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);
            Assert.IsEmpty(runtime.GetUnit(2).Statuses);
            Assert.AreEqual(50, runtime.GetUnit(2).CurrentHp);
            Assert.AreEqual(30, runtime.GetUnit(1).CurrentMp);

            // 下一轮敌人恢复正常行动
            runtime.SubmitCommand(TestBattle.Attack(1, 2));
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            BattleStep enemyStep = runtime.AdvanceEnemyTurn();
            Assert.AreEqual(1, enemyStep.Events.Count);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);
        }

        [Test]
        public void Stun_ActedTarget_SkipsNextRoundAction()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            config.AddEnemy(TestConfigFactory.Create<EnemyConfig>(
                "Id", 3001, "Name", "快敌", "MaxHp", 50, "MaxMp", 0, "Atk", 8, "Mat", 0,
                "Speed", 20, "ThreatLevelId", 1, "AiType", EnemyAiType.Random,
                "ActionIds", new List<int> { 201 }, "DropTableId", 0));

            // 敌人速度 20 > 玩家 10，敌人先手
            PlayerUnitState player = TestBattle.Player(speed: 10, partyOrder: 1, characterId: 1001, skillActionId: 104);
            BattleRuntime runtime = TestBattle.Create1v1(player, config);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);

            // 第 1 轮敌人行动后，玩家眩晕已行动的敌人
            runtime.AdvanceEnemyTurn();
            Assert.AreEqual(1, runtime.CurrentActorUnitId);
            BattleStep step = runtime.SubmitCommand(TestBattle.Skill(1, 104, 2));

            // 敌人第 2 轮机会被跳过，回到玩家行动
            Assert.AreEqual(2, runtime.RoundNumber);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);
            Assert.IsEmpty(runtime.GetUnit(2).Statuses);
        }

        [Test]
        public void Stun_Reapply_KeepsLongerDuration()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            config.AddParty(TestConfigs.Party(4002, 3001));
            // 技能 106：SingleEnemy，Stun(3)
            config.AddAction(TestConfigs.SkillAction(106, BattleTargetType.SingleEnemy, 0,
                BattleStatType.None, 0, 10, BattleStatType.None, BattleStateType.Stun, 3));

            PlayerUnitState pA = TestBattle.Player(speed: 30, partyOrder: 1, characterId: 1001, skillActionId: 106);
            PlayerUnitState pB = TestBattle.Player(currentHp: 120, maxHp: 120, speed: 20, partyOrder: 2, characterId: 1002);
            BattleEncounter encounter = new BattleEncounter(1, 4002, false);
            BattleRuntime runtime = BattleRuntime.Create(encounter, new List<PlayerUnitState> { pA, pB }, config, new TestRandomSource());

            // 第 1 轮：A 施加 Stun(3)，B 攻击后敌人跳过（剩余 2）
            BattleStep stunStep = runtime.SubmitCommand(TestBattle.Skill(1, 106, 3));
            Assert.AreEqual(3, stunStep.Events[0].StatusRemainingRounds);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            runtime.SubmitCommand(TestBattle.Attack(2, 3));
            Assert.AreEqual(2, runtime.RoundNumber);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);

            // 第 2 轮：A 补 Stun(1)，保留较长的 2（不是覆盖为 1）
            BattleStep reapplyStep = runtime.SubmitCommand(TestBattle.Skill(1, 104, 3));
            Assert.AreEqual(BattleStateType.Stun, reapplyStep.Events[0].StatusType);
            Assert.AreEqual(2, reapplyStep.Events[0].StatusRemainingRounds);

            runtime.SubmitCommand(TestBattle.Attack(2, 3));
            Assert.AreEqual(3, runtime.RoundNumber);
            List<BattleState> statuses = runtime.GetUnit(3).Statuses;
            Assert.AreEqual(1, statuses.Count);
            Assert.AreEqual(BattleStateType.Stun, statuses[0].Type);
            Assert.AreEqual(1, statuses[0].RemainingRounds);
        }

        [Test]
        public void EnemyStunsPlayer_PlayerSkipsTurn()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            // 敌人眩晕技能 205：SingleEnemy，Stun(1)，MpCost = 10
            config.AddAction(TestConfigs.SkillAction(205, BattleTargetType.SingleEnemy, 0,
                BattleStatType.None, 0, 10, BattleStatType.None, BattleStateType.Stun, 1));
            config.AddEnemy(TestConfigFactory.Create<EnemyConfig>(
                "Id", 3001, "Name", "眩晕敌", "MaxHp", 50, "MaxMp", 20, "Atk", 8, "Mat", 0,
                "Speed", 20, "ThreatLevelId", 1, "AiType", EnemyAiType.Random,
                "ActionIds", new List<int> { 205 }, "DropTableId", 0));

            // 敌人速度 20 > 玩家 10，敌人先手眩晕玩家
            PlayerUnitState player = TestBattle.Player(speed: 10);
            BattleRuntime runtime = TestBattle.Create1v1(player, config);
            BattleStep step = runtime.AdvanceEnemyTurn();

            // 施加事件 + 玩家跳过事件；进入第 2 轮敌人行动
            Assert.AreEqual(2, step.Events.Count);
            Assert.AreEqual(205, step.Events[0].ActionConfigId);
            Assert.AreEqual(1, step.Events[0].TargetUnitId);
            Assert.AreEqual(BattleStateType.Stun, step.Events[1].StatusType);
            Assert.AreEqual(0, step.Events[1].StatusRemainingRounds);

            Assert.AreEqual(2, runtime.RoundNumber);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            Assert.IsEmpty(runtime.GetUnit(1).Statuses);
            Assert.AreEqual(10, runtime.GetUnit(2).CurrentMp);
        }

        [Test]
        public void Speed_SlowUnactedEnemy_ReordersRemainingTurn()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            config.AddParty(TestConfigs.Party(4002, 3001));
            // 技能 107：SingleEnemy，减速 15，MpCost = 10
            config.AddAction(TestConfigs.SkillAction(107, BattleTargetType.SingleEnemy, -15,
                BattleStatType.None, 0, 10, BattleStatType.Speed));

            // 敌人速度 20：A 减速 15 -> 5，低于 B（10），剩余顺序立即刷新为 B 先行
            BattleRuntime runtime = BuildTwoVersusOne(config, enemySpeed: 20, slowActionId: 107);
            Assert.AreEqual(1, runtime.CurrentActorUnitId);

            BattleStep step = runtime.SubmitCommand(TestBattle.Skill(1, 107, 3));

            Assert.AreEqual(5, runtime.GetUnit(3).Speed);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            Assert.AreEqual(30, runtime.GetUnit(1).CurrentMp);
            Assert.AreEqual(1, step.Events.Count);
        }

        [Test]
        public void Speed_SlowEnemy_ReducesSpeedValue()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            // 真实配表 101：SingleEnemy，减速 5，MpCost = 10
            PlayerUnitState player = TestBattle.Player(speed: 20, skillActionId: 101);
            BattleRuntime runtime = TestBattle.Create1v1(player, config);

            BattleStep step = runtime.SubmitCommand(TestBattle.Skill(1, 101, 2));

            Assert.AreEqual(3, runtime.GetUnit(2).Speed);
            Assert.AreEqual(50, runtime.GetUnit(2).CurrentHp);
            Assert.AreEqual(30, runtime.GetUnit(1).CurrentMp);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            Assert.AreEqual(1, step.Events.Count);
        }

        [Test]
        public void Speed_BuffedActedUnit_DoesNotActAgain()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            config.AddParty(TestConfigs.Party(4002, 3001));
            // 敌人自加速技能 204：Self，加速 50，无消耗
            config.AddAction(TestConfigs.SkillAction(204, BattleTargetType.Self, 50,
                BattleStatType.None, 0, 0, BattleStatType.Speed));
            config.AddEnemy(TestConfigFactory.Create<EnemyConfig>(
                "Id", 3001, "Name", "加速敌", "MaxHp", 100, "MaxMp", 0, "Atk", 8, "Mat", 0,
                "Speed", 20, "ThreatLevelId", 1, "AiType", EnemyAiType.Random,
                "ActionIds", new List<int> { 204 }, "DropTableId", 0));

            PlayerUnitState pA = TestBattle.Player(speed: 30, partyOrder: 1, characterId: 1001);
            PlayerUnitState pB = TestBattle.Player(currentHp: 120, maxHp: 120, speed: 10, partyOrder: 2, characterId: 1002);
            BattleEncounter encounter = new BattleEncounter(1, 4002, false);
            BattleRuntime runtime = BattleRuntime.Create(encounter, new List<PlayerUnitState> { pA, pB }, config, new TestRandomSource());

            // 第 1 轮：A 攻击，敌人自加速 20 -> 70
            runtime.SubmitCommand(TestBattle.Attack(1, 3));
            runtime.AdvanceEnemyTurn();
            Assert.AreEqual(70, runtime.GetUnit(3).Speed);

            // 已行动单位加速后本轮不再行动，轮到 B
            Assert.AreEqual(1, runtime.RoundNumber);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);

            // B 行动后进入第 2 轮，加速后的敌人先行
            runtime.SubmitCommand(TestBattle.Attack(2, 3));
            Assert.AreEqual(2, runtime.RoundNumber);
            Assert.AreEqual(3, runtime.CurrentActorUnitId);
        }

        [Test]
        public void Speed_ReducedBelowOne_ClampsToOne()
        {
            TestConfigProvider config = TestConfigProvider.StandardWithSkills();
            // 技能 108：SingleEnemy，减速 20，MpCost = 10
            config.AddAction(TestConfigs.SkillAction(108, BattleTargetType.SingleEnemy, -20,
                BattleStatType.None, 0, 10, BattleStatType.Speed));

            PlayerUnitState player = TestBattle.Player(speed: 20, skillActionId: 108);
            BattleRuntime runtime = TestBattle.Create1v1(player, config);

            runtime.SubmitCommand(TestBattle.Skill(1, 108, 2));

            // 敌人 8 - 20 = -12，钳制为 1，仍可正常行动
            Assert.AreEqual(1, runtime.GetUnit(2).Speed);
            Assert.AreEqual(2, runtime.CurrentActorUnitId);
            BattleStep enemyStep = runtime.AdvanceEnemyTurn();
            Assert.AreEqual(1, enemyStep.Events.Count);
        }

        private static BattleRuntime BuildTwoVersusOne(TestConfigProvider config, int enemySpeed,
            int slowActionId)
        {
            config.AddEnemy(TestConfigFactory.Create<EnemyConfig>(
                "Id", 3001, "Name", "敌人3001", "MaxHp", 50, "MaxMp", 0, "Atk", 8, "Mat", 0,
                "Speed", enemySpeed, "ThreatLevelId", 1, "AiType", EnemyAiType.Random,
                "ActionIds", new List<int> { 201 }, "DropTableId", 0));

            PlayerUnitState pA = TestBattle.Player(speed: 30, partyOrder: 1, characterId: 1001, skillActionId: slowActionId);
            PlayerUnitState pB = TestBattle.Player(currentHp: 120, maxHp: 120, speed: 10, partyOrder: 2, characterId: 1002);
            BattleEncounter encounter = new BattleEncounter(1, 4002, false);
            return BattleRuntime.Create(encounter, new List<PlayerUnitState> { pA, pB }, config, new TestRandomSource());
        }
    }
}
