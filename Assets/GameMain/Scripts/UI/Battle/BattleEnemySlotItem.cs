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

        public Button TargetButton => targetButton;
        public GameObject SelectedMarker => selectedMarker;
        public Image Icon => icon;
        public Image HpFill => hpFill;
        public TextMeshProUGUI EnemyName => enemyName;
        public TextMeshProUGUI HpText => hpText;

#if UNITY_EDITOR
        public void ConfigureEditor(Button button, GameObject marker, Image iconImage, Image hpFillImage,
            TextMeshProUGUI nameLabel, TextMeshProUGUI hpLabel)
        {
            targetButton = button;
            selectedMarker = marker;
            icon = iconImage;
            hpFill = hpFillImage;
            enemyName = nameLabel;
            hpText = hpLabel;
        }
#endif
    }
}
