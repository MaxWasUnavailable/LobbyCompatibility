using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Custom MonoBehaviour to be added to LobbySlot game objects.
///     Allows us to add our modded lobby functionality to the game.
/// </summary>
public class ModdedLobbySlot : MonoBehaviour
{
    private ButtonEventHandler? _buttonEventHandler;
    private RectTransform? _buttonTransform;
    private LobbyDiff? _lobbyDiff;
    private LobbySlot? _lobbySlot;
    private Transform? _parentContainer;

    // Runs after LobbySlot data set
    // TODO: Review & refactor
    private void Start()
    {
        // Not 100% ideal, but I don't want to mess with IL/stack weirdness too much right now
        _lobbySlot = GetComponent<LobbySlot>();
        if (_lobbySlot == null)
            return;

        // Get the "diff" of the lobby - mock data right now
        _lobbyDiff = MockLobbyHelper.GetDiffFromLobby(_lobbySlot.thisLobby);

        // Find player count text (could be moved/removed in a future update, but unlikely)
        var playerCount = _lobbySlot.playerCount;
        if (playerCount == null)
            return;

        // Find "Join Lobby" button template
        var joinButton = GetComponentInChildren<Button>();
        if (joinButton == null)
            return;

        // Get button sprites (depending on the lobby type/status, sometimes it will need to be a warning/alert for incompatible lobbies)
        var sprite = GetLobbySprite(_lobbyDiff.GetModdedLobbyType());
        var invertedSprite = GetLobbySprite(_lobbyDiff.GetModdedLobbyType(), true);

        if (joinButton != null && sprite != null && invertedSprite != null && _lobbySlot.LobbyName != null)
        {
            // Shift player count to the right to make space for our "Mod Settings" button
            var localPosition = playerCount.transform.localPosition;
            playerCount.transform.localPosition = new Vector3(32f, localPosition.y, localPosition.z);

            // Create the actual modlist button to the left of the player count text
            CreateModListButton(joinButton, sprite, invertedSprite, _lobbySlot.LobbyName.color, playerCount.transform);
        }
    }

    /// <summary>
    ///     Unregisters event handlers when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_buttonEventHandler == null)
            return;

