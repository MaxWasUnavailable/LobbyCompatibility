using System.Linq;
using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using Steamworks;
using Steamworks.Data;

namespace LobbyCompatibility.Patches;

/// <summary>
///     Patches <see cref="GameNetworkManager.SteamMatchmaking_OnLobbyCreated" />.
///     Adds extra lobby metadata to be used for dependency checking.
/// </summary>
/// <seealso cref="GameNetworkManager.SteamMatchmaking_OnLobbyCreated" />
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SteamMatchmaking_OnLobbyCreated))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class SteamMatchmakingOnLobbyCreatedPostfix
{
    [HarmonyPostfix]
    private static void Postfix(Result result, ref Lobby lobby)
    {
        // lobby has not yet been created or something went wrong
        if (result != Result.OK)
            return;
        
        var pluginInfo = PluginHelper.GetAllPluginInfo().CalculateCompatibilityLevel(lobby);

        // Modded is flagged as true, since we're using mods
        lobby.SetData(LobbyMetadata.Modded, "true");

        // Add paginated plugin metadata to the lobby so clients can check if they have the required plugins
        var pluginsString = PluginHelper.GetLobbyPluginsMetadata(pluginInfo).ToArray();
        // Add each page - with a delimiter if there's another page
        for (var i = 0; i < pluginsString.Length; i++)
            lobby.SetData($"{LobbyMetadata.Plugins}{i}", $"{pluginsString[i]}{(i < pluginsString.Length - 1 ? "@" : string.Empty)}");

        // Set the joinable modded metadata to the same value as the original joinable metadata, in case it wasn't originally joinable
        lobby.SetData(LobbyMetadata.JoinableModded, lobby.GetData(LobbyMetadata.Joinable));

        // Add checksum metadata to the lobby so steamworks can filter out incompatible lobbies.
        lobby.SetData(LobbyMetadata.RequiredChecksum, PluginHelper.Checksum);

        // Add a prefix to the lobby name to indicate that it's modded, if it doesn't already have some kind of modded mention
        if (pluginInfo.Any(plugin => plugin.CompatibilityLevel is CompatibilityLevel.ServerOnly
                or CompatibilityLevel.Everyone or CompatibilityLevel.ClientOptional) &&
            !LobbyHelper.LobbyNameContainsModIdentifier(lobby) && LobbyCompatibilityPlugin.Config!.ChangeLobbyName.Value)
            lobby.SetData(LobbyMetadata.Name, LobbyMetadata.ModdedLobbyPrefix + lobby.GetData(LobbyMetadata.Name));

        // Check if there are any required plugins in the lobby
        if (pluginInfo.Any(plugin => plugin.CompatibilityLevel == CompatibilityLevel.Everyone))
        {
            LobbyCompatibilityPlugin.Logger?.LogWarning(
                "You are hosting a lobby with required plugins. Disabling vanilla clients from joining.");
            lobby.SetData(LobbyMetadata.Joinable, "false");
        
            // Set the tag to "modded" to hide from vanilla unless otherwise specified by the user (also hidden from vanilla)
            if (lobby.GetData(LobbyMetadata.Tag) == "none")
                lobby.SetData(LobbyMetadata.Tag, "modded");
        }
    }
}