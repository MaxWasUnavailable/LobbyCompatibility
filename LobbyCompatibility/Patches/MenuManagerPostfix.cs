using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Features;
using UnityEngine;
using UnityEngine.UI;

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
    [HarmonyPrefix]
    private static void Prefix(MenuManager __instance)
    {
        // Don't run on startup screen
        if (__instance.isInitScene)
            return;

        var listPanel = __instance.serverListUIContainer.transform.Find("ListPanel");
        var lobbyListScrollView = listPanel != null ? listPanel.Find("Scroll View") : null;
        var privatePublicDescription = __instance.privatePublicDescription;
        if (listPanel == null || lobbyListScrollView == null || privatePublicDescription == null)
            return;

        LobbyCompatibilityPlugin.Logger?.LogInfo("Initializing menu UI.");
        
        // Setup hover notification/tooltip UI
        var parent = __instance.menuNotification.transform.parent;
        
        var modListTooltipPanel =
            Object.Instantiate(__instance.menuNotification, parent);
        modListTooltipPanel.AddComponent<ModListTooltipPanel>();
        modListTooltipPanel.SetActive(true);

        // Setup modlist panel
        var modListPanelNotification =
            Object.Instantiate(__instance.menuNotification, parent);

        // Get description text for bottom of modlist panel

        // Create actual panel handler (needs to be a seperate object because of the way notifications are structured)
        var modListPanelObject = new GameObject("ModListPanel Handler");
        modListPanelObject.transform.SetParent(parent);
        var modListPanel = modListPanelObject.AddComponent<ModListPanel>();
        modListPanel.SetupPanel(modListPanelNotification, lobbyListScrollView, privatePublicDescription);
    }
    
    [HarmonyPostfix]
    private static void Postfix(MenuManager __instance)
    {
        // Don't run on startup screen
        if (__instance.isInitScene)
            return;

        var listPanel = __instance.serverListUIContainer.transform.Find("ListPanel");
        if (listPanel == null)
            return;

        // Set challenge moon leaderboard title text to not wrap halfway through
        __instance.leaderboardHeaderText.rectTransform.offsetMax = new Vector2(2000, __instance.leaderboardHeaderText.rectTransform.offsetMax.y);

        // Make refresh button a more compact image so we have space for our custom dropdown
        var refreshButton = listPanel.Find("RefreshButton")?.GetComponent<Button>();
        if (refreshButton != null)
            UIHelper.ReskinRefreshButton(refreshButton);

        // Add a custom "Mods" filtering type, and shift all other filtering UI elements to the left
        if (!GameNetworkManager.Instance || !GameNetworkManager.Instance.disableSteam)
            UIHelper.AddCustomFilterToLobbyList(listPanel);
    }
}