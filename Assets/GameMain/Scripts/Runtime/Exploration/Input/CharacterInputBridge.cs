using UnityEngine;

namespace SepCore.Exploration
{
    /// <summary>
    /// 全局输入桥接中心。
    /// 维护默认的复合输入实例，供未单独注入输入源的角色控制器读取，
    /// 同时允许 UI 界面（如 JoystickForm）在开启与关闭时动态挂载与卸载输入源。
    /// </summary>
    public static class CharacterInputBridge
    {
        private static readonly CompositeCharacterInput s_CompositeInput = new CompositeCharacterInput();
        private static readonly LegacyCharacterInput s_LegacyInput = new LegacyCharacterInput();
        private static ICharacterInput s_ActiveUiInput;

        static CharacterInputBridge()
        {
            s_CompositeInput.AddSource(s_LegacyInput);
        }

        /// <summary>
        /// 默认全局复合输入源。
        /// </summary>
        public static ICharacterInput DefaultInput => s_CompositeInput;

        /// <summary>
        /// 全局内置的旧版键鼠输入源。
        /// </summary>
        public static LegacyCharacterInput LegacyInput => s_LegacyInput;

        /// <summary>
        /// 当前已注册的 UI 虚拟输入源（如未注册则为 null）。
        /// </summary>
        public static ICharacterInput ActiveUiInput => s_ActiveUiInput;

        /// <summary>
        /// 注册 UI 虚拟输入源（如 JoystickForm 开启时调用）。
        /// </summary>
        public static void RegisterUIInput(ICharacterInput uiInput)
        {
            if (s_ActiveUiInput != null)
            {
                s_CompositeInput.RemoveSource(s_ActiveUiInput);
            }

            s_ActiveUiInput = uiInput;
            if (uiInput != null)
            {
                s_CompositeInput.AddSource(uiInput);
            }
        }

        /// <summary>
        /// 注销 UI 虚拟输入源（如 JoystickForm 关闭时调用）。
        /// </summary>
        public static void UnregisterUIInput()
        {
            if (s_ActiveUiInput != null)
            {
                s_CompositeInput.RemoveSource(s_ActiveUiInput);
                s_ActiveUiInput = null;
            }
        }

        /// <summary>
        /// 重置桥接状态（主要用于测试复位）。
        /// </summary>
        public static void Reset()
        {
            s_CompositeInput.ClearSources();
            s_ActiveUiInput = null;
            s_CompositeInput.AddSource(s_LegacyInput);
        }
    }
}
