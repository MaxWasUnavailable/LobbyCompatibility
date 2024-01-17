using LobbyCompatibility.Enums;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;

namespace LobbyCompatibility.Models
{
    // just my temporarily implementation of how a diff *could* look 
    // done to get UI logic in place
    // should be easy to swap out with a new/better impl if needed
    public class LobbyDiff
    {
        public List<PluginDiff> PluginDiffs { get; }
        public Lobby Lobby { get; }
        public ModdedLobbyType LobbyType { get; }
        public string LobbyCompatibilityName => LobbyType switch
        {
            ModdedLobbyType.Compatible => nameof(ModdedLobbyType.Compatible),
            ModdedLobbyType.Incompatible => nameof(ModdedLobbyType.Incompatible),
            ModdedLobbyType.Unknown => nameof(ModdedLobbyType.Unknown),
            _ => nameof(ModdedLobbyType.Unknown),
        };

        public string LobbyCompatibilityDisplayName => $"Mod Status: {LobbyCompatibilityName}";

        public LobbyDiff(List<PluginDiff> pluginDiffs, Lobby lobby)
        {
            PluginDiffs = pluginDiffs;
            Lobby = lobby;

            // assuming this lobby never changes, we can just immediately calculate the type of lobby
            LobbyType = GetModdedLobbyType();
        }

        private ModdedLobbyType GetModdedLobbyType() 
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
