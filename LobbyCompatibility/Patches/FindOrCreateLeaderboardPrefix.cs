using HarmonyLib;
using Steamworks;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="ISteamUserStats.FindOrCreateLeaderboard" />.
///     Changes the target leaderboard to a modded one.
/// </summary>
/// <seealso cref="ISteamUserStats.FindOrCreateLeaderboard" />
[HarmonyPatch(typeof(ISteamUserStats), nameof(ISteamUserStats.FindOrCreateLeaderboard))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal class FindOrCreateLeaderboardPrefix
{
    [HarmonyPrefix]
    private static void Prefix(ref string pchLeaderboardName)
    {
        pchLeaderboardName = pchLeaderboardName.StartsWith("challenge") ? $"modded_{pchLeaderboardName}" : pchLeaderboardName;
    }
}