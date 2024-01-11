using HarmonyLib;
using Steamworks;
using Steamworks.Data;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal class LeaderboardNamePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ISteamUserStats), nameof(ISteamUserStats.FindOrCreateLeaderboard))]
    private static void SavePrefix(ref string pchLeaderboardName)
    {
        pchLeaderboardName = (pchLeaderboardName.StartsWith("challenge")) ? $"modded_{pchLeaderboardName}" : pchLeaderboardName;
    }
}