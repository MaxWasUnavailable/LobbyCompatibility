using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Steamworks;
using Steamworks.Data;

namespace LobbyCompatibility.Features;

/// <summary>
///     Helper class for Lobby related functions.
/// </summary>
public static class LobbyHelper
{
    public static readonly Regex ModdedLobbyIdentifierRegex = new Regex(@"(\bmod(s|ded)?\b)", RegexOptions.IgnoreCase);

    public static Dictionary<string, string> LatestLobbyRequestStringFilters = new();
    public static LobbyDistanceFilter? LatestLobbyRequestDistanceFilter;
    private static List<PluginInfoRecord>? _clientPlugins;
    public static LobbyDiff LatestLobbyDiff { get; private set; } = new(new List<PluginDiff>());
    private static Dictionary<ulong, LobbyDiff> LobbyDiffCache { get; } = new();
    
    /// <summary>
    ///     The absolute maximum string size for ALL steam lobby metadata is 8192 (2^13).
    ///     We want to give a large enough margin for the checksum, vanilla game metadata, and any other modded metadata.
    /// </summary>
    private const int MaxPluginMetadataLength = 7800;

    /// <summary>
    ///     The average json string size of a plugin.
    /// </summary>
    /// <remarks> Calculated from `{"i":"BMX.LobbyCompatibility","v":"1.4.1","c":null,"s":null}` </remarks>
    private const int AveragePluginJsonLength = 60;

    /// <summary>
    ///     The current maximum string size for the plugin field in lobby metadata.
    /// </summary>
    /// <remarks> WARNING: The absolute maximum string size for ALL steam lobby metadata is 8192 (2^13). </remarks>
    public static int CurrentMaxPluginMetadataLength { get; private set; } = MaxPluginMetadataLength;

    /// <summary>
    ///     The maximum string size for ALL steam lobby metadata is 8192 (2^13).
    ///     We want to give a large margin for the checksum, vanilla game metadata, and any other modded metadata.
    /// </summary>
    private static int AverageMaxLobbyMetadataModCount => CurrentMaxPluginMetadataLength / AveragePluginJsonLength;

    /// <summary>
    ///     Get a <see cref="LobbyDiff" /> from a <see cref="Lobby" />.
    /// </summary>
    /// <param name="lobby"> The lobby to get the diff from. </param>
    /// <returns> The <see cref="LobbyDiff" /> from the <see cref="Lobby" />. </returns>
    public static LobbyDiff GetLobbyDiff(Lobby? lobby) => GetLobbyDiff(lobby, null);

    /// <summary>
    ///     Get a <see cref="LobbyDiff" /> from a <see cref="Lobby" />.
    /// </summary>
    /// <param name="lobbyData"> The lobby to get the diff from. </param>
    /// <returns> The <see cref="LobbyDiff" /> from the <see cref="Lobby" />. </returns>
    public static LobbyDiff GetLobbyDiff(IEnumerable<KeyValuePair<string, string>> lobbyData) => GetLobbyDiff(null, null, lobbyData);

    /// <summary>
    ///     Reserve space in the lobby metadata.
    ///     Useful if you need larger space for lobby metadata for your mod.
    /// </summary>
    /// <param name="length"> The amount to metadata space in bytes space to reserve. The amound of bytes LobbyCompatibility will use is reduced by this amount. </param>
    public static void ReserveLobbyMetadataSpace(int length)
    {
        CurrentMaxPluginMetadataLength -= length;
    }

