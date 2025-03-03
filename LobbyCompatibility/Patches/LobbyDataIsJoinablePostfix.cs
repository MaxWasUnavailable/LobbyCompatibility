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
        LobbyHelper.LatestLobbyData = lobby.Data;
        
        // If original result was false, return false -- if lobby is modded through our mod, it will be Joinable || JoinableModded
        if (!isJoinable)
            return false;

        // If the lobby is not modded, return original result if client doesn't have required plugins
        if (lobby.GetData(LobbyMetadata.Modded) != "true" && !PluginHelper.CanJoinVanillaLobbies())
        {
            Object.FindObjectOfType<MenuManager>().SetLoadingScreen(
                false,
                RoomEnter.NotAllowed,
                "You are using mods which aren't strictly client-side, but the lobby is not modded.");
            return PluginHelper.CanJoinVanillaLobbies() && isJoinable;
        }

        var lobbyPluginString = LobbyHelper.GetLobbyPlugins(lobby).Join(delimiter: string.Empty);

        // Create lobby diff so LatestLobbyDiff is set
        LobbyHelper.GetLobbyDiff(lobby, lobbyPluginString);

        // If the lobby does not have any plugin information, return original result (since we can't check anything)
        if (string.IsNullOrEmpty(lobbyPluginString))
        {
            LobbyCompatibilityPlugin.Logger?.LogWarning("Lobby is modded but does not have any plugin information.");
            return isJoinable;
        }

        var matchesPluginRequirements =
            PluginHelper.MatchesTargetRequirements(PluginHelper.ParseLobbyPluginsMetadata(lobbyPluginString));

        if (!matchesPluginRequirements)
        {
            LobbyCompatibilityPlugin.Logger?.LogWarning("You are missing required plugins to join this lobby.");
            Object.FindObjectOfType<MenuManager>().SetLoadingScreen( // TODO: Improve error message (diff?)
                false,
                RoomEnter.Error,
                "You are missing required mods to join this lobby.");
        }

        return matchesPluginRequirements;
    }
}