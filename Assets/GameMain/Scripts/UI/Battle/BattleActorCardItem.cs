using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public GameObject ActiveMarker => activeMarker;
        public Image Icon => icon;
        public Image HpFill => hpFill;
        public Image MpFill => mpFill;
        public TextMeshProUGUI CharacterName => characterName;
        public TextMeshProUGUI HpText => hpText;
        public TextMeshProUGUI MpText => mpText;

#if UNITY_EDITOR
        public void ConfigureEditor(GameObject marker, Image iconImage, Image hpFillImage, Image mpFillImage,
            TextMeshProUGUI nameLabel, TextMeshProUGUI hpLabel, TextMeshProUGUI mpLabel)
        {
            activeMarker = marker;
            icon = iconImage;
            hpFill = hpFillImage;
            mpFill = mpFillImage;
            characterName = nameLabel;
            hpText = hpLabel;
            mpText = mpLabel;
        }
#endif
    }
}
