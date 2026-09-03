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
    public sealed class BattleActorCardItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _activeMarker;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _hpFill;
        [SerializeField] private Image _mpFill;
        [SerializeField] private TextMeshProUGUI _characterName;
        [SerializeField] private FormatTextUI _hpText;
        [SerializeField] private FormatTextUI _mpText;

        private int _iconVersion;
        private int _currentUnitId;
        private Action<int> _onClick;

        public int CurrentUnitId => _currentUnitId;

        private void Awake()
        {
            EnsureButtonListener();
        }

        private void EnsureButtonListener()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnCardButtonClick);
                _button.onClick.AddListener(OnCardButtonClick);
            }
        }

        private void OnCardButtonClick()
        {
            _onClick?.Invoke(_currentUnitId);
        }

        public void SetOnClick(Action<int> onClick)
        {
            _onClick = onClick;
            EnsureButtonListener();
        }

        /// <summary>
        /// 用战斗单位视图填充我方卡片：名称、HP/MP 数值与血条、当前行动者标记和配置图标。
        /// 同一单位复用时不重复加载图标。
        /// </summary>
        public void SetUnit(BattleUnitView unit, bool isCurrentActor, bool isSelectedTarget = false)
        {
            if (unit == null)
            {
                return;
            }

            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.interactable = !unit.IsDefeated && !unit.IsEscaped;
            }

            if (_characterName != null)
            {
                _characterName.text = BattleUnitViewHelper.GetDisplayName(unit);
            }

            if (_hpText != null)
            {
                _hpText.Set(unit.CurrentHp, unit.MaxHp);
            }

            if (_mpText != null)
            {
                _mpText.Set(unit.CurrentMp, unit.MaxMp);
            }

            SetBar(_hpFill, unit.CurrentHp, unit.MaxHp);
            SetBar(_mpFill, unit.CurrentMp, unit.MaxMp);

            if (_activeMarker != null)
            {
                _activeMarker.SetActive(isCurrentActor || isSelectedTarget);
            }

            if (_currentUnitId != unit.BattleUnitId)
            {
                _currentUnitId = unit.BattleUnitId;
                _iconVersion++;
                ShowIconAsync(BattleUnitViewHelper.GetPlayerIconConfig(unit.ConfigId), _iconVersion).Forget();
            }
        }

        /// <summary>
        /// 异步加载角色图标；iconVersion 用于防止复用格子时旧加载结果覆盖新内容。
        /// </summary>
        private async UniTaskVoid ShowIconAsync(SpriteConfig iconConfig, int iconVersion)
        {
            if (iconConfig == null || _icon == null)
            {
                return;
            }

            Sprite sprite = await SpriteLoader.LoadSpriteAsync(iconConfig);
            if (sprite == null || _iconVersion != iconVersion)
            {
                return;
            }

            _icon.sprite = sprite;
            _icon.gameObject.SetActive(true);
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