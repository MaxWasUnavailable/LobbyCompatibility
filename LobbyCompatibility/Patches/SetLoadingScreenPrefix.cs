using HarmonyLib;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Features;
using Steamworks;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <seealso cref="MenuManager.SetLoadingScreen" />.
///     Handles when the client errors out when joining a lobby. (Most likely due to mod incompatibility)
/// </summary>
/// <seealso cref="MenuManager.SetLoadingScreen" />
[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.SetLoadingScreen))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class SetLoadingScreenPrefix
{
    [HarmonyPrefix]
    private static bool Prefix(bool isLoading, RoomEnter result, MenuManager __instance)
    {
        if (isLoading)
            return true;

        if (result != RoomEnter.Error)
            return true;
        
        __instance.MenuAudio.volume = 0.5f;
        __instance.menuButtons.SetActive(true);
        __instance.loadingScreen.SetActive(false);
        __instance.serverListUIContainer.SetActive(false);

        ModListPanel.Instance.DisplayNotification(LobbyHelper.LatestLobbyDiff, "Error while joining...");
        
        return false;
    }
}