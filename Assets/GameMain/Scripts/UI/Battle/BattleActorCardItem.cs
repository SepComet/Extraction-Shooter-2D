using Cysharp.Threading.Tasks;
using SepCore.AsyncTask;
using SepCore.Battle;
using SepCore.Definition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    [DisallowMultipleComponent]
    public sealed class BattleActorCardItem : MonoBehaviour
    {
        [SerializeField] private GameObject activeMarker;
        [SerializeField] private Image icon;
        [SerializeField] private Image hpFill;
        [SerializeField] private Image mpFill;
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI mpText;

        private int _iconVersion;

        /// <summary>
        /// 用战斗单位视图填充我方卡片：名称、HP/MP 数值与血条、当前行动者标记和配置图标。
        /// </summary>
        public void SetUnit(BattleUnitView unit, bool isCurrentActor)
        {
            if (unit == null)
            {
                return;
            }

            if (characterName != null)
            {
                characterName.text = BattleUnitViewHelper.GetDisplayName(unit);
            }

            if (hpText != null)
            {
                hpText.text = unit.CurrentHp + " / " + unit.MaxHp;
            }

            if (mpText != null)
            {
                mpText.text = unit.CurrentMp + " / " + unit.MaxMp;
            }

            SetBar(hpFill, unit.CurrentHp, unit.MaxHp);
            SetBar(mpFill, unit.CurrentMp, unit.MaxMp);

            if (activeMarker != null)
            {
                activeMarker.SetActive(isCurrentActor);
            }

            _iconVersion++;
            ShowIconAsync(BattleUnitViewHelper.GetPlayerIconConfig(unit.ConfigId), _iconVersion).Forget();
        }

        /// <summary>
        /// 异步加载角色图标；iconVersion 用于防止复用格子时旧加载结果覆盖新内容。
        /// </summary>
        private async UniTaskVoid ShowIconAsync(SpriteConfig iconConfig, int iconVersion)
        {
            if (iconConfig == null || icon == null)
            {
                return;
            }

            Sprite sprite = await SpriteLoader.LoadSpriteAsync(iconConfig);
            if (sprite == null || _iconVersion != iconVersion)
            {
                return;
            }

            icon.sprite = sprite;
            icon.gameObject.SetActive(true);
        }

        private static void SetBar(Image fill, int current, int max)
        {
            if (fill == null)
            {
                return;
            }

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
        }
    }
}