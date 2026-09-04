using UnityGameFramework.Runtime;

namespace SepCore.Entity
{
    /// <summary>
    /// 撤离点实体逻辑。
    /// 当前为最小业务实体，负责挂载撤离点数据与生命周期管理，暂不包含接近交互、撤离倒计时与结算等交互逻辑。
    /// </summary>
    public sealed class EvacuatePointLogic : EntityBase
    {
        private EvacuatePointData _data;

        /// <summary>
        /// 当前绑定的撤离点实体数据。
        /// </summary>
        public EvacuatePointData Data => _data;

        /// <summary>
        /// 撤离点当前是否开放。
        /// </summary>
        public bool IsOpen => _data != null && _data.IsOpen;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            _data = userData as EvacuatePointData;
            if (_data == null)
            {
                Log.Error("Evacuate point entity data is invalid.");
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            _data = null;
            base.OnHide(isShutdown, userData);
        }
    }
}
