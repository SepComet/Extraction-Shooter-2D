using SepCore.Definition;
using UnityGameFramework.Runtime;

namespace SepCore.Entity
{
    /// <summary>
    /// 资源点实体逻辑。
    /// 当前为最小业务实体，负责挂载与读取资源点静态与生成数据，暂不包含长按搜索等交互逻辑。
    /// </summary>
    public sealed class ResourcePointLogic : EntityBase
    {
        private ResourcePointData _data;
        private ResourcePointConfig _config;

        /// <summary>
        /// 当前绑定的资源点实体数据。
        /// </summary>
        public ResourcePointData Data => _data;

        /// <summary>
        /// 当前资源点对应的 Luban 静态配置。
        /// </summary>
        public ResourcePointConfig Config => _config;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            _data = userData as ResourcePointData;
            if (_data == null)
            {
                Log.Error("Resource point entity data is invalid.");
                return;
            }

            _config = GameEntry.Luban.Get<ResourcePointConfig>(_data.ResourcePointId);
            if (_config == null)
            {
                Log.Error("Resource point config '{0}' is invalid.", _data.ResourcePointId);
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
