using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WarehouseSlotItem : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image rarity;
        [SerializeField] private Text quantityText;
    }
}