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
    [property:JsonProperty("id")]
    string GUID,
    
    [property:JsonProperty("v")]
    [property: JsonConverter(typeof(VersionConverter))]
    Version Version,
    
    [property:JsonProperty("cl")]
    CompatibilityLevel? CompatibilityLevel,
    
    [property:JsonProperty("vs")]
    VersionStrictness? VersionStrictness
);