using System;
using System.Collections.Generic;
using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;

namespace LobbyCompatibility.Serialization;

public static class LobbyCompatibilitySerializer
{
    private static string _pluginSeparator = "&";
    private static string _fieldSeparator = ";";
    
    public static string[] Serialize(IEnumerable<PluginInfoRecord> pluginInfoRecordList)
    {
        return [ "" ];
    }
    
    public static string Serialize(PluginInfoRecord pluginInfoRecord)
    {
        List<string> serializedPluginInfo =
        [
            Serialize(pluginInfoRecord.GUID),
            Serialize(pluginInfoRecord.Version),
            Serialize(pluginInfoRecord.CompatibilityLevel),
            Serialize(pluginInfoRecord.VersionStrictness)
        ];

        return serializedPluginInfo.Join(delimiter:_fieldSeparator);
    }

    private static string Serialize(string pluginGuid) => pluginGuid;

    private static string Serialize(Version version) => version.ToString();

    private static string Serialize(CompatibilityLevel? compatibilityLevel)
    {
        return compatibilityLevel switch
        {
            CompatibilityLevel.ClientOnly => "c",
            CompatibilityLevel.ServerOnly => "s",
            CompatibilityLevel.Everyone => "e",
            CompatibilityLevel.ClientOptional => "o",
            _ => ""
        };
    }

    private static string Serialize(VersionStrictness? versionStrictness)
    {
        return versionStrictness switch
        {
            VersionStrictness.None => "o",
            VersionStrictness.Major => "m",
            VersionStrictness.Minor => "n",
            VersionStrictness.Patch => "p",
            _ => ""
        };
    }

    public static IEnumerable<PluginInfoRecord> Deserialize(IEnumerable<string> paginatedSerializedPluginList)
    {
        string serializedPluginList = paginatedSerializedPluginList.Join(delimiter: _pluginSeparator);
        
        return new List<PluginInfoRecord>();
    }
}