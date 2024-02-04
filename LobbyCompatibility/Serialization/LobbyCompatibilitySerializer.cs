using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;

namespace LobbyCompatibility.Serialization;

public static class LobbyCompatibilitySerializer
{
    private const char PluginSeparator = '&';
    private const char FieldSeparator = ';';

    public static string[] Serialize(IEnumerable<PluginInfoRecord> pluginInfoRecordList)
    {
        List<string> allSerializedPlugins = [];
        List<string> separatedConcatStrings = [];

        allSerializedPlugins.AddRange(pluginInfoRecordList.Select(Serialize));

        var tempString = "";

        foreach (var plugin in allSerializedPlugins)
        {
            if (tempString.Length + plugin.Length + 1 < 8192)
                tempString += $"{(tempString.Length == 0 ? PluginSeparator : "")}{plugin}";
            else
            {
                separatedConcatStrings.Add(tempString);
                tempString = "";
            }
        }
        
        return separatedConcatStrings.ToArray();
    }

    private static string Serialize(PluginInfoRecord pluginInfoRecord)
    {
        List<string> serializedPluginInfo =
        [
            Serialize(pluginInfoRecord.GUID),
            Serialize(pluginInfoRecord.Version),
            Serialize(pluginInfoRecord.CompatibilityLevel),
            Serialize(pluginInfoRecord.VersionStrictness)
        ];

        return serializedPluginInfo.Join(delimiter: FieldSeparator.ToString());
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

    public static IEnumerable<PluginInfoRecord> Deserialize(IEnumerable<string> paginatedSerializedPlugins)
    {
        var serializedPluginList = paginatedSerializedPlugins.Join(delimiter: PluginSeparator.ToString()).Split(PluginSeparator);

        return serializedPluginList.Select(DeserializePluginInfoRecord);
    }

    private static PluginInfoRecord DeserializePluginInfoRecord(string pluginInfoRecord)
    {
        var splitRecord = pluginInfoRecord.Split(FieldSeparator);

        return new PluginInfoRecord(
            DeserializeGuid(splitRecord[0]),
            DeserializeVersion(splitRecord[1]),
            DeserializeCompatibilityLevel(splitRecord[2]),
            DeserializeVersionStrictness(splitRecord[3])
            );
    }

    private static string DeserializeGuid(string pluginGuid) => pluginGuid;

    private static Version DeserializeVersion(string version) => Version.Parse(version);

    private static CompatibilityLevel? DeserializeCompatibilityLevel(string compatibilityLevel)
    {
        return compatibilityLevel switch
        {
            "c" => CompatibilityLevel.ClientOnly,
            "s" => CompatibilityLevel.ServerOnly,
            "e" => CompatibilityLevel.Everyone,
            "o" => CompatibilityLevel.ClientOptional,
            _ => null
        };
    }

    private static VersionStrictness? DeserializeVersionStrictness(string versionStrictness)
    {
        return versionStrictness switch
        {
            "o" => VersionStrictness.None,
            "m" => VersionStrictness.Major,
            "n" => VersionStrictness.Minor,
            "p" => VersionStrictness.Patch,
            _ => null
        };
    }
}