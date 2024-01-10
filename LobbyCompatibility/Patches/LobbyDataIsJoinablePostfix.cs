using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="GameNetworkManager.LobbyDataIsJoinable" />.
///     Checks if required plugins are present in the lobby metadata and are the same version as the client.
/// </summary>
/// <seealso cref="GameNetworkManager.LobbyDataIsJoinable" />
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class LobbyDataIsJoinablePostfix
{
    [HarmonyPostfix]
    private static bool Postfix(bool isJoinable, ref Lobby lobby)
    {
        // If original result was false, return false
        if (!isJoinable)
            return false;

        // If the lobby is not modded, return original result
        if (lobby.GetData(LobbyMetadata.Modded) != "true")
            return isJoinable;

        var lobbyPluginString = lobby.GetData(LobbyMetadata.Plugins);

        // If the lobby does not have any plugin information, return original result (since we can't check anything)
        if (string.IsNullOrEmpty(lobbyPluginString))
        {
            LobbyCompatibilityPlugin.Logger?.LogWarning("Lobby is modded but does not have any plugin information.");
            return isJoinable;
        }

        // TODO: Should probably return a "result" instead of a bool, so we can build a diff / UI / whatever for the user to see what's missing
        var matchesPluginRequirements =
            PluginHelper.MatchesTargetRequirements(PluginHelper.ParseLobbyPluginsMetadata(lobbyPluginString));

        if (!matchesPluginRequirements)
        {
            LobbyCompatibilityPlugin.Logger?.LogWarning("You are missing required plugins to join this lobby.");
            Object.FindObjectOfType<MenuManager>().SetLoadingScreen(
                false,
                RoomEnter.Error,
                "You are missing required mods to join this lobby.");
        }

        return matchesPluginRequirements;
    }
}