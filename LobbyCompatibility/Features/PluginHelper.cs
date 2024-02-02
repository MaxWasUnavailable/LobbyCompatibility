using System;
using System.Collections.Generic;
using System.Linq;
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

        return pluginInfos;
    }

    /// <summary>
    ///     Creates a json string containing the metadata of all plugins, to add to the lobby.
    /// </summary>
    /// <returns> A json string containing the metadata of all plugins. </returns>
    public static string GetLobbyPluginsMetadata()
    {
        return JsonConvert.SerializeObject(GetAllPluginInfo().ToList(), new VersionConverter());
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
    ///     Checks if client is allowed to join vanilla lobbies.
    /// </summary>
    /// <returns> True if client is allowed to join vanilla lobbies, false otherwise. </returns>
    internal static bool CanJoinVanillaLobbies()
    {
        return GetAllPluginInfo().All(plugin => plugin.CompatibilityLevel is CompatibilityLevel.ClientOnly or null);
    }

    /// <summary>
    ///     Creates a checksum of all <see cref="CompatibilityLevel.Everyone"/> level plugins at their lowest acceptable version.
    /// </summary>
    /// <returns> The generated filter checksum of installed plugins </returns>
    internal static string GetRequiredPluginsChecksum()
    {
        // After initial generation, return the cached checksum.
        if (_cachedChecksum != null)
            return _cachedChecksum;
        
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
                case VersionStrictness.Major:
#if DEBUG
                    LobbyCompatibilityPlugin.Logger!.LogDebug(new Version(plugin.Version.Major, 0, 0).ToString());
#endif
                    pluginString += new Version(plugin.Version.Major, 0, 0).ToString();
                    break;
                case VersionStrictness.Minor:
#if DEBUG
                    LobbyCompatibilityPlugin.Logger!.LogDebug(new Version(plugin.Version.Major, plugin.Version.Minor, 0).ToString());
#endif
                    pluginString += new Version(plugin.Version.Major, plugin.Version.Minor, 0).ToString();
                    break;
                case VersionStrictness.Patch:
#if DEBUG
                    LobbyCompatibilityPlugin.Logger!.LogDebug(plugin.Version.ToString());
#endif
                    pluginString += plugin.Version.ToString();
                    break;
                case VersionStrictness.None:
                case null:
                default:
                    break;
            }
        }
        
        var checksum = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(pluginString));
        
        var sb = new StringBuilder();
        foreach (var checksumByte in checksum)
            sb.Append(checksumByte.ToString("X2")); // Convert every byte to hexadecimal
        
#if DEBUG
        LobbyCompatibilityPlugin.Logger!.LogError(sb.ToString());
#endif
        
        return _cachedChecksum = sb.ToString();
    }

    private static string? _cachedChecksum;
}