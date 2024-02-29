using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Models;

/// <summary>
///     Container for diffs between lobby and client plugins.
/// </summary>
/// <param name="PluginDiffs"> The diffs between the lobby and client plugins. </param>
/// <param name="LobbyCompatibilityPresent"> Whether or not the lobby has this mod installed. </param>
public record LobbyDiff(List<PluginDiff> PluginDiffs, bool LobbyCompatibilityPresent = true)
{
    private LobbyDiffResult? _cachedResult;

    /// <summary>
    ///     The text to display for this lobby in the UI.
    /// </summary>
    /// <param name="shortened"> Shorten longer lobby type strings. </param>
    /// <returns> The text to display for this lobby in the UI. </returns>
    public string GetDisplayText(bool shortened = false)
    {
        var lobbyType = GetModdedLobbyType();
        string lobbyTypeString;

        if (lobbyType == LobbyDiffResult.PresumedCompatible)
            lobbyTypeString = shortened ? "Compatible?" : "Presumed Compatible";
        else
            lobbyTypeString = lobbyType.ToString();

        return $"Mod Status: {lobbyTypeString}";
    }

    /// <summary>
    ///     The color of the text to display for this lobby.
    /// </summary>
    /// <returns> The color of the text to display for this lobby. </returns>
    internal LobbyDiffResult GetModdedLobbyType()
    {
        if (_cachedResult != null)
            return (LobbyDiffResult)_cachedResult;

        if (!LobbyCompatibilityPresent)
            return (LobbyDiffResult)(_cachedResult = LobbyDiffResult.Unknown);

        var unknownFound = PluginDiffs.Any(pluginDiff => pluginDiff.PluginDiffResult == PluginDiffResult.Unknown);

        if (PluginDiffs.Any(pluginDiff => pluginDiff.PluginDiffResult != PluginDiffResult.Compatible &&
                                          pluginDiff.PluginDiffResult != PluginDiffResult.Unknown))
            return (LobbyDiffResult)(_cachedResult = LobbyDiffResult.Incompatible);

        if (unknownFound)
            return (LobbyDiffResult)(_cachedResult = LobbyDiffResult.PresumedCompatible);

        return (LobbyDiffResult)(_cachedResult = LobbyDiffResult.Compatible);
    }
}