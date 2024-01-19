using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours
{
    public class ModListTooltipPanel : MonoBehaviour
    {
        public static ModListTooltipPanel? Instance;

        // UI generation settings
        private static readonly bool PreventFromClosing = false; // for debugging visuals
        private static readonly Vector2 NotificationWidth = new Vector2(0.75f, 1.1f);
        private static readonly float HeaderSpacing = 12f;
        private static readonly float TextSpacing = 11f;
        private static readonly int MaxLines = 10;

        private RectTransform? _panelTransform;
        private TextMeshProUGUI? _titleText;

        // Needed for mod diff text generation
        private TextMeshProUGUI? _headerTextTemplate;
        private TextMeshProUGUI? _textTemplate;
        private List<TextMeshProUGUI> _existingText = new();

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
            _panelTransform = panelImage?.rectTransform;
            if (panelImage == null || _panelTransform == null)
                return;

            // Increase panel opacity to 100% since we disabled the background image
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);
            _panelTransform.anchorMin = new Vector2(0, 1);
            _panelTransform.anchorMax = new Vector2(0, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(_panelTransform, NotificationWidth);
            UIHelper.TryMultiplySizeDelta(_panelTransform.Find("Image"), NotificationWidth);
            UIHelper.TryMultiplySizeDelta(_panelTransform.Find("NotificationText"), NotificationWidth);

            // Remove "dismiss" button
            _panelTransform.Find("ResponseButton")?.gameObject.SetActive(false);

            // Set to screen's top left corner temporarily and disable panel
            _panelTransform.anchoredPosition = new Vector2(_panelTransform.sizeDelta.x / 2, -_panelTransform.sizeDelta.y / 2);
            _panelTransform.gameObject.SetActive(false);

            SetupText(_panelTransform);
        }

        // This could/should eventually be moved to a UIHelper method if we want this to look identical to the full modlist panel
        private void SetupText(RectTransform panelTransform)
        {
            _titleText = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (_titleText == null)
                return;

            _titleText.fontSizeMax = 13f;
            _titleText.fontSizeMin = 12f;
            _titleText.rectTransform.anchoredPosition = new Vector2(0, 95f);

            // Setup text as template
            _headerTextTemplate = UIHelper.SetupTextAsTemplate(_titleText, _titleText.color, new Vector2(165f, 75f), 13f, 2f, HorizontalAlignmentOptions.Center);
            _textTemplate = UIHelper.SetupTextAsTemplate(_titleText, _titleText.color, new Vector2(165f, 75f), 13f, 2f, HorizontalAlignmentOptions.Left);

            // Make the title wrap with compact line spacing
            _titleText.lineSpacing = -20f;
            _titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            _titleText.rectTransform.sizeDelta = new Vector2(170f, 75f);
        }

        public void DisplayNotification(LobbyDiff lobbyDiff, RectTransform elementTransform, Transform elementContainerTransform)
        {
            if (_panelTransform == null)
                return;

            // Turn relative position into something we can use
            Vector2 hoverPanelPosition = RectTransformUtility.CalculateRelativeRectTransformBounds(elementContainerTransform, elementTransform).center;

            // If the element is more than ~halfway down the screen, the tooltip should go upwards instead of down
            // TODO: Remove magic number
            bool alignWithBottom = hoverPanelPosition.y > -190f;

            // Add the panel's width/height so it's not offset
            hoverPanelPosition += new Vector2(_panelTransform.sizeDelta.x, alignWithBottom ? -_panelTransform.sizeDelta.y : 0);

            // Add the button size as an additional offset so it's not in the center of the button
            hoverPanelPosition += new Vector2(elementTransform.sizeDelta.x / 2, alignWithBottom ? -elementTransform.sizeDelta.y / 2 : 0);

            // Add a very small amount of padding
            // TODO: Replace this with a non-magic number
            hoverPanelPosition += new Vector2(-15f, alignWithBottom ? 4f : 6f);
            _panelTransform.anchoredPosition = hoverPanelPosition;
            _panelTransform.gameObject.SetActive(true);

            DisplayModList(lobbyDiff);
        }

        private void DisplayModList(LobbyDiff lobbyDiff)
        {
            if (_panelTransform == null || _titleText == null || _headerTextTemplate == null || _textTemplate == null)
                return;

            var incompatibleModsCount = lobbyDiff.PluginDiffs.Where(x => x.Required && x.CompatibilityResult != CompatibilityResult.Compatible).ToList().Count;
            var incompatibleMods = $"({incompatibleModsCount})";
            
            // Make the incompatible count red if there are any incompatible mods
            if (incompatibleModsCount > 0)
                incompatibleMods = $"<color=red>{incompatibleMods}</color>";

            _titleText.text = $"{lobbyDiff.LobbyCompatibilityDisplayName}\nTotal Mods: ({lobbyDiff.PluginDiffs.Count})\nIncompatible Mods: {incompatibleMods}\n========================";

            // clear old text
            foreach (var text in _existingText)
            {
                if (text == null)
                    continue;

                Destroy(text.gameObject);
            }
            _existingText.Clear();

            // Generate text based on LobbyDiff
            var (newText, padding, pluginsShown) = UIHelper.GenerateTextFromDiff(lobbyDiff, _textTemplate, _headerTextTemplate, TextSpacing, HeaderSpacing, -51.5f, true, MaxLines);

            // Add cutoff text if necessary
            var remainingPlugins = lobbyDiff.PluginDiffs.Count - pluginsShown;
            if (newText.Count >= MaxLines && remainingPlugins > 0)
            {
                var cutoffText = UIHelper.CreateTextFromTemplate(_textTemplate, $"{lobbyDiff.PluginDiffs.Count - pluginsShown} more mods...", -padding, Color.gray);
                newText.Add(cutoffText);
            }

            _existingText.AddRange(newText); // probably doesn't need to be an AddRange since we just deleted stuff
        }

        public void HideNotification()
        {
            if (_panelTransform == null || PreventFromClosing)
                return;

            _panelTransform.gameObject.SetActive(false);
        }
    }
}
