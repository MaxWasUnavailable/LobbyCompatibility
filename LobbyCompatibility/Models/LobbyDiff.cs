using LobbyCompatibility.Enums;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyCompatibility.Models
{
    // just my temporarily implementation of how a diff *could* look 
    // done to get UI logic in place
    // should be easy to swap out with a new/better impl if needed
    public class LobbyDiff
    {
        public List<PluginDiff> PluginDiffs { get; set; }
        public Lobby Lobby { get; set; }

        public LobbyDiff(List<PluginDiff> pluginDiffs, Lobby lobby)
        {
            PluginDiffs = pluginDiffs;
            Lobby = lobby;
        }

        public ModdedLobbyType GetModdedLobbyType() 
        {  
            // unknown lobbies have zero plugins / don't hook into the mod
            // could be either vanilla or a modded user without the mod
            if (PluginDiffs.Count == 0) 
                return ModdedLobbyType.Unknown;

            // any lobby with non-required mod conflicts is incompatible
            if (PluginDiffs.Any(x => x.Required && x.CompatibilityResult != CompatibilityResult.Compatible))
                return ModdedLobbyType.Incompatible;

            return ModdedLobbyType.Compatible;
        }
    }
}
