using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            CreateModListButton(joinButton);
        }

        // Create button that displays mod list when clicked 
        // TODO: Behaviour on Hover?
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
    }
}
