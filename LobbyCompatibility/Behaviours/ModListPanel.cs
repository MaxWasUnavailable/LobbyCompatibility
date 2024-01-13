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
using UnityEngine.UI;

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
        }

        public void SetupPanel(GameObject panel)
        {
            var panelImage = panel.transform.Find("Panel")?.GetComponent<Image>();
            panelTransform = panelImage?.rectTransform;

            if (panelImage == null || panelTransform == null)
                return;

            // Get "dismiss" button so we can inject some custom behaviour
            var button = panelTransform.Find("ResponseButton")?.GetComponent<Button>();
            if (button == null)
                return;

            // Increase panel opacity to 100% so we can't see error messages underneath (if they exist)
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(panelTransform, notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("Image"), notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("NotificationText"), notificationWidth);

            // Inject custom button behaviour so it doesn't force you back to the main menu
            button.onClick.m_PersistentCalls.Clear();
            button.onClick.AddListener(() => { SetPanelActive(false); });

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

        private void SetPanelActive(bool active)
        {
            if (panelTransform == null)
                return;

            // Disable the parent because it also contains a background image used for blocking raycasts
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
