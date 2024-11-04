using HarmonyLib;
using LobbyCompatibility.Features;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="MenuManager.StartAClient" />.
///     Checks if required plugins are present in the lobby metadata and are the same version as the client.
/// </summary>
/// <seealso cref="MenuManager.StartAClient" />
[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.StartAClient))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class StartAClientPostfix
{
    [HarmonyPrefix]
    private static void Prefix()
    {
        // Create lobby diff so LatestLobbyDiff is set
        LobbyHelper.GetLobbyDiff(null);
    }
}