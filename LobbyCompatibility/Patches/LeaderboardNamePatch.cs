using System.Reflection;
using HarmonyLib;
using Steamworks;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal class LeaderboardNamePatch
{
    [HarmonyPatch(typeof(ISteamUserStats), nameof(ISteamUserStats.FindOrCreateLeaderboard))]
    [HarmonyPrefix]
    private static void SavePrefix(ref string pchLeaderboardName)
    {
        pchLeaderboardName = (pchLeaderboardName.StartsWith("challenge")) ? $"modded_{pchLeaderboardName}" : pchLeaderboardName;
    }
}