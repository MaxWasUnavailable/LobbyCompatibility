using System.Collections.Generic;
using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Mod list panel used to display a lobby's diff.
/// </summary>
public class ModListPanel : MonoBehaviour
{
    public static ModListPanel? Instance;

    private static readonly Vector2 NotificationWidth = new(1.25f, 1.75f);
    private static readonly float HeaderSpacing = 20f;
    private static readonly float TextSpacing = 15f;
    private readonly List<TextMeshProUGUI> _existingText = new();

    // Needed for mod diff text generation
    private TextMeshProUGUI? _headerTextTemplate;

    private RectTransform? _panelTransform;

    // Needed for scrolling / content size recalculation
    private ScrollRect? _scrollView;
    private TextMeshProUGUI? _textTemplate;
    private TextMeshProUGUI? _titleText;

    /// <summary>
    ///     Assign instance on awake so we can access it statically
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    ///     Set up our mod list panel using notification & lobby list objects as donors
    /// </summary>
    /// <param name="panel"> A notification panel to use as a donor </param>
    /// <param name="scrollViewTemplate"> A lobby list's scroll view to use as a donor </param>
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

    /// <summary>
    ///     Set up the scroll view for the mod list panel using a lobby list's scroll view as a donor
    /// </summary>
    /// <param name="panelTransform"> The mod list panel's transform </param>
    /// <param name="scrollViewTemplate"> A lobby list's scroll view to use as a donor </param>
    /// <param name="defaultTextColor"> The default text color to use for the mod list panel </param>
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
        _headerTextTemplate = UIHelper.SetupTextAsTemplate(text, defaultTextColor, new Vector2(290f, 30f), 18.35f, 2f);
        _textTemplate = UIHelper.SetupTextAsTemplate(text, defaultTextColor, new Vector2(290f, 30f), 18.35f, 2f,
            HorizontalAlignmentOptions.Left);
    }

    /// <summary>
    ///     Open the panel and display a lobby's mod list diff
    /// </summary>
    /// <param name="lobbyDiff"> The lobby diff to display </param>
    /// <param name="titleOverride"> Override the title text of the mod list panel </param>
    public void DisplayNotification(LobbyDiff lobbyDiff, string? titleOverride = null)
    {
        if (_scrollView == null)
            return;

        // Set scroll to zero
        _scrollView.verticalNormalizedPosition = 1f;
        SetPanelActive(true);
        // EventSystem.current.SetSelectedGameObject(this.menuNotification.GetComponentInChildren<Button>().gameObject);

        DisplayModList(lobbyDiff, titleOverride);
    }

    /// <summary>
    ///     Display a lobby's mod list diff through the mod list panel
    /// </summary>
    /// <param name="lobbyDiff"> The lobby diff to display </param>
    /// <param name="titleOverride"> Override the title text of the mod list panel </param>
    private void DisplayModList(LobbyDiff lobbyDiff, string? titleOverride = null)
    {
        if (_panelTransform == null || _scrollView?.content == null || _titleText == null ||
            _headerTextTemplate == null || _textTemplate == null)
            return;

        _titleText.text = titleOverride ?? lobbyDiff.GetDisplayText();

        // clear old text
        foreach (var text in _existingText)
        {
            if (text == null)
                continue;

            Destroy(text.gameObject);
        }

        _existingText.Clear();

        // Generate text based on LobbyDiff
        var (newText, padding, pluginsShown) = UIHelper.GenerateTextFromDiff(lobbyDiff, _textTemplate,
            _headerTextTemplate, TextSpacing, HeaderSpacing);
        _existingText.AddRange(newText); // TODO: probably doesn't need to be an AddRange since we just deleted stuff

        // Resize ScrollView to not extend far past the content
        _scrollView.content.sizeDelta =
            new Vector2(0,
                padding + HeaderSpacing); // TODO: could probably be done natively with a contentsizefilter. don't wanna look into it rn
    }

    /// <summary>
    ///     Set the panel's active state
    /// </summary>
    /// <param name="active"> Whether or not the panel should be active </param>
    private void SetPanelActive(bool active)
    {
        if (_panelTransform == null)
            return;

        // Disable the parent because it also contains a background image used for blocking raycasts
        _panelTransform.parent.gameObject.SetActive(active);
    }
}