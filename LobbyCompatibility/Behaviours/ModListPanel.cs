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
        private static readonly Vector2 notificationWidth = new Vector2(1.25f, 1.75f);

        public static ModListPanel? Instance;

        private RectTransform? panelTransform;
        private TextMeshProUGUI? text;

        private void Awake()
        {
            Instance = this;
        }

        public void SetupPanel(GameObject panel, Transform scrollViewTemplate)
        {
            var panelImage = panel.transform.Find("Panel")?.GetComponent<Image>();
            panelTransform = panelImage?.rectTransform;

            if (panelImage == null || panelTransform == null)
                return;

            // Get "dismiss" button so we can inject some custom behaviour
            var button = panelTransform.Find("ResponseButton")?.GetComponent<Button>();
            var buttonTransform = button?.GetComponent<RectTransform>();
            if (button == null || buttonTransform == null)
                return;

            SetupScrollView(panelTransform, scrollViewTemplate);


            // Increase panel opacity to 100% so we can't see error messages underneath (if they exist)
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);

            // Multiply panel element sizes to make the hover notification skinnier
            UIHelper.TryMultiplySizeDelta(panelTransform, notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("Image"), notificationWidth);
            UIHelper.TryMultiplySizeDelta(panelTransform.Find("NotificationText"), notificationWidth);

            // Set button to be consistently spaced from the bottom of the panel 
            // This is the exact pixel distance the "Back" button is from the bottom on normal panels. TODO: do this dynamically based on notificationWidth
            buttonTransform.anchoredPosition = new Vector2(buttonTransform.anchoredPosition.x, -110.5f);
            // Inject custom button behaviour so it doesn't force you back to the main menu
            button.onClick.m_PersistentCalls.Clear();
            button.onClick.AddListener(() => { SetPanelActive(false); });

            SetupText(panelTransform);
            SetPanelActive(false);

            // setup scroll rect
        }

        private void SetupScrollView(RectTransform panelTransform, Transform scrollViewTemplate)
        {
            // Setup ScrollView for panel
            var scrollViewObject = Instantiate(scrollViewTemplate, panelTransform);
            var scrollViewTransform = scrollViewObject.GetComponent<RectTransform>();
            var scrollView = scrollViewObject.GetComponent<ScrollRect>();

            if (scrollViewTransform == null || scrollView == null)
                return;

            // Delete lobby manager (not sure why it's on this object?)
            var lobbyManager = scrollViewObject.GetComponentInChildren<SteamLobbyManager>();
            if (lobbyManager != null)
                Destroy(lobbyManager);

            // Set pos/scale
            scrollViewTransform.anchoredPosition = new Vector2(15f, -30f);
            scrollViewTransform.sizeDelta = new Vector2(-30f, -100f);

            // Set scroll to zero
            scrollView.verticalNormalizedPosition = 1f;
        }

        // This could/should eventually be moved to a UIHelper method if we want this to look identical to the full modlist panel
        private void SetupText(RectTransform panelTransform)
        {
            text = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                // Move header up
                text.rectTransform.anchoredPosition = new Vector2(-2f, 155f);
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
