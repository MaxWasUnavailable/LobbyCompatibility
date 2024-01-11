using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours
{
    // tooltip, essentially
    public class HoverNotification : MonoBehaviour
    {
        public static HoverNotification? Instance;

        // Used for getting the relative transform for panel position calculation
        private Transform? parentContainer;
        private RectTransform? panel;
        private TextMeshProUGUI? text;

        private void Awake()
        {
            Instance = this;
        }

        public void Setup(RectTransform panel, TextMeshProUGUI text)
        {
            this.panel = panel;
            this.text = text;
        }

        public void DisplayNotification(Lobby lobby, RectTransform elementTransform, Transform elementContainerTransform)
        {
            if (panel == null)
                return;
            // Turn relative position into something we can use
            Vector2 hoverPanelPosition = RectTransformUtility.CalculateRelativeRectTransformBounds(elementContainerTransform, elementTransform).center;

            // If the element is more than ~halfway down the screen, the tooltip should go upwards instead of down
            bool alignWithBottom = hoverPanelPosition.y > -200f;

            // Add the panel's width/height so it's not offset
            hoverPanelPosition += new Vector2(panel.sizeDelta.x, alignWithBottom ? -panel.sizeDelta.y : 0);

            // Add the button size as an additional offset so it's not in the center of the button
            hoverPanelPosition += new Vector2(elementTransform.sizeDelta.x / 2, alignWithBottom ? -elementTransform.sizeDelta.y / 2 : 0);

            // Add a very small amount of padding
            hoverPanelPosition += new Vector2(4f, alignWithBottom ? -4f : -4f);
            panel.anchoredPosition = hoverPanelPosition;
            panel.gameObject.SetActive(true);
            
            // set text based on lobby data here
        }

        public void HideNotification()
        {
            if (panel == null)
                return;

            panel.gameObject.SetActive(false);
        }
    }
}
