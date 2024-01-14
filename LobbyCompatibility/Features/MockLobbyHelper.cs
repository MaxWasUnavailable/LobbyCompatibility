using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

namespace LobbyCompatibility.Features
{
    /// <summary>
    ///     Helper class for mocking lobby related functions.
    /// </summary>
    internal static class MockLobbyHelper
    {
        // enum GetValues but cached
        private static readonly List<CompatibilityResult> PossibleResults = new List<CompatibilityResult>()
        {
            CompatibilityResult.Compatible,
            CompatibilityResult.ServerMissingMod,
            CompatibilityResult.ClientMissingMod,
            CompatibilityResult.ServerModOutdated,
            CompatibilityResult.ClientModOutdated,
        };

        // for mock name generation
        // giving myself a pat on the back, this name generation is pretty funny
        private static readonly List<string> ModPrefixes = new List<string>()
        {
            "LateGame",
            "More",
            "Bigger",
            "Lethal",
            "EvenMore",
            "TooMany",
            "Ship",
            "Reserved",
            "Helmet",
            "Input",
            "Yippee",
            "HD",
            "Additional",
            "Employee",
            "Custom",
            "Better",
            "Weather",
            "Hotbar",
            "Brutal",
            "Goku",
            "CoilHead",
            "SCP"
        };

        private static readonly List<string> ModSuffixes = new List<string>()
        {
            "Upgrades",
            "Lobby",
            "Company",
            "Scrap",
            "Lib",
            "Things",
            "Cosmetics",
            "Boombox",
            "API",
            "_API",
            "Core",
            "Walkers",
            "Emotes",
            "Suits",
            "Mimics",
            "Loot",
            "Utils",
            "Sounds",
            "Controller",
            "Expansion",
            "ItemDropship",
            "Posters",
            "Shotgun",
            "Bracken",
            "Screams",
            "FOV",
            "Teleporter",
            "Progression",
            "TV"
        };

        // Hardcode-y mock data for testing all other features
        public static LobbyDiff GetDiffFromLobby(Lobby lobby)
        {
            // roll a small chance to massively up the amount of mods in a lobby
            bool extraModLobby = Random.Range(0, 4) == 0;
            var modCount = Random.Range(extraModLobby ? 20 : 0, extraModLobby ? 100 : 10);

            var plugins = new List<PluginDiff>();
            for (int i = 0; i < modCount; i++)
            {
                plugins.Add(GenerateRandomPluginDiff());
            }

            return new(plugins, lobby);
        }

        private static PluginDiff GenerateRandomPluginDiff()
        {
            int resultType = Random.Range(0, 5);
            var result = PossibleResults[resultType];

            // 50% chance to be required
            bool required = Random.Range(0, 2) == 0;

            // random version
            var version = new Version(Random.Range(1, 4), Random.Range(0, 10), Random.Range(0, 10));
            Version? requiredVersion = null;
            
            // get random required version for version-based conflict
            if (resultType == 3)
            {
                // server mod needs to be older
                requiredVersion = new Version(version.Major - 1, version.Minor, version.Build);
                required = true;
            }
            else if (resultType == 4)
            {
                // server mod needs to be newer
                requiredVersion = new Version(version.Major, version.Minor + Random.Range(0, 5), version.Build);
                required = true;
            }


            // random name
            var name = $"{ModPrefixes[Random.Range(0, ModPrefixes.Count)]}{ModSuffixes[Random.Range(0, ModSuffixes.Count)]}";

            return new PluginDiff(result, required, name, version, requiredVersion);
        }

        // i am very tired. this is good enough for now.
        // not 100% sure on the name of some categories, like CompatibilityResult.ServerMissingMod with required = false
        // TODO: Short versions for hover list
        public static string GetCompatibilityCategoryName(CompatibilityResult compatibilityResult, bool required)
        {
            if (compatibilityResult == CompatibilityResult.Compatible)
            {
                return "Compatible";
            }
            else if (compatibilityResult == CompatibilityResult.ClientMissingMod)
            {
                if (required)
                {
                    return "Missing Required Mods";
                }
                else
                {
                    return "Missing Optional Mods"; // ???
                }
            }
            else if (compatibilityResult == CompatibilityResult.ServerMissingMod)
            {
                if (required)
                {
                    return "Incompatible With Server";
                }
                else
                {
                    return "Optional Mods Not On Server"; // ???
                }
            }
            else if (compatibilityResult == CompatibilityResult.ServerModOutdated)
            {
                return "Outdated On Server"; // ???
            }
            else if (compatibilityResult == CompatibilityResult.ClientModOutdated)
            {
                return "Mod Updates Required"; // ???
            }

            return "Unknown";
        }
    }
}
