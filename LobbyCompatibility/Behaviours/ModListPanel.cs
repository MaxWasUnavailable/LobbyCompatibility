using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;
using Steamworks.Data;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LobbyCompatibility.Models;
using Newtonsoft.Json;

namespace LobbyCompatibility.Behaviours
{
    public class ModListPanel : MonoBehaviour
    {
        private static readonly Vector2 notificationWidth = new Vector2(1.25f, 1.75f);
        private static readonly float headerSpacing = 20f;
        private static readonly float textSpacing = 15f;

        public static ModListPanel? Instance;

        private List<GameObject> existingText = new();

        private RectTransform? panelTransform;
        private TextMeshProUGUI? titleText;

        // Needed for scrolling / content size recalculation
        private ScrollRect? scrollView;

        // Needed for mod diff text generation
        private TextMeshProUGUI? contentTextTemplate;
        private Color defaultTextColor;

        private void Awake()
        {
            Instance = this;
        }

        public void SetupPanel(GameObject panel, Transform scrollViewTemplate)
        {
            var panelImage = panel.transform.Find("Panel")?.GetComponent<Image>();
            panelTransform = panelImage?.rectTransform;

            if (panelImage == null || panelTransform == null)
                return;

            // Get "dismiss" button so we can inject some custom behaviour
            var button = panelTransform.Find("ResponseButton")?.GetComponent<Button>();
            var buttonTransform = button?.GetComponent<RectTransform>();
            if (button == null || buttonTransform == null)
                return;

            titleText = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
                return;

            // set default text color
            defaultTextColor = titleText.color;

            // Move header up
            titleText.rectTransform.anchoredPosition = new Vector2(-2f, 155f);

            SetupScrollView(panelTransform, scrollViewTemplate);

            // Increase panel opacity to 100% so we can't see error messages underneath (if they exist)
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(panelTransform, notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("Image"), notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("NotificationText"), notificationWidth);

            // Set button to be consistently spaced from the bottom of the panel 
            // This is the exact pixel distance the "Back" button is from the bottom on normal panels. TODO: do this dynamically based on notificationWidth
            buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, -110.5f);
            // Inject custom button behaviour so it doesn't force you back to the main menu
            button.onClick.m_PersistentCalls.Clear();
            button.onClick.AddListener(() => { SetPanelActive(false); });

            SetPanelActive(false);
        }

        private void SetupScrollView(RectTransform panelTransform, Transform scrollViewTemplate)
        {
            // Setup ScrollView for panel
            var scrollViewObject = Instantiate(scrollViewTemplate, panelTransform);
            var scrollViewTransform = scrollViewObject.GetComponent<RectTransform>();
            scrollView = scrollViewObject.GetComponent<ScrollRect>();
            var text = scrollViewObject.GetComponentInChildren<TextMeshProUGUI>();
            if (scrollViewTransform == null || scrollView == null || text == null)
                return;

            // Delete lobby manager (not sure why it's on this object?)
            var lobbyManager = scrollViewObject.GetComponentInChildren<SteamLobbyManager>();
            if (lobbyManager != null)
                Destroy(lobbyManager);

            // Set pos/scale
            scrollViewTransform.anchoredPosition = new Vector2(15f, -30f);
            scrollViewTransform.sizeDelta = new Vector2(-30f, -100f);

            // Set scroll to zero
            scrollView.verticalNormalizedPosition = 1f;

            // Use text as template
            text.gameObject.SetActive(false);
            contentTextTemplate = text;
        }

        private void GenerateTextFromDiff(LobbyDiff lobbyDiff)
        {
            if (titleText == null || scrollView?.content == null)
                return;

            titleText.text = $"Mod Status: {MockLobbyHelper.GetModdedLobbyText(lobbyDiff.GetModdedLobbyType())}";

            // sort in the following order:
            // ClientMissingMod -> ServerMissingMod -> ClientModOutdated -> ServerModOutdated -> Compatible
            // Required -> not required
            // i mostly just made that up but it makes sense to me
            // NOTE: All ServerModOutdated/ClientModOutdated results WILL be required

            // clear old text
            foreach (var text in existingText)
            {
                if (text == null)
                    continue;

                Destroy(text);
            }
            existingText.Clear();

            float padding = 0f;
            // should probably be a list
            // padding doesn't really need to be a ref
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ClientMissingMod, true, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ServerMissingMod, true, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ClientModOutdated, true, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ServerModOutdated, true, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ClientMissingMod, false, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.ServerMissingMod, false, ref padding);
            CreateTextFromDiffCategory(lobbyDiff, CompatibilityResult.Compatible, false, ref padding);

