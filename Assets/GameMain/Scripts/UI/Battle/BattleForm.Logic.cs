using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 战斗界面逻辑（手写 partial，与自动生成的 BattleForm.cs 合并）。
    /// 首版为半透明壳层：只负责展示快照、收集指令与占位反馈；
    /// 不修改战斗数据、不读取存档、不决定掉落，也不承担暂停地图计时等流程职责。
    /// 暂停地图更新由单局流程（RunBattleCoordinator / 探索层）负责。
    /// </summary>
    public partial class BattleForm : UGuiForm
    {
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            View.attackButton.onClick.AddListener(OnAttackButtonClick);
            View.skillButton.onClick.AddListener(OnSkillButtonClick);
            View.itemButton.onClick.AddListener(OnItemButtonClick);
            View.escapeButton.onClick.AddListener(OnEscapeButtonClick);

            // 道具行动保留但首版禁用，不产生 BattleCommand
            View.itemButton.interactable = false;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            View.currentActorText.text = string.Empty;
            Log.Info("BattleForm opened as empty shell.");
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            View.attackButton.onClick.RemoveListener(OnAttackButtonClick);
            View.skillButton.onClick.RemoveListener(OnSkillButtonClick);
            View.itemButton.onClick.RemoveListener(OnItemButtonClick);
            View.escapeButton.onClick.RemoveListener(OnEscapeButtonClick);

            base.OnClose(isShutdown, userData);
        }

        private void OnAttackButtonClick()
        {
            Log.Info("BattleForm placeholder: attack clicked.");
        }

        private void OnSkillButtonClick()
        {
            Log.Info("BattleForm placeholder: skill clicked.");
        }

        private void OnItemButtonClick()
        {
            Log.Info("BattleForm placeholder: item clicked.");
        }

        private void OnEscapeButtonClick()
        {
            Log.Info("BattleForm placeholder: escape clicked.");
        }
    }
}