using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Models;

/// <summary>
///     Container for diffs between lobby and client plugins.
/// </summary>
/// <param name="PluginDiffs"> The diffs between the lobby and client plugins. </param>
public record LobbyDiff(List<PluginDiff> PluginDiffs)
{
    public string GetDisplayText()
    {
        return $"Mod Status: {nameof(GetModdedLobbyType)}";
    }

    internal LobbyDiffResult GetModdedLobbyType()
    {
        if (PluginDiffs.Count == 0)
            return LobbyDiffResult.Unknown;

        return PluginDiffs.Any(pluginDiff => pluginDiff.PluginDiffResult != PluginDiffResult.Compatible)
            ? LobbyDiffResult.Incompatible
            : LobbyDiffResult.Compatible;
    }
}