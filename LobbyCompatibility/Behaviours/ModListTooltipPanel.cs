using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours
{
    public class ModListTooltipPanel : MonoBehaviour
    {
        private static readonly Vector2 notificationWidth = new Vector2(0.6f, 1f);

        public static ModListTooltipPanel? Instance;

        private RectTransform? panelTransform;
        private TextMeshProUGUI? text;

        private void Awake()
        {
            Instance = this;

            // Disable invisible background image (allows us to still interact with below UI elements with the hover menu open)
            var invisibleBackgroundImage = gameObject.GetComponent<Image>();
            if (invisibleBackgroundImage != null)
                invisibleBackgroundImage.enabled = false;

            // Disable background image
            var backgroundImage = transform.Find("Image")?.GetComponent<Image>();
            if (backgroundImage != null)
                backgroundImage.enabled = false;

            // Find actual alert panel so we can modify it
            var panel = transform.Find("Panel")?.GetComponent<Image>();
            panelTransform = panel?.GetComponent<RectTransform>();
            if (panel == null || panelTransform == null)
                return;

            // Increase panel opacity to 100% since we disabled the background image
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 1);
            panelTransform.anchorMin = new Vector2(0, 1);
            panelTransform.anchorMax = new Vector2(0, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(panelTransform, notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("Image"), notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("NotificationText"), notificationWidth);

            // Remove "dismiss" button
            panelTransform.Find("ResponseButton")?.gameObject.SetActive(false);

            // Set to screen's top left corner temporarily and disable panel
            panelTransform.anchoredPosition = new Vector2(panelTransform.sizeDelta.x / 2, -panelTransform.sizeDelta.y / 2);
            panelTransform.gameObject.SetActive(false);

            SetupText(panelTransform);
        }

        // This could/should eventually be moved to a UIHelper method if we want this to look identical to the full modlist panel
        private void SetupText(RectTransform panelTransform)
        {
            text = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                // Move text up & make smaller
                text.fontSizeMax = 15f;
                text.fontSizeMin = 12f;
                text.rectTransform.anchoredPosition = new Vector2(-2f, 75f);
            }
        }

        public void DisplayNotification(Lobby lobby, ModdedLobbyType lobbyType, RectTransform elementTransform, Transform elementContainerTransform)
        {
            if (panelTransform == null)
                return;

            // Turn relative position into something we can use
            Vector2 hoverPanelPosition = RectTransformUtility.CalculateRelativeRectTransformBounds(elementContainerTransform, elementTransform).center;

            // If the element is more than ~halfway down the screen, the tooltip should go upwards instead of down
            bool alignWithBottom = hoverPanelPosition.y > -200f;

            // Add the panel's width/height so it's not offset
            hoverPanelPosition += new Vector2(panelTransform.sizeDelta.x, alignWithBottom ? -panelTransform.sizeDelta.y : 0);

            // Add the button size as an additional offset so it's not in the center of the button
            hoverPanelPosition += new Vector2(elementTransform.sizeDelta.x / 2, alignWithBottom ? -elementTransform.sizeDelta.y / 2 : 0);

            // Add a very small amount of padding
            hoverPanelPosition += new Vector2(4f, alignWithBottom ? -4f : -4f);
            panelTransform.anchoredPosition = hoverPanelPosition;
            panelTransform.gameObject.SetActive(true);

            // TODO: set text based on lobby data here
            if (text != null)
            {
                text.text = $"Mod Status: {GetModdedLobbyText(lobbyType)}";
            }
        }

        public void HideNotification()
        {
            if (panelTransform == null)
                return;

            panelTransform.gameObject.SetActive(false);
        }

        // Enum.GetName is slow
        // TODO: Replace with proper utility
        private string GetModdedLobbyText(ModdedLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case ModdedLobbyType.Compatible:
                    return "Compatible";
                case ModdedLobbyType.Incompatible:
                    return "Incompatible";
                case ModdedLobbyType.Unknown:
                default:
                    return "Unknown";
            }
        }
    }
}
