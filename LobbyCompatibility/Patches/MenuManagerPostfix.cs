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
        var newHoverNotification = Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);
        newHoverNotification.AddComponent<ModListTooltipPanel>();
        newHoverNotification.SetActive(true);

        // Setup modlist panel UI
        var newNotification = Object.Instantiate(__instance.menuNotification, __instance.menuNotification.transform.parent);
        newNotification.AddComponent<ModListPanel>();
        newNotification.SetActive(true);
    }
}