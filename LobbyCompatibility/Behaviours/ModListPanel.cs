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

namespace LobbyCompatibility.Behaviours
{
    public class ModListPanel : MonoBehaviour
    {
        private static readonly Vector2 notificationWidth = new Vector2(1.25f, 1.25f);

        public static ModListPanel? Instance;

        private RectTransform? panelTransform;
        private TextMeshProUGUI? text;

        private void Awake()
        {
            Instance = this;

            // Find actual alert panel so we can modify it
            panelTransform = transform.Find("Panel")?.GetComponent<RectTransform>();
            if (panelTransform == null)
                return;

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(panelTransform, notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("Image"), notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("NotificationText"), notificationWidth);

            SetupText(panelTransform);
            SetPanelActive(false);
        }

        // This could/should eventually be moved to a UIHelper method if we want this to look identical to the full modlist panel
        private void SetupText(RectTransform panelTransform)
        {
            text = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                // Move text up & make smaller
                text.fontSizeMax = 15f;
                text.fontSizeMin = 12f;
                text.rectTransform.anchoredPosition = new Vector2(-2f, 75f);
            }
        }

        public void DisplayNotification(Lobby lobby, ModdedLobbyType lobbyType)
        {
            if (panelTransform == null)
                return;

            SetPanelActive(true);
            // EventSystem.current.SetSelectedGameObject(this.menuNotification.GetComponentInChildren<Button>().gameObject);
            // TODO: set text based on lobby data here
            if (text != null)
            {
                text.text = $"Mod Status: {GetModdedLobbyText(lobbyType)}";
            }
        }

        // Probably a bad idea to have this MonoBehaviour be a child object of its own panel...
        private void SetPanelActive(bool active)
        {
            if (panelTransform == null)
                return;

            panelTransform.parent.gameObject.SetActive(active);
        }

        // Enum.GetName is slow
        // TODO: Replace with proper utility
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
    }
}
