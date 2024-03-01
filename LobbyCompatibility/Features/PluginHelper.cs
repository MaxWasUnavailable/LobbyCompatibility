using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LobbyCompatibility.Features;

/// <summary>
///     Helper class for plugin related functions.
/// </summary>
internal static class PluginHelper
{
    /// <summary>
    ///     PluginInfos registered through the register command, rather than found using the attribute.
    /// </summary>
    private static readonly List<PluginInfoRecord> RegisteredPluginInfoRecords = new();
    
    /// <summary>
    ///     Cached checksum of the required plugins.
    /// </summary>
    private static string? _cachedChecksum;

    /// <summary>
    ///     Get the checksum of the required plugins, using the cached value if available.
    /// </summary>
    internal static string Checksum
    {
        get => _cachedChecksum ?? GetRequiredPluginsChecksum();
        set => _cachedChecksum = value;
    }
    
    /// <summary>
    ///     Register a plugin's compatibility information manually.
    /// </summary>
    /// <param name="guid"> The GUID of the plugin. </param>
    /// <param name="version"> The version of the plugin. </param>
    /// <param name="compatibilityLevel"> The compatibility level of the plugin. </param>
    /// <param name="versionStrictness"> The version strictness of the plugin. </param>
    public static void RegisterPlugin(string guid, Version version, CompatibilityLevel compatibilityLevel, VersionStrictness versionStrictness)
    {
        RegisteredPluginInfoRecords.Add(new PluginInfoRecord(guid, version, compatibilityLevel, versionStrictness));
        _cachedChecksum = null;
    }

    /// <summary>
    ///     Check if a plugin has the <see cref="LobbyCompatibilityAttribute" /> attribute.
    /// </summary>
    /// <param name="plugin"> The plugin to check. </param>
    private static bool HasCompatibilityAttribute(BaseUnityPlugin plugin)
    {
        return plugin.GetType().GetCustomAttributes(typeof(LobbyCompatibilityAttribute), false).Any();
    }

    /// <summary>
    ///     Get all plugins that have the <see cref="LobbyCompatibilityAttribute" /> attribute.
    /// </summary>
    private static IEnumerable<BepInEx.PluginInfo> GetCompatibilityPlugins()
    {
        return Chainloader.PluginInfos.Where(plugin =>
            HasCompatibilityAttribute(plugin.Value.Instance)).Select(plugin => plugin.Value);
    }

    /// <summary>
    ///     Get the <see cref="LobbyCompatibilityAttribute" /> attribute of a plugin.
    /// </summary>
    /// <param name="plugin"> The plugin to get the attribute from. </param>
    /// <returns> The <see cref="LobbyCompatibilityAttribute" /> attribute of the plugin. </returns>
    private static LobbyCompatibilityAttribute? GetCompatibilityAttribute(BaseUnityPlugin plugin)
    {
        return (LobbyCompatibilityAttribute?)plugin.GetType()
            .GetCustomAttributes(typeof(LobbyCompatibilityAttribute), false).FirstOrDefault();
    }

    /// <summary>
    ///     Get all plugins in the <see cref="PluginInfoRecord" /> format.
    /// </summary>
    /// <returns> An IEnumerable of plugins in the <see cref="PluginInfoRecord" /> format. </returns>
    internal static IEnumerable<PluginInfoRecord> GetAllPluginInfo()
    {
        var pluginInfos = new List<PluginInfoRecord>();

        var compatibilityPlugins = GetCompatibilityPlugins().ToList();
        var nonCompatibilityPlugins = Chainloader.PluginInfos.Where(plugin =>
            !HasCompatibilityAttribute(plugin.Value.Instance)).Select(plugin => plugin.Value).ToList();

        pluginInfos.AddRange(compatibilityPlugins.Select(plugin =>
            new PluginInfoRecord(plugin.Metadata.GUID, plugin.Metadata.Version,
                GetCompatibilityAttribute(plugin.Instance)?.CompatibilityLevel ?? null,
                GetCompatibilityAttribute(plugin.Instance)?.VersionStrictness ?? null)));

        pluginInfos.AddRange(nonCompatibilityPlugins.Select(plugin =>
            new PluginInfoRecord(plugin.Metadata.GUID, plugin.Metadata.Version, null, null)));

        return pluginInfos.Concat(RegisteredPluginInfoRecords);
    }