        _buttonEventHandler.OnHoverStateChanged -= OnModListHoverStateChanged;
        _buttonEventHandler.OnClick -= OnModListClick;
    }

    /// <summary>
    ///     Registers the parent container of the LobbySlot. Required for tooltip position math.
    /// </summary>
    /// <param name="parentTransform"> The LobbySlot's parent container. Equivalent to the ScrollRect's viewport. </param>
    public void SetParentContainer(Transform parentTransform)
    {
        _parentContainer = parentTransform;
    }

    /// <summary>
    ///     Clone the "Join" button into a new mod list button we can use.
    /// </summary>
    /// <param name="original"> The original "Join" button. </param>
    /// <param name="sprite"> The sprite to use for the button. </param>
    /// <param name="invertedSprite"> The inverted sprite to use for the button. </param>
    /// <param name="color"> The color to use for the button. </param>
    /// <param name="parent"> The parent transform to attach the button to. </param>
    /// <returns> The new button. </returns>
    private Button? CreateModListButton(Button original, Sprite sprite, Sprite invertedSprite, Color color,
        Transform parent)
    {
        if (_lobbyDiff == null)
            return null;

        var button = Instantiate(original, parent);
        _buttonTransform = button.GetComponent<RectTransform>();

        var buttonImageTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
        var buttonImage = buttonImageTransform?.GetComponent<Image>();

        if (_buttonTransform == null || buttonImageTransform == null || buttonImage == null)
            return null; // TODO: Do we want to throw an exception / log an error / etc... here?

        // Disable text
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) buttonText.enabled = false;

        // Clear original onClick events
        button.onClick.m_PersistentCalls.Clear();

        // Disable default button hover/click transition
        button.transition = Selectable.Transition.None;
        button.animator.enabled = false;

        SetupButtonPositioning(_buttonTransform);
        SetupButtonImage(buttonImageTransform, buttonImage);

        // Get brighter color for incompatible lobbies
        var incompatibleColor = buttonText?.color ?? color;

        // Inject custom event handling
        _buttonEventHandler = button.gameObject.AddComponent<ButtonEventHandler>();
        _buttonEventHandler.SetButtonImageData(buttonImage, sprite, invertedSprite,
            _lobbyDiff.GetModdedLobbyType() == LobbyDiffResult.Compatible ? color : incompatibleColor,
            _lobbyDiff.GetModdedLobbyType() == LobbyDiffResult.Compatible ? color : incompatibleColor);
        _buttonEventHandler.OnHoverStateChanged += OnModListHoverStateChanged;
        _buttonEventHandler.OnClick += OnModListClick;

        return button;
    }

    /// <summary>
    ///     Handles displaying the mod list when the button is clicked.
    /// </summary>
    private void OnModListClick()
    {
        if (_lobbyDiff == null || ModListPanel.Instance == null)
            return;

        ModListPanel.Instance.DisplayNotification(_lobbyDiff);
    }

    /// <summary>
    ///     Handles displaying the mod list tooltip when the button is hovered.
    /// </summary>
    /// <param name="hovered"> Whether or not the button is hovered. </param>
    private void OnModListHoverStateChanged(bool hovered)
    {
        if (_lobbyDiff == null || _buttonTransform == null || _parentContainer == null ||
            ModListTooltipPanel.Instance == null)
            return;

        if (hovered)
            ModListTooltipPanel.Instance.DisplayNotification(_lobbyDiff, _buttonTransform, _parentContainer);
        else
            ModListTooltipPanel.Instance.HideNotification();
    }

    /// <summary>
    ///     Sets up the positioning of the new mod list button.
    /// </summary>
    /// <param name="buttonTransform"> The button's RectTransform. </param>
    private void SetupButtonPositioning(RectTransform buttonTransform)
    {
        // Set positioning of new button (slightly offset left from the player count text)
        buttonTransform.sizeDelta = new Vector2(30f, 30f);
        buttonTransform.offsetMin = new Vector2(-37f, -37f);
        buttonTransform.offsetMax = new Vector2(-9.3f, -7f);
        buttonTransform.anchoredPosition = new Vector2(-85f, -7f);
        buttonTransform.localPosition = new Vector3(-5.5f, 1.25f, 0f);
        buttonTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
    }

    /// <summary>
    ///     Sets up the image of the new mod list button.
    /// </summary>
    /// <param name="buttonImageTransform"> The button's RectTransform. </param>
    /// <param name="buttonImage"> The button's Image. </param>
    private void SetupButtonImage(RectTransform buttonImageTransform, Image buttonImage)
    {
        // Align background highlight with new positioning
        // has to be slightly bigger on the X axis because of weird scaling in the Lobby list
        buttonImageTransform.sizeDelta = new Vector2(31.5f, 30f);
        buttonImageTransform.offsetMin = new Vector2(0f, 0f);
        buttonImageTransform.offsetMax = new Vector2(31.5f, 30f);
        buttonImageTransform.anchoredPosition = new Vector2(14f, 15f);
        buttonImageTransform.localScale = Vector3.one;
        buttonImage.enabled = true;
    }

    // TODO: A bit hardcoded right now, will need to be redone once we have proper lobby data enums
    private Sprite? GetLobbySprite(LobbyDiffResult lobbyDiffResult, bool inverted = false)
    {
        var path = "LobbyCompatibility.Resources.";
        if (inverted)
            path += "Inverted";

        if (lobbyDiffResult == LobbyDiffResult.Compatible)
            path += "ModSettings";
        else if (lobbyDiffResult == LobbyDiffResult.Incompatible)
            path += "ModSettingsExclamationPoint";
        else
            path += "ModSettingsQuestionMark";

        return TextureHelper.FindSpriteInAssembly(path + ".png");
    }
}