using System.Linq;
using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="MenuManager.Start" />.
///     Adds extra UI components, such as lobby filtering.
/// </summary>
/// <seealso cref="MenuManager.Start" />
[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Start))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class MenuManagerPostfix
{
    public static RectTransform Panel;

    [HarmonyPostfix]
    private static void Postfix(MenuManager __instance)
    {
        // Don't run on startup screen
        if (__instance.isInitScene)
            return;

        LobbyCompatibilityPlugin.Logger?.LogInfo("menu managed.");
        LobbyCompatibilityPlugin.Logger?.LogInfo(__instance.menuNotification);

        var newNotification = UnityEngine.Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);
        newNotification.SetActive(true);

        // Disable invisible background image (used for disabling input raycasts on below layers)
        var invisibleBackgroundImage = newNotification.GetComponent<Image>();
        if (invisibleBackgroundImage != null)
            invisibleBackgroundImage.enabled = false;

        // Disable background image
        var backgroundImage = newNotification.transform.Find("Image")?.GetComponent<Image>();
        if (backgroundImage != null)
            backgroundImage.enabled = false;

        var panel = newNotification.transform.Find("Panel")?.GetComponent<Image>();
        if (panel == null)
            return;

        // Increase panel opacity to 100% since we disabled the background image
        panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 1);
        var panelTransform = panel.GetComponent<RectTransform>();
        panelTransform.anchorMin = new Vector2(0, 1);
        panelTransform.anchorMax = new Vector2(0, 1);

        MultiplySizeDelta(panelTransform, 0.6f, 1f);
        MultiplySizeDelta(panelTransform.Find("Image"), 0.6f, 1f);
        MultiplySizeDelta(panelTransform.Find("NotificationText"), 0.6f, 1f);
        panelTransform.Find("ResponseButton").gameObject.SetActive(false);

        panelTransform.anchoredPosition = new Vector2(panelTransform.sizeDelta.x / 2, -panelTransform.sizeDelta.y / 2);
        Panel = panelTransform;
        Panel.gameObject.SetActive(false);

        var hoverNotification = newNotification.AddComponent<HoverNotification>();
        var text = panelTransform.Find("NotificationText").GetComponent<TextMeshProUGUI>();
        text.fontSizeMax = 15f;
        text.fontSizeMin = 12f;
        text.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2f, 75f);
        hoverNotification.Setup(panelTransform, text);
    }

    private static void MultiplySizeDelta(Transform transform, float xMultiplier, float yMultiplier)
    {
        var rectTransform = transform.GetComponent<RectTransform>();
        if (rectTransform == null) 
            return;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * xMultiplier, rectTransform.sizeDelta.y * yMultiplier);
    }
}