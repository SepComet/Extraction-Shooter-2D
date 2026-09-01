using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SepCore.UI
{
    [DisallowMultipleComponent]
    public sealed class BattleTurnSlotItem : MonoBehaviour
    {
        [SerializeField] private GameObject activeMarker;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;

        public GameObject ActiveMarker => activeMarker;
        public Image Icon => icon;
        public TextMeshProUGUI Label => label;

#if UNITY_EDITOR
        public void ConfigureEditor(GameObject marker, Image iconImage, TextMeshProUGUI slotLabel)
        {
            activeMarker = marker;
            icon = iconImage;
            label = slotLabel;
        }
#endif
    }
}
