using System;
using LobbyCompatibility.Configuration;
using LobbyCompatibility.Enums;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace LobbyCompatibility.Models;

/// <summary>
///     Diff between two plugins.
/// </summary>
/// <param name="PluginDiffResult"> The compatibility result of the plugin. </param>
/// <param name="GUID"> The GUID of the plugin. </param>
/// <param name="ClientVersion"> The version of the plugin. </param>
/// <param name="ServerVersion"> The required version of the plugin (null if not required) </param>
public record PluginDiff(
    PluginDiffResult PluginDiffResult,
    string GUID,
    Version? ClientVersion,
    Version? ServerVersion)
{
    /// <summary>
    ///     The text to display for this plugin in the UI.
    /// </summary>
    public string GetDisplayText()
    {
        var name = $"{GUID}";

        if (ClientVersion != null)
            name += $"-v{ClientVersion}";

        if (PluginDiffResult == PluginDiffResult.ModVersionMismatch && ServerVersion != null)
            name += $" — v{ServerVersion} was required";

        return name;
    }

    /// <summary>
    ///     The color of the text to display for this plugin.
    /// </summary>
    public Color GetTextColor()
    {
        return PluginDiffResult switch
        {
            PluginDiffResult.Compatible => LobbyCompatibilityPlugin.Config?.CompatibleColor.Value ?? Config.DefaultCompatibleColor,
            PluginDiffResult.ClientMissingMod or PluginDiffResult.ServerMissingMod or PluginDiffResult.ModVersionMismatch 
                => LobbyCompatibilityPlugin.Config?.IncompatibleColor.Value ?? Config.DefaultIncompatibleColor,
            _ => LobbyCompatibilityPlugin.Config?.UnknownColor.Value ?? Config.DefaultUnknownColor
        };
    }
}