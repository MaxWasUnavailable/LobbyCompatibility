using BepInEx.Configuration;
using LobbyCompatibility.Enums;
using UnityEngine;

namespace LobbyCompatibility.Configuration;

/// <summary>
///     BepInEx Config used to handle any configurable mod values.
/// </summary>
public class Config
{
    /// <summary>
    ///     Default <see cref="Color"/> used to represent compatible plugins.
    /// </summary>
    public static readonly Color DefaultCompatibleColor = Color.green;

    /// <summary>
    ///     Default <see cref="Color"/> used to represent incompatible plugins.
    /// </summary>
    public static readonly Color DefaultIncompatibleColor = Color.red;

    /// <summary>
    ///     Default <see cref="Color"/> used to represent unknown plugins.
    /// </summary>
    public static readonly Color DefaultUnknownColor = Color.gray;

    /// <summary>
    ///     Default <see cref="ModdedLobbyFilter"/> value for public lobby sorting.
    /// </summary>
    public ConfigEntry<ModdedLobbyFilter> DefaultModdedLobbyFilter;

    /// <summary>
    ///     Default <see cref="ModListFilter"/> when viewing a lobby's mod list.
    /// </summary>
    public ConfigEntry<ModListFilter> DefaultModListTab;

    /// <summary>
    ///     <see cref="Color"/> used to represent compatible plugins.
    /// </summary>
    public ConfigEntry<Color> CompatibleColor;

    /// <summary>
    ///     <see cref="Color"/> used to represent incompatible plugins.
    /// </summary>
    public ConfigEntry<Color> IncompatibleColor;

    /// <summary>
    ///     <see cref="Color"/> used to represent unknown plugins.
    /// </summary>
    public ConfigEntry<Color> UnknownColor;

    public Config(ConfigFile configFile)
    {
        DefaultModdedLobbyFilter = configFile.Bind("General",
            "Default Lobby Filter Type",
            ModdedLobbyFilter.CompatibleFirst,
            "The default filter to apply when searching for a public lobby");
        DefaultModListTab = configFile.Bind("General",
            "Default ModList Tab",
            ModListFilter.All,
            "The default tab to use when viewing a lobby's mod list");
        CompatibleColor = configFile.Bind("Visual",
            "Compatible Plugin Color",
            DefaultCompatibleColor,
            "The color used to respresent compatible plugins");
        IncompatibleColor = configFile.Bind("Visual",
            "Incompatible Plugin Color",
            DefaultIncompatibleColor,
            "The color used to respresent incompatible plugins");
        UnknownColor = configFile.Bind("Visual",
            "Unknown Plugin Color",
            DefaultUnknownColor,
            "The color used to respresent unknown plugins");
    }
}
