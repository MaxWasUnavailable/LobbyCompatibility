using System;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
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
/// <param name="VariableCompatibilityCheck"> A function called to decide the compatibility level, based off the lobby data. Must return a <see cref="LobbyCompatibility.Enums.CompatibilityLevel"/>. </param>
[Serializable]
public record PluginInfoRecord(
    [property:JsonProperty("i")]
    string GUID,
    
    [property:JsonProperty("v")]
    [property: JsonConverter(typeof(VersionConverter))]
    Version Version,
    
    [property:JsonProperty("c")]
    CompatibilityLevel? CompatibilityLevel,
    
    [property:JsonProperty("s")]
    VersionStrictness? VersionStrictness,
    
    [property:JsonIgnore]
    VariableCompatibilityCheckDelegate? VariableCompatibilityCheck = null
);