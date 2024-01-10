using System;
using LobbyCompatibility.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming

namespace LobbyCompatibility.Models;

/// <summary>
///     A record containing information about a plugin.
/// </summary>
/// <param name="GUID"> The GUID of the plugin. </param>
/// <param name="Version"> The version of the plugin. </param>
/// <param name="CompatibilityLevel"> The compatibility level of the plugin. </param>
/// <param name="VersionStrictness"> The version strictness of the plugin. </param>
[Serializable]
public record PluginInfoRecord(
    string GUID,
    [property: JsonConverter(typeof(VersionConverter))]
    Version Version,
    CompatibilityLevel CompatibilityLevel,
    VersionStrictness VersionStrictness
);