using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Features;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

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

        // Make refresh button a more compact image so we have space for our custom dropdown
        var refreshButton = __instance.serverListUIContainer.transform.Find("ListPanel/RefreshButton")?.GetComponent<Button>();
        if (refreshButton != null)
            UIHelper.ReskinRefreshButton(refreshButton);

        var dropdown = __instance.serverListUIContainer.transform.Find("ListPanel/Dropdown");
        var customDropdownTransform = UnityEngine.Object.Instantiate(dropdown, dropdown.parent, false);
        var customDropdown = customDropdownTransform.GetComponent<TMP_Dropdown>();
        var customDropdownRect = customDropdownTransform.GetComponent<RectTransform>();

        customDropdownRect.anchoredPosition = new Vector2(0f, customDropdownRect.anchoredPosition.y);

        customDropdown.captionText.fontSize = 10f;
        customDropdown.itemText.fontSize = 10f;
        customDropdown.ClearOptions();
        customDropdown.AddOptions(new List<OptionData>()
        {
            new OptionData("Mods: Compatible first"),
            new OptionData("Mods: Compatible only"),
            new OptionData("Mods: All"),
        });
    }
}