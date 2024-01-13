using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyCompatibility.Features
{
    /// <summary>
    ///     Helper class for mocking lobby related functions.
    /// </summary>
    internal static class MockLobbyHelper
    {
        private static List<LobbyDiff> cachedLobbyDiffs = new();

        public static LobbyDiff GetDiffFromLobby(Lobby lobby)
        {
            return new(new(), lobby);
        }

        // Enum.GetName is slow
        public static string GetModdedLobbyText(ModdedLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case ModdedLobbyType.Compatible:
                    return "Compatible";
                case ModdedLobbyType.Incompatible:
                    return "Incompatible";
                case ModdedLobbyType.Unknown:
                default:
                    return "Unknown";
            }
        }
    }
}
