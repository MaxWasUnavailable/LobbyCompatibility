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
    [HarmonyPostfix]
    private static void Postfix(MenuManager __instance)
    {
        // Don't run on startup screen
        if (__instance.isInitScene)
            return;

        LobbyCompatibilityPlugin.Logger?.LogInfo("Initializing menu UI.");


        // Setup hover notification/tooltip UI
        var modListTooltipPanel = Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);
        modListTooltipPanel.AddComponent<ModListTooltipPanel>();
        modListTooltipPanel.SetActive(true);

        // Setup modlist panel
        var modListPanelNotification = Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);

        // Create actual panel handler (needs to be a seperate object because of the way notifications are structured)
        var modListPanelObject = new GameObject("ModListPanel Handler");
        modListPanelObject.transform.SetParent(__instance.menuNotification.transform.parent);
        var modListPanel = modListPanelObject.AddComponent<ModListPanel>();

        modListPanel.SetupPanel(modListPanelNotification);

        // newNotification.SetActive(true);
    }
}