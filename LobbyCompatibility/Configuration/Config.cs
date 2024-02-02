using BepInEx.Configuration;
using LobbyCompatibility.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LobbyCompatibility.Configuration
{
    public class Config
    {
        public ConfigEntry<ModdedLobbyFilter> DefaultModdedLobbyFilter;

        public Config(ConfigFile configFile)
        {
            DefaultModdedLobbyFilter = configFile.Bind("General",
                "Default Lobby Filter Type",
                ModdedLobbyFilter.CompatibleFirst,
                "The default filter to apply when searching for a public lobby");
        }
    }
}
