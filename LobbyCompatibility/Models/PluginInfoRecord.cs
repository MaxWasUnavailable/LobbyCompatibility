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
)
{
    private int? _jsonLength;
    
    /// <summary>
    ///     The calculated length of the json string.
    /// </summary>
    public int JsonLength => _jsonLength ??= 25 + GUID.Length + Version.ToString().Length + 
                               (CompatibilityLevel.HasValue ? 1 : 4) + (VersionStrictness.HasValue ? 1 : 4);

    public int CompareTo(PluginInfoRecord other)
    {
        if (CompatibilityLevel != other.CompatibilityLevel)
        {
            if (CompatibilityLevel is null) return -1;
            if (other.CompatibilityLevel is null) return 1;
            
            return (CompatibilityLevel ?? 0) - (other.CompatibilityLevel ?? 0);
        }
        
        if (VersionStrictness != other.VersionStrictness)
        {
            if (VersionStrictness is null) return -1;
            if (other.VersionStrictness is null) return 1;
            
            return (VersionStrictness ?? 0) - (other.VersionStrictness ?? 0);
        }

        return string.Compare(GUID, other.GUID, StringComparison.Ordinal);
    }
}