    /// <summary>
    ///     Creates a list of json strings containing the metadata of all plugins, to add to the lobby.
    /// </summary>
    /// <returns> A list of json strings containing the metadata of all plugins. </returns>
    internal static IEnumerable<string> GetLobbyPluginsMetadata()
    {
        var json = JsonConvert.SerializeObject(GetAllPluginInfo().ToList(), new VersionConverter());

        // The maximum string size for steam lobby metadata is 8192 (2^13).
        // We want one less than the maximum to allow space for a delimiter
        var maxChunkLength = 8191;
        
        for (var i = 0; i < json.Length; i += maxChunkLength)
        {
            if (maxChunkLength + i > json.Length)
                maxChunkLength = json.Length - i;

            yield return json.Substring(i, maxChunkLength);
        }
    }

    /// <summary>
    ///     Parses a json string containing the metadata of all plugins.
    /// </summary>
    /// <param name="json"> The json string to parse. </param>
    /// <returns> A list of plugins in the APIPluginInfo format. </returns>
    internal static IEnumerable<PluginInfoRecord> ParseLobbyPluginsMetadata(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<List<PluginInfoRecord>>(json, new VersionConverter()) ??
                   new List<PluginInfoRecord>();
        }
        catch (Exception e)
        {
            LobbyCompatibilityPlugin.Logger?.LogError("Failed to parse lobby plugins metadata.");
            LobbyCompatibilityPlugin.Logger?.LogDebug(e);
            throw;
        }
    }

    /// <summary>
    ///     Checks if a plugin matches the version of a target, according to the source's version strictness.
    /// </summary>
    /// <param name="source"> The source plugin. </param>
    /// <param name="target"> The target plugin. </param>
    /// <returns> True if the plugin matches the version of the target, false otherwise. </returns>
    internal static bool MatchesVersion(PluginInfoRecord source, PluginInfoRecord target)
    {
        if (source.VersionStrictness == VersionStrictness.None)
            return true;

        if (source.VersionStrictness == VersionStrictness.Major)
        {
            if (target.Version.Major != source.Version.Major)
                return false;
        }
        else if (source.VersionStrictness == VersionStrictness.Minor)
        {
            if (target.Version.Major != source.Version.Major ||
                target.Version.Minor != source.Version.Minor)
                return false;
        }
        else if (source.VersionStrictness == VersionStrictness.Patch)
        {
            if (target.Version != source.Version) return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if the client and server match plugin requirements.
    /// </summary>
    /// <param name="targetPluginInfo"> The plugin info of the target. </param>
    internal static bool MatchesTargetRequirements(IEnumerable<PluginInfoRecord> targetPluginInfo)
    {
        var clientPluginInfoList = GetAllPluginInfo().ToList();
        var serverPluginInfoList = targetPluginInfo.ToList();

        foreach (var clientPlugin in clientPluginInfoList)
            if (clientPlugin.CompatibilityLevel is CompatibilityLevel.Everyone or CompatibilityLevel.ClientOptional)
                if (!serverPluginInfoList.Any(serverPlugin =>
                        serverPlugin.GUID == clientPlugin.GUID && MatchesVersion(clientPlugin, serverPlugin)))
                    return false;

        foreach (var serverPlugin in serverPluginInfoList)
            if (serverPlugin.CompatibilityLevel is CompatibilityLevel.Everyone)
                if (!clientPluginInfoList.Any(clientPlugin =>
                        clientPlugin.GUID == serverPlugin.GUID && MatchesVersion(serverPlugin, clientPlugin)))
                    return false;

        return true;
    }

    /// <summary>
    ///     Checks if client is allowed to join vanilla or unknown lobbies.
    /// </summary>
    /// <returns> True if client is allowed to join vanilla or unknown lobbies, false otherwise. </returns>
    /// <remarks> This means the client is allowed to have unknown or clientside mods. </remarks>
    internal static bool CanJoinVanillaLobbies()
    {
        // Disabled until we have enough people using the mod to make this realistic to do
        // If this was enabled immediately in first release, you would be unable to play with anyone who's missing LC, which will be nearly everyone
        /* return GetAllPluginInfo().All(plugin =>
            plugin.CompatibilityLevel != CompatibilityLevel.Everyone &&
            plugin.CompatibilityLevel != CompatibilityLevel.ClientOptional); */

        return true;
    }

    /// <summary>
    ///     Filter <see cref="PluginDiff"/> list based on a <see cref="ModListFilter"/>.
    /// </summary>
    /// <param name="pluginDiffs"> A list of plugin diffs to apply the filter to. </param>
    /// <param name="modListFilter"> The <see cref="ModListFilter" /> to filter the plugin diffs against. </param>
    /// <returns> A <see cref="PluginDiff" /> array with the <see cref="ModListFilter" /> applied. </returns>
    public static IEnumerable<PluginDiff> FilterPluginDiffs(IEnumerable<PluginDiff> pluginDiffs, ModListFilter modListFilter)
    {
        if (modListFilter == ModListFilter.Compatible)
        {
            return pluginDiffs.Where(x => x.PluginDiffResult == PluginDiffResult.Compatible);
        }
        else if (modListFilter == ModListFilter.Incompatible)
        {
            return pluginDiffs.Where(x =>
                x.PluginDiffResult == PluginDiffResult.ServerMissingMod
                || x.PluginDiffResult == PluginDiffResult.ClientMissingMod
                || x.PluginDiffResult == PluginDiffResult.ModVersionMismatch
            );
        }
        else if (modListFilter == ModListFilter.Unknown)
        {
            return pluginDiffs.Where(x => x.PluginDiffResult == PluginDiffResult.Unknown);
        }

        return pluginDiffs;
    }

    /// <summary>
    ///     Creates a checksum of all <see cref="CompatibilityLevel.Everyone" /> level plugins at their lowest acceptable
    ///     version.
    /// </summary>
    /// <returns> The generated filter checksum of installed plugins </returns>
    private static string GetRequiredPluginsChecksum()
    {
        // Get the required plugins and sort to guarantee consistency between all clients.
        var requiredPlugins = GetAllPluginInfo()
            .Where(plugin => plugin.CompatibilityLevel is CompatibilityLevel.Everyone)
            .OrderBy(plugin => plugin.GUID, StringComparer.Ordinal).ToList();

        if (!requiredPlugins.Any())
            return _cachedChecksum = "";

        var pluginString = "";

        foreach (var plugin in requiredPlugins)
        {
            pluginString += plugin.GUID;

            // ReSharper disable twice RedundantCaseLabel
            switch (plugin.VersionStrictness)
            {
                default:
                case null:
                case VersionStrictness.None:
                    break;
                case VersionStrictness.Major:
                    pluginString += new Version(plugin.Version.Major, 0).ToString();
                    break;
                case VersionStrictness.Minor:
                    pluginString += new Version(plugin.Version.Major, plugin.Version.Minor).ToString();
                    break;
                case VersionStrictness.Patch:
                    pluginString += plugin.Version.ToString();
                    break;
            }
        }

        var checksum = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pluginString));

        var stringBuilder = new StringBuilder();

        // Convert every byte to hexadecimal
        foreach (var checksumByte in checksum)
            stringBuilder.Append(checksumByte.ToString("X2"));

        _cachedChecksum = stringBuilder.ToString();
        
        LobbyCompatibilityPlugin.Logger?.LogDebug(_cachedChecksum);

        return _cachedChecksum;
    }
}
