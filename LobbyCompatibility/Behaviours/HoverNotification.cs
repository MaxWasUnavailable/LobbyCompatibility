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

        public void DisplayNotification(Lobby lobby, Transform elementTransform, Transform elementContainerTransform)
        {
            if (panel == null)
                return;

            panel.anchoredPosition = RectTransformUtility.CalculateRelativeRectTransformBounds(elementContainerTransform, elementTransform).center;
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