            scrollView.content.sizeDelta = new Vector2(0, padding + headerSpacing);
        }

        private void CreateTextFromDiffCategory(LobbyDiff lobbyDiff, CompatibilityResult compatibilityResult, bool? required, ref float padding)
        {
            if (contentTextTemplate == null)
                return;

            // if required == null, any value is fine
            var plugins = lobbyDiff.PluginDiffs.Where(x => x.CompatibilityResult == compatibilityResult && (required == null || x.Required == required)).ToList();
            if (plugins.Count == 0) 
                return;

            // adds a few units of extra header padding
            padding += headerSpacing - textSpacing;

            // Create the category header
            AddText(contentTextTemplate, contentTextTemplate.transform.parent, defaultTextColor, -padding, "Category", HorizontalAlignmentOptions.Center);
            padding += headerSpacing;

            // Add each plugin
            foreach (var plugin in plugins)
            {
                var color = GetTextColorFromPluginDiff(plugin);
                AddText(contentTextTemplate, contentTextTemplate.transform.parent, color, -padding, GetModNameFromPluginDiff(plugin), HorizontalAlignmentOptions.Left);
                padding += textSpacing;
            }
        }

        private string GetModNameFromPluginDiff(PluginDiff pluginDiff)
        {
            var name = $"{pluginDiff.Name}-{pluginDiff.Version}";

            // Add the required version to version-based conflicts
            if ((pluginDiff.CompatibilityResult == CompatibilityResult.ServerModOutdated || pluginDiff.CompatibilityResult == CompatibilityResult.ClientModOutdated) && pluginDiff.RequiredVersion != null)
            {
                name += $" (Need {pluginDiff.RequiredVersion})";
            }
            return name;
        }

        private Color GetTextColorFromPluginDiff(PluginDiff pluginDiff)
        {
            // Nice and bright green if we're compatible
            if (pluginDiff.CompatibilityResult == CompatibilityResult.Compatible)
                return Color.green;

            // Red if we're required and not compatible
            if (pluginDiff.Required)
                return Color.red;

            // Gray if it's not required, but also not compatible
            return Color.gray;
        }

        // Fairly slow but it is what it is 
        private void AddText(TextMeshProUGUI template, Transform parent, Color color, float yPosition, string content, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
        {
            var text = Instantiate(template, parent);
            text.rectTransform.anchoredPosition = new Vector2(0f, yPosition);

            // Set size to not quite take up the full box
            text.rectTransform.sizeDelta = new Vector2(290f, 30f);
            text.text = content;
            text.color = color;

            // Set text alignment / formatting options
            text.horizontalAlignment = alignment;
            text.enableAutoSizing = true;
            text.fontSizeMax = 18.35f;
            text.fontSizeMin = 2f;
            text.enableWordWrapping = false;

            text.gameObject.SetActive(true);
            existingText.Add(text.gameObject);
        }

        public void DisplayNotification(LobbyDiff lobbyDiff)
        {
            if (panelTransform == null || scrollView == null)
                return;

            // Set scroll to zero
            scrollView.verticalNormalizedPosition = 1f;

            SetPanelActive(true);
            // EventSystem.current.SetSelectedGameObject(this.menuNotification.GetComponentInChildren<Button>().gameObject);
            GenerateTextFromDiff(lobbyDiff);
        }

        private void SetPanelActive(bool active)
        {
            if (panelTransform == null)
                return;

            // Disable the parent because it also contains a background image used for blocking raycasts
            panelTransform.parent.gameObject.SetActive(active);
        }
    }
}
