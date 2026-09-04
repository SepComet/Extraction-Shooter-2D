using SepCore.Definition;
using UnityGameFramework.Runtime;

namespace SepCore.Entity
{
    /// <summary>
    /// 敌人队伍实体逻辑。
    /// 当前为最小业务实体，负责挂载与读取敌人队伍静态配置及威胁等级，暂不包含巡逻、警惕与进入战斗等交互逻辑。
    /// </summary>
    public sealed class EnemyPartyLogic : EntityBase
    {
        private EnemyPartyData _data;
        private EnemyPartyConfig _config;

        /// <summary>
        /// 当前绑定的敌人队伍实体数据。
        /// </summary>
        public EnemyPartyData Data => _data;

        /// <summary>
        /// 当前敌人队伍对应的 Luban 静态配置。
        /// </summary>
        public EnemyPartyConfig Config => _config;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            _data = userData as EnemyPartyData;
            if (_data == null)
            {
                Log.Error("Enemy party entity data is invalid.");
                return;
            }

            _config = GameEntry.Luban.Get<EnemyPartyConfig>(_data.EnemyPartyId);
            if (_config == null)
            {
                Log.Error("Enemy party config '{0}' is invalid.", _data.EnemyPartyId);
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            _data = null;
            _config = null;
            base.OnHide(isShutdown, userData);
        }
    }
}
