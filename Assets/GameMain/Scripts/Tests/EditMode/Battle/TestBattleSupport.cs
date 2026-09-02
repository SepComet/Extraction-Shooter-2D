using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using SepCore.Battle;
using SepCore.CustomComponent;
using SepCore.Definition;

namespace SepCore.Tests
{
    /// <summary>
    /// 用反射构造 Luban 生成的只读配置行，供测试的假配置提供者使用。
    /// </summary>
    internal static class TestConfigFactory
    {
        public static T Create<T>(params object[] fieldValues)
        {
            T instance = (T)FormatterServices.GetUninitializedObject(typeof(T));
            for (int i = 0; i < fieldValues.Length; i += 2)
            {
                FieldInfo field = typeof(T).GetField((string)fieldValues[i]);
                field.SetValue(instance, fieldValues[i + 1]);
            }

            return instance;
        }
    }

    /// <summary>
    /// 与真实配表数值对齐的测试配置构建器。
    /// </summary>
    internal static class TestConfigs
    {
        /// <summary>
        /// 单体敌方攻击行动。
        /// </summary>
        public static BattleActionConfig AttackAction(int id, int flat, BattleStatType sourceStat,
            int sourceScalePermille, int mpCost = 0)
        {
            return TestConfigFactory.Create<BattleActionConfig>(
                "Id", id, "Name", "攻击" + id, "ActionType", BattleActionType.Attack,
                "TargetType", BattleTargetType.SingleEnemy, "MpCost", mpCost,
                "Effects", new List<BattleEffect>
                {
                    TestConfigFactory.Create<BattleEffect>(
                        "TargetStat", BattleStatType.HP, "FlatValue", flat,
                        "SourceStat", sourceStat, "SourceScalePermille", sourceScalePermille,
                        "Status", BattleStatusType.None, "DurationRounds", 0)
                });
        }

        public static EnemyConfig Enemy(int id, int maxHp, int atk, params int[] actionIds)
        {
            return EnemyWithSpeed(id, maxHp, atk, 8, actionIds);
        }

        /// <summary>
        /// 指定速度的敌人；独立方法名避免与上面 params 重载产生歧义。
        /// </summary>
        public static EnemyConfig EnemyWithSpeed(int id, int maxHp, int atk, int speed, params int[] actionIds)
        {
            return TestConfigFactory.Create<EnemyConfig>(
                "Id", id, "Name", "敌人" + id, "MaxHp", maxHp, "MaxMp", 0, "Atk", atk, "Mat", 0,
                "Speed", speed, "ThreatLevelId", 1, "AiType", EnemyAiType.Random,
                "ActionIds", new List<int>(actionIds), "DropTableId", 0);
        }

        public static EnemyPartyConfig Party(int id, params int[] enemyIds)
        {
            return TestConfigFactory.Create<EnemyPartyConfig>(
                "Id", id, "Name", "队伍" + id, "EnemyIds", new List<int>(enemyIds));
        }
    }

    /// <summary>
    /// 内存配置提供者：EditMode 使用，不依赖 Luban 与 GameEntry。
    /// </summary>
    internal sealed class TestConfigProvider : IBattleConfigProvider
    {
        private readonly Dictionary<int, EnemyPartyConfig> _parties = new Dictionary<int, EnemyPartyConfig>();
        private readonly Dictionary<int, EnemyConfig> _enemies = new Dictionary<int, EnemyConfig>();
        private readonly Dictionary<int, BattleActionConfig> _actions = new Dictionary<int, BattleActionConfig>();
        private GlobalConfig _global;

        public void AddParty(EnemyPartyConfig party)
        {
            _parties[party.Id] = party;
        }

        public void AddEnemy(EnemyConfig enemy)
        {
            _enemies[enemy.Id] = enemy;
        }

        public void AddAction(BattleActionConfig action)
        {
            _actions[action.Id] = action;
        }

        public void SetGlobal(GlobalConfig global)
        {
            _global = global;
        }

        public EnemyPartyConfig GetEnemyParty(int id)
        {
            EnemyPartyConfig value;
            return _parties.TryGetValue(id, out value) ? value : null;
        }

        public EnemyConfig GetEnemy(int id)
        {
            EnemyConfig value;
            return _enemies.TryGetValue(id, out value) ? value : null;
        }

        public BattleActionConfig GetAction(int id)
        {
            BattleActionConfig value;
            return _actions.TryGetValue(id, out value) ? value : null;
        }

        public GlobalConfig GetGlobal()
        {
            return _global;
        }

        /// <summary>
        /// 标准 1v1：行动 1（玩家攻击，伤害 = ATK）、行动 201（敌人攻击，伤害 = ATK）、
        /// 敌人 3001（50 HP，ATK 8，行动 201）、队伍 4001（单个 3001）。
        /// </summary>
        public static TestConfigProvider Standard1v1()
        {
            TestConfigProvider provider = new TestConfigProvider();
            provider.AddAction(TestConfigs.AttackAction(1, 0, BattleStatType.ATK, -1000));
            provider.AddAction(TestConfigs.AttackAction(201, 0, BattleStatType.ATK, -1000));
            provider.AddEnemy(TestConfigs.Enemy(3001, 50, 8, 201));
            provider.AddParty(TestConfigs.Party(4001, 3001));
            return provider;
        }
    }

    /// <summary>
    /// 可注入序列的测试随机源；用尽后返回 minInclusive。
    /// </summary>
    internal sealed class TestRandomSource : IRunRandomSource
    {
        private readonly Queue<int> _values = new Queue<int>();

        public TestRandomSource(params int[] values)
        {
            foreach (int value in values)
            {
                _values.Enqueue(value);
            }
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return _values.Count > 0 ? _values.Dequeue() : minInclusive;
        }

        public bool RollPermille(int successPermille)
        {
            return NextInt(0, 1000) < successPermille;
        }
    }

    /// <summary>
    /// 标准测试输入与指令构造。
    /// </summary>
    internal static class TestBattle
    {
        /// <summary>
        /// 标准玩家：120 HP / 40 MP，ATK 14，攻击行动 1，技能行动 101。
        /// </summary>
        public static RunPlayerState Player(int currentHp = 120, int maxHp = 120, int atk = 14,
            int speed = 12, int partyOrder = 1, int characterId = 1001)
        {
            return new RunPlayerState
            {
                CharacterId = characterId,
                PartyOrder = partyOrder,
                CurrentHp = currentHp,
                CurrentMp = 40,
                MaxHp = maxHp,
                MaxMp = 40,
                Atk = atk,
                Mat = 6,
                Speed = speed,
                AttackActionId = 1,
                SkillActionId = 101
            };
        }

        public static BattleEncounter Encounter(int encounterId = 1, int enemyPartyConfigId = 4001)
        {
            return new BattleEncounter(encounterId, enemyPartyConfigId, false);
        }

        public static BattleCommand Attack(int actorUnitId, int targetUnitId, int actionId = 1)
        {
            return new BattleCommand(actorUnitId, BattleActionType.Attack, actionId,
                new List<int> { targetUnitId });
        }

        /// <summary>
        /// 创建标准 1v1 运行时。
        /// </summary>
        public static BattleRuntime Create1v1(RunPlayerState player, IBattleConfigProvider config = null,
            IRunRandomSource random = null)
        {
            return BattleRuntime.Create(Encounter(), new List<RunPlayerState> { player },
                config ?? TestConfigProvider.Standard1v1(), random ?? new TestRandomSource());
        }
    }
}