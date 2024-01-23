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
    private LobbyDiffResult? _cachedResult;

    public string GetDisplayText()
    {
        return $"Mod Status: {nameof(GetModdedLobbyType)}";
    }

    internal LobbyDiffResult GetModdedLobbyType()
    {
        if (_cachedResult != null)
            return (LobbyDiffResult)_cachedResult;

        if (PluginDiffs.Count == 0)
        {
            _cachedResult = LobbyDiffResult.Unknown;
            return LobbyDiffResult.Unknown;
        }

        if (PluginDiffs.Any(pluginDiff => pluginDiff.PluginDiffResult != PluginDiffResult.Compatible))
        {
            _cachedResult = LobbyDiffResult.Incompatible;
            return LobbyDiffResult.Incompatible;
        }

        _cachedResult = LobbyDiffResult.Compatible;
        return LobbyDiffResult.Compatible;
    }
}