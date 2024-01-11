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
            LobbyCompatibilityPlugin.Logger?.LogInfo(lobbySlot.thisLobby.Id);
            var joinButton = GetComponentInChildren<Button>();
            // joinButton.interactable = false;
            var numPlayers = lobbySlot.playerCount;

            CreateModListButton(joinButton);

            var sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.Warning.png");
            if (sprite != null && lobbySlot.LobbyName != null)
            {
                CreateImage(lobbySlot.transform, sprite, lobbySlot.LobbyName.color);
            }

            var moddedLobbyType = GetModdedLobbyType(lobbySlot.thisLobby);
            numPlayers.text = numPlayers.text + $" - {GetModdedLobbyText(moddedLobbyType)}";
        }

        private static int count = 0;

        private void CreateImage(Transform parent, Sprite sprite, Color color)
        {
            var imageObject = new GameObject("WarningSymbol");
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.AddComponent<Image>();

            count++;
            var testCount = count % 3;
            if (testCount == 0)
            {
                image.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.Warning.png");
            }
            else if (testCount == 1)
            {
                image.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.QuestionMarkWarning.png");
            }
            else if (testCount == 2)
            {
                image.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.CheckMarkWarning.png");
            }
            image.color = color;

            var imageTransform = imageObject.GetComponent<RectTransform>();

            // a lot of manual transform-ing...
            imageTransform.anchorMin = Vector2.one;
            imageTransform.anchorMax = Vector2.one;
            imageTransform.pivot = Vector2.one;
            imageTransform.sizeDelta = new Vector2(30f, 30f);
            imageTransform.offsetMin = new Vector2(-37f, -37f);
            imageTransform.offsetMax = Vector2.zero;
            imageTransform.anchoredPosition = Vector2.zero;
            imageTransform.localPosition = new Vector3(450f, -4, 0);
            imageTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        }

        // Create button that displays mod list when clicked 
        // TODO: Behaviour on Hover?
        // TODO: Hook up to lobby info panel
        private Button CreateModListButton(Button original)
        {
            var button = Instantiate(original, transform);
            var buttonTransform = button.GetComponent<RectTransform>();

            // set positioning of new button (slightly offset left from the join button)
            buttonTransform.sizeDelta = new Vector2(30f, 30f);
            buttonTransform.offsetMin = new Vector2(-37f, -37f);
            buttonTransform.offsetMax = new Vector2(-9.3f, -7f);
            buttonTransform.anchoredPosition = new Vector2(-85f, -7f);

            var buttonHighlightTransform = button.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
            if (buttonHighlightTransform != null)
            {
                // Align background highlight with new positioning
                buttonHighlightTransform.sizeDelta = new Vector2(25.191f, 25.191f);
                buttonHighlightTransform.offsetMin = new Vector2(-0.423f, 3.0325f);
                buttonHighlightTransform.offsetMax = new Vector2(28.2235f, 28.2235f);
                buttonHighlightTransform.anchoredPosition = new Vector2(14f, 15f);
            }

            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "?";
            }

            // Clear "join" event added in unity editor
            button.onClick.m_PersistentCalls.Clear();
            return button;
        }

        // Enum.GetName is slow
        private string GetModdedLobbyText(ModdedLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case ModdedLobbyType.Compatible:
                    return "Compatible";
                case ModdedLobbyType.Incompatible:
                    return "Incompatible";
                case ModdedLobbyType.Unknown:
                default:
                    return "Unknown";
            }
        }

        // TODO: Replace with real implementation
        private ModdedLobbyType GetModdedLobbyType(Lobby lobby)
        {
            // some lobby.getdata shenanigans here
            return ModdedLobbyType.Unknown;
        }

        // Unnecessary if we just hijack the player count text in Start()
        /*
        private TextMeshProUGUI CreateCompatibilityText(TextMeshProUGUI original)
        {
            var text = Instantiate(original, transform);
            text.transform.localPosition = new Vector3(60, text.transform.localPosition.y, text.transform.localPosition.z);

            text.text = "- Compatible";
            return text;
        }
        */
    }
}
