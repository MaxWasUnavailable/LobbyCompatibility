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
    public class ModdedLobbySlot : MonoBehaviour
    {
        private LobbySlot? lobbySlot;

        // Runs after LobbySlot data set
        public void Start()
        {
            LobbyCompatibilityPlugin.Logger?.LogInfo("Modded lobby awake!");
            // Not 100% ideal, but I don't want to mess with IL/stack weirdness too much right now
            lobbySlot = GetComponent<LobbySlot>();
            if (lobbySlot == null) return;

            var numPlayers = lobbySlot.playerCount;
            numPlayers.transform.localPosition = new Vector3(32f, numPlayers.transform.localPosition.y, numPlayers.transform.localPosition.z); // adjust playercount to the right to make button space
            
            var moddedLobbyType = GetModdedLobbyType(lobbySlot.thisLobby);

            var sprite = GetLobbySprite(moddedLobbyType);
            var joinButton = GetComponentInChildren<Button>(); // Find "Join Lobby" button template

            if (joinButton != null && sprite != null && lobbySlot.LobbyName != null)
            {
                CreateModListButton(joinButton, sprite, lobbySlot.LobbyName.color, numPlayers.transform);
            }
        }

        // Create button that displays mod list when clicked 
        // TODO: Hook up to lobby info panel
        private Button CreateModListButton(Button original, Sprite sprite, Color color, Transform parent)
        {
            var button = Instantiate(original, parent);
            var buttonTransform = button.GetComponent<RectTransform>();

            // Set positioning of new button (slightly offset left from the player count)
            buttonTransform.sizeDelta = new Vector2(30f, 30f);
            buttonTransform.offsetMin = new Vector2(-37f, -37f);
            buttonTransform.offsetMax = new Vector2(-9.3f, -7f);
            buttonTransform.anchoredPosition = new Vector2(-85f, -7f);
            buttonTransform.localPosition = new Vector3(-5.5f, 1.25f, 0f);
            buttonTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            SetupSelectionHighlight(button, sprite, color);

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = ""; // Set the string to blank so we can still use the graphic's "hitbox" for hover/click raycast calculation 
            }

            // Clear "join" event added in unity editor
            button.onClick.m_PersistentCalls.Clear();
            return button;
        }

        private void SetupSelectionHighlight(Button button, Sprite sprite, Color color)
        {
            var buttonHighlightTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
            if (buttonHighlightTransform == null) return;

            // Align background highlight with new positioning
            buttonHighlightTransform.sizeDelta = new Vector2(25f, 25f);
            buttonHighlightTransform.offsetMin = new Vector2(0f, 0f);
            buttonHighlightTransform.offsetMax = new Vector2(25f, 25f);
            buttonHighlightTransform.anchoredPosition = new Vector2(14f, 15f);
            buttonHighlightTransform.localScale = Vector3.one;
            buttonHighlightTransform.GetComponent<Image>().sprite = sprite; // Add custom image to display when hovered. TODO: Make a better version (not just recolored, use some fancy outlines/inversion)

            var buttonImageTransform = Instantiate(buttonHighlightTransform, buttonHighlightTransform.transform.parent, true);
            buttonImageTransform.transform.SetAsFirstSibling(); // layer UNDER the selection, so selection will override
            var buttonImage = buttonImageTransform.GetComponent<Image>();
            buttonImage.color = color;
            buttonImage.enabled = true;
            
        }

        // TODO: This only needs to be loaded once. Good enough for debugging as-is
        private Sprite? GetLobbySprite(ModdedLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case ModdedLobbyType.Compatible:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.CheckMarkWarning.png");
                case ModdedLobbyType.Incompatible:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.Warning.png");
                case ModdedLobbyType.Unknown:
                default:
                    return TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.QuestionMarkWarning.png");
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
