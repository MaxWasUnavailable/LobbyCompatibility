using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Behaviours
{
    public class ModdedLobbySlot : MonoBehaviour
    {
        private LobbySlot? _lobbySlot;
        private LobbyDiff? _lobbyDiff;
        private Transform? _parentContainer;
        private RectTransform? _buttonTransform;
        private ButtonEventHandler? _buttonEventHandler;

        // Runs after LobbySlot data set
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
            var sprite = GetLobbySprite(_lobbyDiff.LobbyType);
            var invertedSprite = GetLobbySprite(_lobbyDiff.LobbyType, true);

            if (joinButton != null && sprite != null && invertedSprite != null && _lobbySlot.LobbyName != null)
            {
                // Shift player count to the right to make space for our "Mod Settings" button
                playerCount.transform.localPosition = new Vector3(32f, playerCount.transform.localPosition.y, playerCount.transform.localPosition.z);

                // Create the actual modlist button to the left of the player count text
                CreateModListButton(joinButton, sprite, invertedSprite, _lobbySlot.LobbyName.color, playerCount.transform);
            }
        }

        // Unsubscribe to button events when destroyed
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
        /// <param name="transform"> The LobbySlot's parent container. Equivalent to the ScrollRect's viewport. </param>
        public void SetParentContainer(Transform transform)
        {
            _parentContainer = transform;
        }

        // Clone the "Join" button into a new modlist button we can use
        private Button? CreateModListButton(Button original, Sprite sprite, Sprite invertedSprite, Color color, Transform parent)
        {
            if (_lobbyDiff == null)
                return null;

            var button = Instantiate(original, parent);
            _buttonTransform = button.GetComponent<RectTransform>();

            var buttonImageTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
            var buttonImage = buttonImageTransform?.GetComponent<Image>();

            if (_buttonTransform == null || buttonImageTransform == null || buttonImage == null) 
                return null;

            SetupButtonPositioning(_buttonTransform);
            SetupButtonImage(buttonImageTransform, buttonImage);

            // Disable "Join" text
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.enabled = false;
            }

            // Clear "join" event added in unity editor
            button.onClick.m_PersistentCalls.Clear();

            // Disable default button hover/click transition
            button.transition = Selectable.Transition.None;
            button.animator.enabled = false;

            // Get brighter color for noncompatible lobbies
            var incompatibleColor = buttonText?.color ?? color;

            // Inject custom event handling
            _buttonEventHandler = button.gameObject.AddComponent<ButtonEventHandler>();
            _buttonEventHandler.SetButtonImageData(buttonImage, sprite, invertedSprite, _lobbyDiff.LobbyType == ModdedLobbyType.Compatible ? color : incompatibleColor, _lobbyDiff.LobbyType == ModdedLobbyType.Compatible ? color : incompatibleColor);
            _buttonEventHandler.OnHoverStateChanged += OnModListHoverStateChanged;
            _buttonEventHandler.OnClick += OnModListClick;

            return button;
        }

        private void OnModListClick()
        {
            if (_lobbyDiff == null || ModListPanel.Instance == null)
                return;

            ModListPanel.Instance.DisplayNotification(_lobbyDiff);
        }

        // Handles displaying tooltips
        private void OnModListHoverStateChanged(bool hovered)
        {
            if (_lobbyDiff == null || _buttonTransform == null || _parentContainer == null || ModListTooltipPanel.Instance == null)
                return;

            if (hovered)
            {
                ModListTooltipPanel.Instance.DisplayNotification(_lobbyDiff, _buttonTransform, _parentContainer);
            }
            else
            {
                ModListTooltipPanel.Instance.HideNotification();
            }
        }

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

        // A bit hardcoded right now, will need to be redone once we have proper lobby data enums
        private Sprite? GetLobbySprite(ModdedLobbyType lobbyType, bool inverted = false)
        {
            string path = "LobbyCompatibility.Resources.";
            if (inverted)
                path += "Inverted";

            if (lobbyType == ModdedLobbyType.Compatible)
                path += "ModSettings";
            else if (lobbyType == ModdedLobbyType.Incompatible)
                path += "ModSettingsExclamationPoint";
            else
                path += "ModSettingsQuestionMark";

            return TextureHelper.FindSpriteInAssembly(path + ".png");
        }
    }
}
