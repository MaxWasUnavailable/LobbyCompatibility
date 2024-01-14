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
using LobbyCompatibility.Models;
using UnityEngine.UIElements;

namespace LobbyCompatibility.Behaviours
{
    public class ModListTooltipPanel : MonoBehaviour
    {
        public static ModListTooltipPanel? Instance;

        // UI generation settings
        private static readonly bool preventFromClosing = true; // for debugging visuals
        private static readonly Vector2 notificationWidth = new Vector2(0.75f, 1f);
        private static readonly float headerSpacing = 12f;
        private static readonly float textSpacing = 11f;

        private RectTransform? panelTransform;
        private TextMeshProUGUI? titleText;

        // Needed for mod diff text generation
        private TextMeshProUGUI? headerTextTemplate;
        private TextMeshProUGUI? textTemplate;
        private List<TextMeshProUGUI> existingText = new();

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
            var panelImage = transform.Find("Panel")?.GetComponent<Image>();
            panelTransform = panelImage?.rectTransform;
            if (panelImage == null || panelTransform == null)
                return;

            // Increase panel opacity to 100% since we disabled the background image
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);
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
            titleText = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
                return;

            titleText.fontSizeMax = 13f;
            titleText.fontSizeMin = 12f;
            titleText.rectTransform.anchoredPosition = new Vector2(-2f, 85f);

            // Setup text as template
            headerTextTemplate = UIHelper.SetupTextAsTemplate(titleText, titleText.color, new Vector2(165f, 75f), 13f, 2f, HorizontalAlignmentOptions.Center);
            textTemplate = UIHelper.SetupTextAsTemplate(titleText, titleText.color, new Vector2(165f, 75f), 13f, 2f, HorizontalAlignmentOptions.Left);

            // Make the title wrap with compact line spacing
            titleText.lineSpacing = -20f;
            titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            titleText.rectTransform.sizeDelta = new Vector2(170f, 75f);
        }

        public void DisplayNotification(LobbyDiff lobbyDiff, RectTransform elementTransform, Transform elementContainerTransform)
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

            DisplayModList(lobbyDiff);
        }

        private void DisplayModList(LobbyDiff lobbyDiff)
        {
            if (panelTransform == null || titleText == null || headerTextTemplate == null || textTemplate == null)
                return;

            var incompatibleMods = lobbyDiff.PluginDiffs.Where(x => x.Required && x.CompatibilityResult != CompatibilityResult.Compatible).ToList();
            titleText.text = $"{lobbyDiff.LobbyCompatibilityDisplayName}\nIncompatible Mods: ({incompatibleMods.Count})\nTotal Mods: ({lobbyDiff.PluginDiffs.Count})\n==========================";

            // clear old text
            foreach (var text in existingText)
            {
                if (text == null)
                    continue;

                Destroy(text);
            }
            existingText.Clear();

            // Generate text based on LobbyDiff
            var (newText, padding) = UIHelper.GenerateTextFromDiff(lobbyDiff, textTemplate, headerTextTemplate, textSpacing, headerSpacing, -43f, true);
            existingText.AddRange(newText); // probably doesn't need to be an AddRange since we just deleted stuff
        }

        public void HideNotification()
        {
            if (panelTransform == null || preventFromClosing)
                return;

            panelTransform.gameObject.SetActive(false);
        }
    }
}
