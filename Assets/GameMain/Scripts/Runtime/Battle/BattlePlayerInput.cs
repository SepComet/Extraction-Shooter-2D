namespace SepCore.Battle
{
    /// <summary>
    /// 玩家输入快照，是进入本场战斗时的值快照。
    /// 装备计算由单局角色快照构建器完成；战斗内核不读取角色存档、装备栏或背包。
    /// </summary>
    public sealed class BattlePlayerInput
    {
        /// <summary>
        /// 角色配置标识；同时用于把结果回写到单局角色状态。
        /// </summary>
        public readonly int CharacterId;

        /// <summary>
        /// 当前战备中的顺序。
        /// </summary>
        public readonly int PartyOrder;

        /// <summary>
        /// 单局临时状态中的当前 HP。
        /// </summary>
        public readonly int CurrentHp;

        /// <summary>
        /// 单局临时状态中的当前 MP。
        /// </summary>
        public readonly int CurrentMp;

        /// <summary>
        /// 已结算装备加成的最终 HP 上限。
        /// </summary>
        public readonly int MaxHp;

        /// <summary>
        /// 已结算装备加成的最终 MP 上限。
        /// </summary>
        public readonly int MaxMp;

        /// <summary>
        /// 已结算装备加成的最终攻击。
        /// </summary>
        public readonly int Atk;

        /// <summary>
        /// 已结算装备加成的最终魔力。
        /// </summary>
        public readonly int Mat;

        /// <summary>
        /// 已结算装备加成的最终速度。
        /// </summary>
        public readonly int Speed;

        /// <summary>
        /// 普通攻击行动配置 ID。
        /// </summary>
        public readonly int AttackActionId;

        /// <summary>
        /// 该角色首版唯一技能行动配置 ID。
        /// </summary>
        public readonly int SkillActionId;

        public BattlePlayerInput(int characterId, int partyOrder, int currentHp, int currentMp,
            int maxHp, int maxMp, int atk, int mat, int speed, int attackActionId, int skillActionId)
        {
            CharacterId = characterId;
            PartyOrder = partyOrder;
            CurrentHp = currentHp;
            CurrentMp = currentMp;
            MaxHp = maxHp;
            MaxMp = maxMp;
            Atk = atk;
            Mat = mat;
            Speed = speed;
            AttackActionId = attackActionId;
            SkillActionId = skillActionId;
        }
    }
}