    /// <summary>
    ///     Get a <see cref="LobbyDiff" /> from a <see cref="Lobby" /> or <see cref="IEnumerable{String}" />.
    /// </summary>
    /// <param name="lobby"> The lobby to cache the diff to and/or get the diff from. </param>
    /// <param name="lobbyPluginString"> The json string to parse. </param>
    /// <param name="lobbyData"> (Opt.) The lobby data. </param>
    /// <returns> The <see cref="LobbyDiff" />. </returns>
    internal static LobbyDiff GetLobbyDiff(Lobby? lobby, string? lobbyPluginString, IEnumerable<KeyValuePair<string, string>>? lobbyData = null)
    {
        if (lobby.HasValue && LobbyDiffCache.TryGetValue(lobby.Value.Id, out var cachedLobbyDiff))
        {
            LatestLobbyDiff = cachedLobbyDiff;
            return cachedLobbyDiff;
        }

        var lobbyDataList = lobbyData?.ToList();

        var lobbyPlugins = ParseLobbyPluginsMetadata(lobbyPluginString ?? (lobby.HasValue 
            ? GetLobbyPlugins(lobby.Value)
            : lobbyDataList != null 
                ? GetLobbyPlugins(lobbyDataList) 
                : string.Empty)).ToList();
        _clientPlugins = PluginHelper.GetAllPluginInfo().CalculateCompatibilityLevel(lobby, lobbyDataList);

        var pluginDiffs = new List<PluginDiff>();

        foreach (var lobbyPlugin in lobbyPlugins)
        {
            var clientPlugin = _clientPlugins.FirstOrDefault(plugin => plugin.GUID == lobbyPlugin.GUID);

            if (lobbyPlugin.CompatibilityLevel == null || lobbyPlugin.VersionStrictness == null)
            {
                var clientVersion = clientPlugin?.Version;
                // Unknown mods with the same version are implicitly compatible, and will be added as compatible here
                pluginDiffs.Add(new PluginDiff(
                    clientVersion == lobbyPlugin.Version ? PluginDiffResult.Compatible : PluginDiffResult.Unknown,
                    lobbyPlugin.GUID, clientVersion, lobbyPlugin.Version));
                continue;
            }

            if (clientPlugin == null)
            {
                if (lobbyPlugin.CompatibilityLevel == CompatibilityLevel.Everyone)
                    pluginDiffs.Add(new PluginDiff(PluginDiffResult.ClientMissingMod, lobbyPlugin.GUID, null,
                        lobbyPlugin.Version));

                else
                    pluginDiffs.Add(new PluginDiff(PluginDiffResult.Compatible, lobbyPlugin.GUID, null,
                        lobbyPlugin.Version));

                continue;
            }

            if (clientPlugin.CompatibilityLevel != lobbyPlugin.CompatibilityLevel)
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ModVersionMismatch, lobbyPlugin.GUID,
                    clientPlugin.Version, lobbyPlugin.Version));

