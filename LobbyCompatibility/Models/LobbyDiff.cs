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

    /// <summary>
    ///     The text to display for this lobby in the UI.
    /// </summary>
    /// <returns> The text to display for this lobby in the UI. </returns>
    public string GetDisplayText()
    {
        return $"Mod Status: {GetModdedLobbyType().ToString()}";
    }

    /// <summary>
    ///     The color of the text to display for this lobby.
    /// </summary>
    /// <returns> The color of the text to display for this lobby. </returns>
    internal LobbyDiffResult GetModdedLobbyType()
    {
        if (_cachedResult != null)
            return (LobbyDiffResult)_cachedResult;

        if (PluginDiffs.Count == 0)
        {
            _cachedResult = LobbyDiffResult.Unknown;
            return LobbyDiffResult.Unknown;
        }

        if (PluginDiffs.Any(pluginDiff => pluginDiff.PluginDiffResult != PluginDiffResult.Compatible &&
                                          pluginDiff.PluginDiffResult != PluginDiffResult.Unknown))
        {
            _cachedResult = LobbyDiffResult.Incompatible;
            return LobbyDiffResult.Incompatible;
        }

        _cachedResult = LobbyDiffResult.Compatible;
        return LobbyDiffResult.Compatible;
    }
}