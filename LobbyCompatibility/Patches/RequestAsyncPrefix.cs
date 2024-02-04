using HarmonyLib;
using LobbyCompatibility.Features;
using Steamworks.Data;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="LobbyQuery.RequestAsync" />.
///     Used to store previous lobby filter settings, so we can make additional requests with similar settings
///     This is done as late in the execution chain as possible, to maintain compatibility with lobby filters from other mods
/// </summary>
/// <seealso cref="MenuManager.Start" />
[HarmonyPatch(typeof(LobbyQuery), nameof(LobbyQuery.RequestAsync))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class RequestAsyncPrefix
{
    [HarmonyPrefix]
    private static void Prefix(LobbyQuery __instance)
    {
        var lobbyQuery = __instance;
        LobbyHelper.LatestLobbyRequestStringFilters = lobbyQuery.stringFilters ?? new();
        LobbyHelper.LatestLobbyRequestDistanceFilter = lobbyQuery.distance;
    }
}