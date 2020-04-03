using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCharacter
{
    public class CustomCharacterControllerUI : MonoBehaviour
    {
        public RectTransform barPanel;
        public RectTransform healthBarBottom;
        public RectTransform healthBar;
        public RectTransform staminaBarBottom;
        public RectTransform staminaBar;

        public Vector2 barPanelScreenPos;
        public float barHeight;
        public float barLength;
        public float barDistance;

        CustomCharacterStats stats;

        void Start()
        {
            stats = GetComponent<CustomCharacterStats>();
            barPanel.anchoredPosition = barPanelScreenPos;
            barPanel.sizeDelta = new Vector2(barLength, 2 * barHeight + barDistance);
            staminaBarBottom.anchoredPosition = new Vector2(0, 0);
            staminaBarBottom.sizeDelta = new Vector2(0, barHeight);
            healthBarBottom.anchoredPosition = new Vector2(0, barHeight + barDistance);
            healthBarBottom.sizeDelta = new Vector2(0, barHeight);
        }

        void Update()
        {
            staminaBar.sizeDelta = new Vector2(stats._stamina.percent * barLength, 0);
            healthBar.sizeDelta = new Vector2(stats._health.percent * barLength, 0);
        }
    }
}
