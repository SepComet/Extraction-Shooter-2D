using System;
using Cysharp.Threading.Tasks;
using SepCore.AsyncTask;
using SepCore.Battle;
using SepCore.Definition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SepCore.UI
{
    [DisallowMultipleComponent]
    public sealed class BattleEnemySlotItem : MonoBehaviour
    {
        [SerializeField] private Button targetButton;
        [SerializeField] private GameObject selectedMarker;
        [SerializeField] private Image icon;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI enemyName;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI stateText;

        private int _iconVersion;
        private int _currentUnitId;
        private Action<int> _onClick;

        public int CurrentUnitId => _currentUnitId;

        public void SetOnClick(Action<int> onClick)
        {
            _onClick = onClick;
            if (targetButton != null)
            {
                targetButton.onClick.RemoveAllListeners();
                if (_onClick != null)
                {
                    targetButton.onClick.AddListener(() => _onClick(_currentUnitId));
                }
            }
        }

        /// <summary>
        /// 用战斗单位视图填充敌人槽：名称、HP 数值与血条、配置图标和当前行动者标记。
        /// 阵亡或已逃跑目标不可选；同一单位复用时不重复加载图标。
        /// </summary>
        public void SetEnemy(BattleUnitView unit, bool isCurrentActor, bool isSelectedTarget = false)
        {
            if (unit == null)
            {
                return;
            }

            if (enemyName != null)
            {
                enemyName.text = BattleUnitViewHelper.GetDisplayName(unit);
            }

            if (hpText != null)
            {
                hpText.text = unit.CurrentHp + " / " + unit.MaxHp;
            }

            SetBar(hpFill, unit.CurrentHp, unit.MaxHp);

            if (selectedMarker != null)
            {
                selectedMarker.SetActive(isCurrentActor || isSelectedTarget);
            }

            if (targetButton != null)
            {
                targetButton.interactable = !unit.IsDefeated && !unit.IsEscaped;
            }

            // 状态模板只做飘字底板，不常驻显示（眩晕只在轮到该单位时飘一次）
            if (stateText != null)
            {
                stateText.text = string.Empty;
            }

            if (_currentUnitId != unit.BattleUnitId)
            {
                _currentUnitId = unit.BattleUnitId;
                _iconVersion++;
                ShowIconAsync(BattleUnitViewHelper.GetEnemyIconConfig(unit.ConfigId), _iconVersion).Forget();
            }
        }

        /// <summary>
        /// 生成一个独立飘字（伤害数字、状态名）：出现后上浮淡出，不受后续刷新影响。
        /// </summary>
        public void SpawnFloatText(string text)
        {
            FloatText.Spawn(stateText, text);
        }

        /// <summary>
        /// 异步加载敌人图标；iconVersion 用于防止复用格子时旧加载结果覆盖新内容。
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