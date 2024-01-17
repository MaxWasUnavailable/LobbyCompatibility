using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours
{
    public class ModListPanel : MonoBehaviour
    {
        public static ModListPanel? Instance;

        private static readonly Vector2 NotificationWidth = new Vector2(1.25f, 1.75f);
        private static readonly float HeaderSpacing = 20f;
        private static readonly float TextSpacing = 15f;

        private RectTransform? _panelTransform;
        private TextMeshProUGUI? _titleText;

        // Needed for scrolling / content size recalculation
        private ScrollRect? _scrollView;

        // Needed for mod diff text generation
        private TextMeshProUGUI? _headerTextTemplate;
        private TextMeshProUGUI? _textTemplate;
        private List<TextMeshProUGUI> _existingText = new();

        private void Awake()
        {
            Instance = this;
        }

        public void SetupPanel(GameObject panel, Transform scrollViewTemplate)
        {
            var panelImage = panel.transform.Find("Panel")?.GetComponent<Image>();
            _panelTransform = panelImage?.rectTransform;
            if (panelImage == null || _panelTransform == null)
                return;

            // Get "dismiss" button so we can inject some custom behaviour
            var button = _panelTransform.Find("ResponseButton")?.GetComponent<Button>();
            var buttonTransform = button?.GetComponent<RectTransform>();
            if (button == null || buttonTransform == null)
                return;

            _titleText = _panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (_titleText == null)
                return;

            // Move header up
            _titleText.rectTransform.anchoredPosition = new Vector2(-2f, 155f);

            // Initialize scroll view by taking the game's lobby list and modifying it
            SetupScrollView(_panelTransform, scrollViewTemplate, _titleText.color);

            // Increase panel opacity to 100% so we can't see error messages underneath (if they exist)
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(_panelTransform, NotificationWidth);
            UIHelper.TryMultiplySizeDelta(_panelTransform.Find("Image"), NotificationWidth);
            UIHelper.TryMultiplySizeDelta(_panelTransform.Find("NotificationText"), NotificationWidth);

            // Set button to be consistently spaced from the bottom of the panel 
            // This is the exact pixel distance the "Back" button is from the bottom on normal panels. TODO: do this dynamically based on notificationWidth
            buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, -110.5f);

            // Inject custom button behaviour so it doesn't force you back to the main menu
            button.onClick.m_PersistentCalls.Clear();
            button.onClick.AddListener(() => { SetPanelActive(false); });

            SetPanelActive(false);
        }

        private void SetupScrollView(RectTransform panelTransform, Transform scrollViewTemplate, Color defaultTextColor)
        {
            // Setup ScrollView for panel
            var scrollViewObject = Instantiate(scrollViewTemplate, panelTransform);
            var scrollViewTransform = scrollViewObject.GetComponent<RectTransform>();
            _scrollView = scrollViewObject.GetComponent<ScrollRect>();
            var text = scrollViewObject.GetComponentInChildren<TextMeshProUGUI>();
            if (scrollViewTransform == null || _scrollView == null || text == null)
                return;

            // Delete lobby manager (not sure why it's on this object?)
            var lobbyManager = scrollViewObject.GetComponentInChildren<SteamLobbyManager>();
            if (lobbyManager != null)
                Destroy(lobbyManager);

            // Set pos/scale
            scrollViewTransform.anchoredPosition = new Vector2(15f, -30f);
            scrollViewTransform.sizeDelta = new Vector2(-30f, -100f);

            // Reset scroll to default position
            _scrollView.verticalNormalizedPosition = 1f;

            // Setup text as template
            text.gameObject.SetActive(false);
            _headerTextTemplate = UIHelper.SetupTextAsTemplate(text, defaultTextColor, new Vector2(290f, 30f), 18.35f, 2f, HorizontalAlignmentOptions.Center);
            _textTemplate = UIHelper.SetupTextAsTemplate(text, defaultTextColor, new Vector2(290f, 30f), 18.35f, 2f, HorizontalAlignmentOptions.Left);
        }

        public void DisplayNotification(LobbyDiff lobbyDiff)
        {
            if (_scrollView == null)
                return;

            // Set scroll to zero
            _scrollView.verticalNormalizedPosition = 1f;
            SetPanelActive(true);
            // EventSystem.current.SetSelectedGameObject(this.menuNotification.GetComponentInChildren<Button>().gameObject);

            DisplayModList(lobbyDiff);
        }

        private void DisplayModList(LobbyDiff lobbyDiff)
        {
            if (_panelTransform == null || _scrollView?.content == null || _titleText == null || _headerTextTemplate == null || _textTemplate == null)
                return;

            _titleText.text = lobbyDiff.LobbyCompatibilityDisplayName;

            // clear old text
            foreach (var text in _existingText)
            {
                if (text == null)
                    continue;

                Destroy(text.gameObject);
            }
            _existingText.Clear();

            // Generate text based on LobbyDiff
            var (newText, padding, pluginsShown) = UIHelper.GenerateTextFromDiff(lobbyDiff, _textTemplate, _headerTextTemplate, TextSpacing, HeaderSpacing);
            _existingText.AddRange(newText); // probably doesn't need to be an AddRange since we just deleted stuff

            // Resize ScrollView to not extend far past the content
            _scrollView.content.sizeDelta = new Vector2(0, padding + HeaderSpacing); // could probably be done natively with a contentsizefilter. don't wanna look into it rn
        }

        private void SetPanelActive(bool active)
        {
            if (_panelTransform == null)
                return;

            // Disable the parent because it also contains a background image used for blocking raycasts
            _panelTransform.parent.gameObject.SetActive(active);
        }
    }
}