            else if (clientPlugin.CompatibilityLevel != CompatibilityLevel.ClientOnly &&
                     !PluginHelper.MatchesVersion(clientPlugin, lobbyPlugin))
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ModVersionMismatch, lobbyPlugin.GUID,
                    clientPlugin.Version, lobbyPlugin.Version));

            else
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.Compatible, lobbyPlugin.GUID, clientPlugin.Version,
                    lobbyPlugin.Version));
        }

        foreach (var clientPlugin in _clientPlugins)
        {
            var lobbyPlugin = lobbyPlugins.FirstOrDefault(plugin => plugin.GUID == clientPlugin.GUID);

            if (clientPlugin.CompatibilityLevel == null || clientPlugin.VersionStrictness == null)
            {
                var lobbyVersion = lobbyPlugin?.Version;
                if (lobbyVersion != clientPlugin.Version)
                    pluginDiffs.Add(new PluginDiff(PluginDiffResult.Unknown, clientPlugin.GUID, clientPlugin.Version,
                        lobbyVersion));
                continue;
            }

            if (lobbyPlugin != null) continue;

            if (clientPlugin.CompatibilityLevel is CompatibilityLevel.Everyone or CompatibilityLevel.ClientOptional)
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.ServerMissingMod, clientPlugin.GUID,
                    clientPlugin.Version, null));

            else
                pluginDiffs.Add(new PluginDiff(PluginDiffResult.Compatible, clientPlugin.GUID,
                    clientPlugin.Version, null));
        }

        var lobbyCompatibilityPresent = lobbyPlugins.Any();

        LatestLobbyDiff = new LobbyDiff(pluginDiffs, lobbyCompatibilityPresent);

        // Add to cache to avoid making multiple unnecessary GetData() calls
        if (lobby.HasValue)
            LobbyDiffCache.Add(lobby.Value.Id, LatestLobbyDiff);

        return LatestLobbyDiff;
    }
    
    /// <summary>
    ///     Get the plugins json from a <see cref="Lobby" />.
    /// </summary>
    /// <param name="lobby"> The <see cref="Lobby" /> to get the json string from. </param>
    /// <returns> A json <see cref="string" /> from the <see cref="Lobby" />. </returns>
    internal static string GetLobbyPlugins(Lobby lobby)
    {
        if (GameNetworkManager.Instance && !GameNetworkManager.Instance.disableSteam)
        {
            return lobby.GetData($"{LobbyMetadata.Plugins}0");
        }

        return string.Empty;
    }
    
    /// <summary>
    ///     Get the plugins json from lobby data.
    /// </summary>
    /// <param name="lobbyData"> The lobby data that has the json string. </param>
    /// <returns> A json <see cref="string" /> from the lobby data. </returns>
    internal static string GetLobbyPlugins(List<KeyValuePair<string, string>> lobbyData)
    {
        var lobbyPluginStrings = new List<string>();

        if (GameNetworkManager.Instance)
        {
            var i = 0;
            do lobbyPluginStrings.Insert(i, lobbyData.FirstOrDefault(x => x.Key == $"{LobbyMetadata.Plugins}{i}").Value);
            while (lobbyPluginStrings[i++].Contains("@"));
        }

        return lobbyPluginStrings
            .Join(delimiter: string.Empty)
            .Replace("@", string.Empty);
    }
    
    /// <summary>
    ///     Creates a json string containing the metadata of the maximum amount of plugins, sorted by highest compatibility requirement first.
    /// </summary>
    /// <returns> A json strings containing the maximum allowed mod count, as dictated by <see cref="CurrentMaxPluginMetadataLength"/>. </returns>
    internal static string GetLobbyPluginsMetadata(List<PluginInfoRecord>? plugins = null)
    {
        plugins ??= PluginHelper.GetAllPluginInfo().ToList();

        plugins.Sort();

        var allowedPluginInfoRecords = plugins.Take(AverageMaxLobbyMetadataModCount).ToList();

        if (allowedPluginInfoRecords.Sum(record => record.JsonLength) + 1 + allowedPluginInfoRecords.Count > CurrentMaxPluginMetadataLength)
        {
            do
            {
                allowedPluginInfoRecords.RemoveAt(allowedPluginInfoRecords.Count - 1);
            } while (allowedPluginInfoRecords.Sum(record => record.JsonLength) + 1 + allowedPluginInfoRecords.Count > CurrentMaxPluginMetadataLength);
        
            return JsonConvert.SerializeObject(allowedPluginInfoRecords, new VersionConverter());
        }

        do
        {
            allowedPluginInfoRecords.Add(plugins[allowedPluginInfoRecords.Count]);
        } while (allowedPluginInfoRecords.Sum(record => record.JsonLength) + 1 + allowedPluginInfoRecords.Count <= CurrentMaxPluginMetadataLength);
        
        allowedPluginInfoRecords.RemoveAt(allowedPluginInfoRecords.Count - 1);
        
        return JsonConvert.SerializeObject(allowedPluginInfoRecords, new VersionConverter());
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

    public static string GetCompatibilityHeader(PluginDiffResult pluginDiffResult)
    {
        return pluginDiffResult switch
        {
            PluginDiffResult.Compatible => "Compatible",
            PluginDiffResult.ClientMissingMod => "Missing lobby-required mods",
            PluginDiffResult.ServerMissingMod => "Incompatible with lobby",
            PluginDiffResult.ModVersionMismatch => "Incompatible mod versions",
            _ => "Unknown"
        };
    }

    /// <summary>
    ///     Filter lobbies based on a <see cref="ModdedLobbyFilter" />.
    /// </summary>
    /// <param name="normalLobbies"> The base game list of lobbies, with no special search filters applied. </param>
    /// <param name="filteredLobbies"> A custom list of lobbies, with special search filters applied. </param>
    /// <param name="currentFilter"> The <see cref="ModdedLobbyFilter" /> to filter the lobbies against. </param>
    /// <returns> A <see cref="Lobby" /> array with the <see cref="ModdedLobbyFilter" /> applied. </returns>
    public static Lobby[] FilterLobbies(Lobby[] normalLobbies, Lobby[]? filteredLobbies,
        ModdedLobbyFilter currentFilter)
    {
        List<Lobby> allLobbies = new();

        if (filteredLobbies != null)
            // Remove duplicate "normal" lobbies if they were also caught by the hashfilter
            normalLobbies = normalLobbies
                .Where(lobby => !filteredLobbies.Any(check => lobby.Equals(check)))
                .ToArray();

        if (currentFilter == ModdedLobbyFilter.VanillaAndUnknownOnly)
        {
            // TODO: If we have an abundance of time, see if we can do a hashfilter-esque way to filter for vanilla/unknown lobbies
            // I initially intended in checking for a null value of LobbyMetadata.Modded, but there's no way to make steam filter for null string values, as far as I can tell
            // So we'll likely need to change the game's version, or change base game metadata in an otherwise detectable/filterable way
            // I'm not sure if this is possible without breaking vanilla compatibility?

            // Add only lobbies that are vanilla/unknown
            allLobbies.AddRange(FilterLobbiesByDiffResult(normalLobbies, LobbyDiffResult.Unknown));
        }
        else if (filteredLobbies != null && currentFilter is ModdedLobbyFilter.CompatibleFirst or ModdedLobbyFilter.CompatibleOnly)
        {
            // Lobbies returned by the hashfilter are not 100% going to always be compatible, so we'll still need to filter them
            var (compatibleFilteredLobbies, otherFilteredLobbies) =
                SplitLobbiesByDiffResult(filteredLobbies, LobbyDiffResult.Compatible);
            var (compatibleNormalLobbies, otherNormalLobbies) =
                SplitLobbiesByDiffResult(normalLobbies, LobbyDiffResult.Compatible);

            // Add filtered lobbies that are 100% compatible first, then any extra compatible lobbies not caught by the hashfilter
            allLobbies.AddRange(compatibleFilteredLobbies);
            allLobbies.AddRange(compatibleNormalLobbies);

            if (currentFilter == ModdedLobbyFilter.CompatibleFirst)
            {
                // Run a second pass to further seperate presumed compatible and non-compatible lobbies
                var (presumedCompatibleFilteredLobbies, incompatibleFilteredLobbies) =
                    SplitLobbiesByDiffResult(otherFilteredLobbies, LobbyDiffResult.PresumedCompatible);
                var (presumedCompatibleNormalLobbies, incompatibleNormalLobbies) =
                    SplitLobbiesByDiffResult(normalLobbies, LobbyDiffResult.PresumedCompatible);

                // Add presumed compatible lobbies as a secondary, as they're very likely compatible (but not 100%)
                allLobbies.AddRange(presumedCompatibleFilteredLobbies);
                allLobbies.AddRange(presumedCompatibleNormalLobbies);

                // Finally, add the non-compatible lobbies. We want to prioritize hashfilter non-compatible lobbies, as they're likely closer to compatibility.
                allLobbies.AddRange(incompatibleFilteredLobbies);
                allLobbies.AddRange(incompatibleNormalLobbies);
            }
        }
        else if (filteredLobbies == null && currentFilter is ModdedLobbyFilter.CompatibleFirst)
        {
            // Even if we can't find any filtered lobbies, try to sort any compatible lobbies we happen to find towards the front
            var (compatibleNormalLobbies, otherNormalLobbies) =
                SplitLobbiesByDiffResult(normalLobbies, LobbyDiffResult.Compatible);
            var (presumedCompatibleNormalLobbies, incompatibleNormalLobbies) =
                SplitLobbiesByDiffResult(normalLobbies, LobbyDiffResult.PresumedCompatible);

            allLobbies.AddRange(compatibleNormalLobbies);
            allLobbies.AddRange(presumedCompatibleNormalLobbies);
            allLobbies.AddRange(incompatibleNormalLobbies);
        }
        else if (filteredLobbies == null && currentFilter == ModdedLobbyFilter.CompatibleOnly)
        {
            // Handle the special case where we're sorting for compatible only and nothing comes up, so we need to force return nothing
            allLobbies = new List<Lobby>();
        }
        else
        {
            // no need for special filtering or sorting if we're filtering for "All"
            allLobbies = normalLobbies.ToList();
        }

        return allLobbies.ToArray();
    }

    /// <summary>
    ///     Splits a <see cref="Lobby" /> IEnumerable into two arrays based on of it matches <see cref="LobbyDiffResult" /> or
    ///     not.
    /// </summary>
    /// <param name="lobbies"> The lobbies. </param>
    /// <param name="filteredLobbyDiffResult"> The <see cref="LobbyDiffResult" /> to match against. </param>
    /// <returns>
    ///     A tuple containing two <see cref="Lobby" /> IEnumerables. matchedLobbies contains the lobbies that match the
    ///     LobbyDiffResult, and unmatchedLobbies contains everything else.
    /// </returns>
    private static (IEnumerable<Lobby> matchedLobbies, IEnumerable<Lobby> unmatchedLobbies) SplitLobbiesByDiffResult(
        IEnumerable<Lobby> lobbies, LobbyDiffResult filteredLobbyDiffResult)
    {
        List<Lobby> matchedLobbies = new();
        List<Lobby> unmatchedLobbies = new();

        foreach (var lobby in lobbies)
        {
            var lobbyDiffResult = GetLobbyDiff(lobby).GetModdedLobbyType();
            if (lobbyDiffResult == filteredLobbyDiffResult)
                matchedLobbies.Add(lobby);
            else
                unmatchedLobbies.Add(lobby);
        }

        return (matchedLobbies, unmatchedLobbies);
    }

    /// <summary>
    ///     Filters a <see cref="Lobby" /> IEnumerable based on if it matches <see cref="LobbyDiffResult" />.
    /// </summary>
    /// <param name="lobbies"> The lobbies. </param>
    /// <param name="filteredLobbyDiffResult"> The <see cref="LobbyDiffResult" /> to match against. </param>
    /// <returns> A filtered <see cref="Lobby" /> IEnumerable. </returns>
    private static IEnumerable<Lobby> FilterLobbiesByDiffResult(IEnumerable<Lobby> lobbies,
        LobbyDiffResult filteredLobbyDiffResult)
    {
        return SplitLobbiesByDiffResult(lobbies, filteredLobbyDiffResult).matchedLobbies;
    }

    /// <summary>
    ///     Gets the error message for when no lobbies are found using a <see cref="ModdedLobbyFilter" />.
    /// </summary>
    /// <param name="moddedLobbyFilter"> The <see cref="ModdedLobbyFilter" /> to get the error message for. </param>
    public static string GetEmptyLobbyListString(ModdedLobbyFilter moddedLobbyFilter)
    {
        return moddedLobbyFilter switch
        {
            ModdedLobbyFilter.CompatibleOnly => "No available compatible\nservers to join.",
            ModdedLobbyFilter.VanillaAndUnknownOnly => "No available vanilla or unknown\nservers to join.",
            _ => "No available servers to join."
        };
    }

    /// <summary>
    ///     Returns true if a lobby name already contains a mod identifier, such as [MOD] or modded
    /// </summary>
    /// <param name="lobby"> The <see cref="Lobby" /> to check. </param>
    /// <returns> True if the lobby name contains a mod idenitfier. </returns>
    public static bool LobbyNameContainsModIdentifier(Lobby lobby)
    {
        return ModdedLobbyIdentifierRegex.IsMatch(lobby.GetData(LobbyMetadata.Name));
    }
}