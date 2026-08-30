using System.Collections.Generic;
using SepCore.Definition;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 编队界面逻辑（手写 partial，与自动生成的 SquadForm.cs 合并）。
    /// 以传入的角色列表重建角色槽列表，模板槽来自 SquadView.characterSlotTemplate。
    /// </summary>
    public partial class SquadForm : UGuiForm
    {
        /// <summary>
        /// 按传入的角色列表重建角色槽列表 UI，每名角色一个槽位。
        /// </summary>
        public void RefreshCharacterList(IReadOnlyList<CharacterSave> characters)
        {
            SquadView squadView = View;
            if (squadView == null || squadView.characterSlotRoot == null ||
                squadView.characterSlotTemplate == null)
            {
                Log.Warning("SquadForm is not fully configured.");
                return;
            }

            CharacterSlotItem template = squadView.characterSlotTemplate;
            template.gameObject.SetActive(false);

            for (int i = squadView.characterSlotRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = squadView.characterSlotRoot.GetChild(i);
                if (child == template.transform)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }

            int characterCount = characters != null ? characters.Count : 0;
            for (int i = 0; i < characterCount; i++)
            {
                CharacterSlotItem slot = Instantiate(template, squadView.characterSlotRoot);
                slot.gameObject.SetActive(true);
                slot.SetCharacter(characters[i]);
            }
        }
    }
}