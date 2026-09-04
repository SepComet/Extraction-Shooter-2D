using SepCore.Exploration;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 虚拟摇杆与交互按钮界面逻辑（手写 partial，与自动生成的 JoystickForm.cs 合并）。
    /// 负责将界面上的 VariableJoystick 与交互按钮组件包装为 JoystickCharacterInput，
    /// 并在界面打开和关闭时向全局 CharacterInputBridge 注册与注销。
    /// </summary>
    public partial class JoystickForm : UGuiForm
    {
        private JoystickCharacterInput _inputSource = null;
        private UIInteractButtonListener _interactListener => View.interactListener;

        /// <summary>
        /// 当前界面提供的角色输入源实例。
        /// </summary>
        public ICharacterInput InputSource => _inputSource;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            _inputSource = new JoystickCharacterInput(
                View.joystick,
                () => _interactListener != null && _interactListener.IsHeld,
                () => _interactListener != null && _interactListener.TriggeredThisFrame,
                () => _interactListener != null && _interactListener.ReleasedThisFrame);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (_inputSource != null)
            {
                CharacterInputBridge.RegisterUIInput(_inputSource);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            CharacterInputBridge.UnregisterUIInput();

            base.OnClose(isShutdown, userData);
        }
    }
}
