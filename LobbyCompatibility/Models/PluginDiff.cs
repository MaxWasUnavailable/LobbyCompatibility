using System;
using LobbyCompatibility.Enums;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace LobbyCompatibility.Models;

/// <summary>
///     Diff between two plugins.
/// </summary>
/// <param name="PluginDiffResult"> The compatibility result of the plugin. </param>
/// <param name="GUID"> The GUID of the plugin. </param>
/// <param name="Version"> The version of the plugin. </param>
/// <param name="RequiredVersion"> The required version of the plugin (null if not required) </param>
public record PluginDiff(
    PluginDiffResult PluginDiffResult,
    string GUID,
    Version? Version,
    Version? RequiredVersion)
{
    /// <summary>
    ///     The text to display for this plugin in the UI.
    /// </summary>
    public string GetDisplayText()
    {
        var name = $"{GUID}";

        if (Version != null)
            name += $"-v{Version}";

        if (PluginDiffResult == PluginDiffResult.ModVersionMismatch && RequiredVersion != null)
            name += $" — v{RequiredVersion} was required";

        return name;
    }

    /// <summary>
    ///     The color of the text to display for this plugin.
    /// </summary>
    public Color GetTextColor()
    {
        return PluginDiffResult switch
        {
            PluginDiffResult.Compatible => Color.green,
            PluginDiffResult.ClientMissingMod or PluginDiffResult.ServerMissingMod => Color.red,
            PluginDiffResult.ModVersionMismatch => Color.yellow,
            _ => Color.gray
        };
    }
}