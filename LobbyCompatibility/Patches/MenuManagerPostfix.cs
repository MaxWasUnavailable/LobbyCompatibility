using HarmonyLib;
using LobbyCompatibility.Behaviours;
using UnityEngine;

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

        var lobbyListScrollView = __instance.serverListUIContainer.transform.Find("ListPanel/Scroll View");
        if (lobbyListScrollView == null)
            return;

        LobbyCompatibilityPlugin.Logger?.LogInfo("Initializing menu UI.");

        // Set challenge moon leaderboard title text to not wrap halfway through
        __instance.leaderboardHeaderText.rectTransform.offsetMax = new Vector2(2000, __instance.leaderboardHeaderText.rectTransform.offsetMax.y);

        // Setup hover notification/tooltip UI
        var modListTooltipPanel =
            Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);
        modListTooltipPanel.AddComponent<ModListTooltipPanel>();
        modListTooltipPanel.SetActive(true);

        // Setup modlist panel
        var modListPanelNotification =
            Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);

        // Create actual panel handler (needs to be a seperate object because of the way notifications are structured)
        var modListPanelObject = new GameObject("ModListPanel Handler");
        modListPanelObject.transform.SetParent(__instance.menuNotification.transform.parent);
        var modListPanel = modListPanelObject.AddComponent<ModListPanel>();
        modListPanel.SetupPanel(modListPanelNotification, lobbyListScrollView);
    }
}