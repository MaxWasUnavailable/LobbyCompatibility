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
using LobbyCompatibility.Patches;

namespace LobbyCompatibility.Behaviours
{
    public class ModdedLobbySlot : MonoBehaviour
    {
        private LobbySlot? lobbySlot;
        private ModdedLobbyType lobbyType;
        private Transform? parentContainer;
        private RectTransform? buttonTransform;
        private ButtonEventHandler? buttonEventHandler;

        // Runs after LobbySlot data set
        private void Start()
        {
            // Not 100% ideal, but I don't want to mess with IL/stack weirdness too much right now
            lobbySlot = GetComponent<LobbySlot>();
            if (lobbySlot == null) return;

            // Get the "Status" of the lobby - is it compatible?
            lobbyType = GetModdedLobbyType(lobbySlot.thisLobby);

            // Find player count text (could be moved/removed in a future update, but unlikely)
            var playerCount = lobbySlot.playerCount;
            if (playerCount == null)
                return;

            // Find "Join Lobby" button template
            var joinButton = GetComponentInChildren<Button>();
            if (joinButton == null)
                return;

            // Get button sprites (depending on the lobby type/status, sometimes it will need to be a warning/alert for incompatible lobbies)
            var sprite = GetLobbySprite(lobbyType);
            var invertedSprite = GetLobbySprite(lobbyType, true);

            if (joinButton != null && sprite != null && invertedSprite != null && lobbySlot.LobbyName != null)
            {
                // Shift player count to the right to make space for our "Mod Settings" button
                playerCount.transform.localPosition = new Vector3(32f, playerCount.transform.localPosition.y, playerCount.transform.localPosition.z);

                // Create the actual modlist button to the left of the player count text
                CreateModListButton(joinButton, sprite, invertedSprite, lobbySlot.LobbyName.color, playerCount.transform);
            }
        }

        // Unsubscribe to button events
        private void OnDestroy()
        {
            if (buttonEventHandler == null) 
                return;

            buttonEventHandler.OnHoverStateChanged -= OnModListHoverStateChanged;
            buttonEventHandler.OnClick -= OnModListClick;
        }

        /// <summary>
        ///     Registers the parent container of the LobbySlot. Required for tooltip position math.
        /// </summary>
        /// <param name="transform"> The LobbySlot's parent container. Equivalent to the ScrollRect's viewport. </param>
        public void SetParentContainer(Transform transform)
        {
            parentContainer = transform;
        }

        // Clone the "Join" button into a new modlist button we can use
        private Button? CreateModListButton(Button original, Sprite sprite, Sprite invertedSprite, Color color, Transform parent)
        {
            var button = Instantiate(original, parent);
            buttonTransform = button.GetComponent<RectTransform>();

            var buttonImageTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
            var buttonImage = buttonImageTransform?.GetComponent<Image>();

            if (buttonTransform == null || buttonImageTransform == null || buttonImage == null) 
                return null;

            SetupButtonPositioning(buttonTransform);
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

            // Inject custom event handling
            buttonEventHandler = button.gameObject.AddComponent<ButtonEventHandler>();
            buttonEventHandler.SetButtonImageData(buttonImage, sprite, invertedSprite, color, color);
            buttonEventHandler.OnHoverStateChanged += OnModListHoverStateChanged;
            buttonEventHandler.OnClick += OnModListClick;

            return button;
        }

        private void OnModListClick()
        {
            if (lobbySlot == null || ModListPanel.Instance == null)
                return;

            LobbyCompatibilityPlugin.Logger?.LogInfo("clicky");
            ModListPanel.Instance.DisplayNotification(lobbySlot.thisLobby, lobbyType);
        }

        // Handles displaying tooltips
        private void OnModListHoverStateChanged(bool hovered)
        {
            if (buttonTransform == null || lobbySlot == null || parentContainer == null || ModListTooltipPanel.Instance == null)
                return;

            if (hovered)
            {
                ModListTooltipPanel.Instance.DisplayNotification(lobbySlot.thisLobby, lobbyType, buttonTransform, parentContainer);
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

        // TODO: Replace with real implementation
        // Just incrementing through lobby types right now for debugging use
        private static int debuggingCount = -1;
        private ModdedLobbyType GetModdedLobbyType(Lobby lobby)
        {
            debuggingCount++;
            switch (debuggingCount % 3)
            {
                case 0:
                    return ModdedLobbyType.Compatible;
                case 1:
                    return ModdedLobbyType.Incompatible;
                case 2:
                default:
                    return ModdedLobbyType.Unknown;
            }
        }
    }
}
