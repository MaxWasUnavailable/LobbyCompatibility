using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using Steamworks.Data;

namespace LobbyCompatibility.Features;

/// <summary>
///     Helper class for Lobby related functions.
/// </summary>
internal static class LobbyHelper
{
    private static List<PluginInfoRecord>? _clientPlugins;

    public static LobbyDiff LatestLobbyDiff { get; private set; } = new(new List<PluginDiff>());

    /// <summary>
    ///     Get a <see cref="LobbyDiff" /> from a <see cref="Lobby" />.
    /// </summary>
    /// <param name="lobby"> The lobby to get the diff from. </param>
    /// <returns> The <see cref="LobbyDiff" /> from the <see cref="Lobby" />. </returns>
    public static LobbyDiff GetLobbyDiff(Lobby lobby)
    {
        var lobbyPlugins = PluginHelper
            .ParseLobbyPluginsMetadata(lobby.GetData(LobbyMetadata.Plugins)).ToList();
        _clientPlugins ??= PluginHelper.GetAllPluginInfo().ToList();

        var pluginDiffs = new List<PluginDiff>();

        foreach (var lobbyPlugin in lobbyPlugins)
        {
            var clientPlugin = _clientPlugins.FirstOrDefault(plugin => plugin.GUID == lobbyPlugin.GUID);

            if (clientPlugin == null && lobbyPlugin.CompatibilityLevel == CompatibilityLevel.Everyone)
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ClientMissingMod, lobbyPlugin.GUID, null,
                    lobbyPlugin.Version));

            else if (!PluginHelper.MatchesVersion(clientPlugin, lobbyPlugin))
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ModVersionMismatch, lobbyPlugin.GUID,
                    clientPlugin.Version, lobbyPlugin.Version));

            else
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.Compatible, lobbyPlugin.GUID, clientPlugin.Version,
                    lobbyPlugin.Version));
        }

        foreach (var clientPlugin in _clientPlugins)
        {
            var lobbyPlugin = lobbyPlugins.FirstOrDefault(plugin => plugin.GUID == clientPlugin.GUID);

            if (lobbyPlugin == null &&
                clientPlugin.CompatibilityLevel is CompatibilityLevel.Everyone or CompatibilityLevel.ClientOptional)
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ServerMissingMod, clientPlugin.GUID,
                    clientPlugin.Version, null));
        }

        LatestLobbyDiff = new LobbyDiff(pluginDiffs);

        return LatestLobbyDiff;
    }

    public static string GetCompatibilityHeader(PluginDiffResult pluginDiffResult)
    {
        return pluginDiffResult switch
        {
            PluginDiffResult.Compatible => "Compatible",
            PluginDiffResult.ClientMissingMod => "Missing required mods",
            PluginDiffResult.ServerMissingMod => "Incompatible with server",
            PluginDiffResult.ModVersionMismatch => "Mod version mismatch",
            _ => "Unknown"
        };
    }
}