using SepCore.Definition;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    /// <summary>
    /// 道具详情界面逻辑（手写 partial，与自动生成的 ItemDetailsForm.cs 合并）。
    /// 只提供详情显示能力，由调用方（WarehouseForm）传入物品 ID 后刷新。
    /// </summary>
    public partial class ItemDetailsForm : UGuiForm
    {
        /// <summary>
        /// 按物品 ID 刷新详情界面；配置不存在时清空显示。
        /// </summary>
        public void Refresh(int itemId)
        {
            ItemDetailsView view = View;
            if (view == null)
            {
                Log.Warning("ItemDetailsForm view is not configured.");
                return;
            }

            ItemConfig config = GameEntry.Luban.Get<ItemConfig>(itemId);
            if (config == null)
            {
                Log.Warning("Can not find item config '{0}' for item details.", itemId);
                ClearDetails(view);
                return;
            }

            if (view.itemDetailNameText != null)
            {
                view.itemDetailNameText.text = config.Name;
            }

            if (view.itemDetailTypeText != null)
            {
                view.itemDetailTypeText.text = GetItemTypeName(config.ItemType) + "  /  " + GetRarityName(config.Rarity);
            }

            if (view.hpFormatText != null)
            {
                view.hpFormatText.Set(config.MaxHpBonus);
            }

            if (view.atkFormatText != null)
            {
                view.atkFormatText.Set(config.AtkBonus);
            }

            if (view.mpFormatText != null)
            {
                view.mpFormatText.Set(config.MaxMpBonus);
            }

            if (view.matFormatText != null)
            {
                view.matFormatText.Set(config.MatBonus);
            }

            if (view.speedFormatText != null)
            {
                view.speedFormatText.Set(0);
            }

            if (view.stackFormatText != null)
            {
                view.stackFormatText.Set(config.StackLimit);
            }

            if (view.itemDetailDescriptionText != null)
            {
                view.itemDetailDescriptionText.text = string.Empty;
            }

            if (view.itemDetailIcon != null)
            {
                view.itemDetailIcon.gameObject.SetActive(false);
            }
        }

        private static void ClearDetails(ItemDetailsView view)
        {
            if (view.itemDetailNameText != null)
            {
                view.itemDetailNameText.text = string.Empty;
            }

            if (view.itemDetailTypeText != null)
            {
                view.itemDetailTypeText.text = string.Empty;
            }

            if (view.itemDetailDescriptionText != null)
            {
                view.itemDetailDescriptionText.text = string.Empty;
            }
        }

        private static string GetItemTypeName(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Loot:
                    return "战利品";
                case ItemType.Equipment:
                    return "装备";
                case ItemType.Consumable:
                    return "消耗品";
                default:
                    return "未知";
            }
        }

        private static string GetRarityName(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.White:
                    return "白";
                case Rarity.Green:
                    return "绿";
                case Rarity.Blue:
                    return "蓝";
                case Rarity.Gold:
                    return "金";
                case Rarity.Red:
                    return "红";
                default:
                    return "无";
            }
        }
    }
}