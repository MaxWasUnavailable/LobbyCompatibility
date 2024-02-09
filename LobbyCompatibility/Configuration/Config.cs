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

    public Config(ConfigFile configFile)
    {
        DefaultModdedLobbyFilter = configFile.Bind("General",
            "Default Lobby Filter Type",
            ModdedLobbyFilter.CompatibleFirst,
            "The default filter to apply when searching for a public lobby");
    }
}
