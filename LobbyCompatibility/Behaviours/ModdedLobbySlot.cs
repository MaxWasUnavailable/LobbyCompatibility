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
using static UnityEngine.UI.Selectable;
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
        public void Start()
        {
            LobbyCompatibilityPlugin.Logger?.LogInfo("Modded lobby awake!");
            // Not 100% ideal, but I don't want to mess with IL/stack weirdness too much right now
            lobbySlot = GetComponent<LobbySlot>();
            if (lobbySlot == null) return;

            lobbyType = GetModdedLobbyType(lobbySlot.thisLobby);

            var numPlayers = lobbySlot.playerCount;
            numPlayers.transform.localPosition = new Vector3(32f, numPlayers.transform.localPosition.y, numPlayers.transform.localPosition.z); // adjust playercount to the right to make button space

            var sprite = GetLobbySprite(lobbyType);
            var joinButton = GetComponentInChildren<Button>(); // Find "Join Lobby" button template

            if (joinButton != null && sprite != null && lobbySlot.LobbyName != null)
            {
                CreateModListButton(joinButton, sprite, lobbySlot.LobbyName.color, numPlayers.transform);
            }
        }

        public void SetParentContainer(Transform transform)
        {
            parentContainer = transform;
        }

        // Create button that displays mod list when clicked 
        // TODO: Hook up to lobby info panel
        private Button? CreateModListButton(Button original, Sprite sprite, Color color, Transform parent)
        {
            var button = Instantiate(original, parent);
            buttonTransform = button.GetComponent<RectTransform>();

            var buttonImageTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
            var buttonImage = buttonImageTransform?.GetComponent<Image>();

            if (buttonImageTransform == null || buttonImage == null) 
                return null;

            // Set positioning of new button (slightly offset left from the player count)
            buttonTransform.sizeDelta = new Vector2(30f, 30f);
            buttonTransform.offsetMin = new Vector2(-37f, -37f);
            buttonTransform.offsetMax = new Vector2(-9.3f, -7f);
            buttonTransform.anchoredPosition = new Vector2(-85f, -7f);
            buttonTransform.localPosition = new Vector3(-5.5f, 1.25f, 0f);
            buttonTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            SetupButtonImage(buttonImageTransform, buttonImage);

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = ""; // Set the string to blank so we can still use the graphic's "hitbox" for hover/click raycast calculation 
            }

            // Clear "join" event added in unity editor
            button.onClick.m_PersistentCalls.Clear();

            // Disable default button hover/click transition
            button.transition = Transition.None;
            button.animator.enabled = false;

            // Inject custom event handling
            buttonEventHandler = button.gameObject.AddComponent<ButtonEventHandler>();
            buttonEventHandler.SetButtonImageData(buttonImage, sprite, sprite, color, buttonImage.color);
            buttonEventHandler.OnHoverStateChanged += OnModListHoverStateChanged;
            return button;
        }

        private void OnModListHoverStateChanged(bool hovered)
        {
            if (buttonTransform == null || lobbySlot == null || parentContainer == null || HoverNotification.Instance == null)
                return;

            if (hovered)
            {
                HoverNotification.Instance.DisplayNotification(lobbySlot.thisLobby, lobbyType, buttonTransform, parentContainer);
            }
            else
            {
                HoverNotification.Instance.HideNotification();
            }
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

        // TODO: This only needs to be loaded once. Good enough for debugging as-is
        private Sprite? GetLobbySprite(ModdedLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case ModdedLobbyType.Compatible:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.ModSettings.png");
                case ModdedLobbyType.Incompatible:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.ModSettingsExclamationPoint.png");
                case ModdedLobbyType.Unknown:
                default:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.ModSettingsQuestionMark.png");
            }
        }

        // TODO: Replace with real implementation
        // Just incrementing through lobby types right now for debugging use
        private static int debuggingCount = 0;
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
