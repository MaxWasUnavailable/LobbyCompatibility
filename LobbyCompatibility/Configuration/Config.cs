using BepInEx.Configuration;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Configuration;

/// <summary>
///     BepInEx Config used to handle any configurable mod values.
/// </summary>
public class Config
{
    /// <summary>
    ///     Default <see cref="ModdedLobbyFilter"/> value for public lobby sorting.
    /// </summary>
    public ConfigEntry<ModdedLobbyFilter> DefaultModdedLobbyFilter;

    /// <summary>
    ///     Default <see cref="ModListFilter"/> when viewing a lobby's mod list.
    /// </summary>
    public ConfigEntry<ModListFilter> DefaultModListTab;

    public Config(ConfigFile configFile)
    {
        DefaultModdedLobbyFilter = configFile.Bind("General",
            "Default Lobby Filter Type",
            ModdedLobbyFilter.CompatibleFirst,
            "The default filter to apply when searching for a public lobby");
        DefaultModListTab = configFile.Bind("General",
            "Default ModList Tab",
            ModListFilter.All,
            "The default tab to use when viewing a lobby's mod list.");
    }
}
