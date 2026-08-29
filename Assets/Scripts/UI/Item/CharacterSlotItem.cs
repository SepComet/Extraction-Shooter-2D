using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterSlotItem : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Image icon;
        [SerializeField] private Text characterName;
        [SerializeField] private Text hpText;
        [SerializeField] private Text mpText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text atkText;
        [SerializeField] private Text matText;
    }
}