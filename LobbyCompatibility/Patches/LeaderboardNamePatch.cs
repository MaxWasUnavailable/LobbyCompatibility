using System.Reflection;
using HarmonyLib;
using Steamworks;

namespace LobbyCompatibility.Patches;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal class LeaderboardNamePatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return typeof(SteamUserStats).Assembly.GetType("Steamworks.ISteamUserStats")!.GetMethod(
            "FindOrCreateLeaderboard", (BindingFlags)60)!;
    }

    [HarmonyPrefix]
    private static void SavePrefix(ref string pchLeaderboardName)
    {
        pchLeaderboardName = pchLeaderboardName.StartsWith("challenge")
            ? $"modded_{pchLeaderboardName}"
            : pchLeaderboardName;
    }
}