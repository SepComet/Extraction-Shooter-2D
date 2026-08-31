using Cysharp.Threading.Tasks;
using SepCore.AsyncTask;
using SepCore.Definition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace SepCore.UI
{
    public class CharacterSlotItem : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private FormatTextUI hpText;
        [SerializeField] private FormatTextUI mpText;
        [SerializeField] private FormatTextUI speedText;
        [SerializeField] private FormatTextUI atkText;
        [SerializeField] private FormatTextUI matText;

        private int _iconVersion = 0;

        /// <summary>
        /// 用存档角色填充格子；角色配置不存在时按空格子显示。
        /// </summary>
        public void SetCharacter(CharacterSave save)
        {
            CharacterConfig config = GameEntry.Luban.Get<CharacterConfig>(save.characterId);
            if (config == null)
            {
                Log.Warning("Can not find character config '{0}' for squad slot.", save.characterId);
                SetEmpty();
                return;
            }

            characterName.text = config.Name;
            hpText.Set(config.MaxHp);
            mpText.Set(config.MaxMp);
            speedText.Set(config.Speed);
            atkText.Set(config.Atk);
            matText.Set(config.Mat);
            _iconVersion++;
            ShowIconAsync(config.Icon_Ref, _iconVersion).Forget();
        }

        /// <summary>
        /// 显示为空格子。
        /// </summary>
        public void SetEmpty()
        {
            _iconVersion++;
            characterName.text = string.Empty;
            hpText.Clear();
            mpText.Clear();
            speedText.Clear();
            atkText.Clear();
            matText.Clear();
            HideIcon();
        }

        private void HideIcon()
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = null;
            icon.gameObject.SetActive(false);
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
    }
}