using LobbyCompatibility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyCompatibility.Models
{
    // just my temporarily implementation of how a diff *could* look 
    // done to get UI logic in place
    // should be easy to swap out with a new/better impl if needed
    public class PluginDiff
    {
        public CompatibilityResult CompatibilityResult { get; }
        public bool Required { get; }
        public string Name { get; }
        public Version Version { get; }
        public Version? RequiredVersion { get; } // only applicable when ServerModOutdated / ClientModOutdated
        public string NameAndVersion => $"{Name}-{Version}";

        // Used for compatibility colors in modlist UI
        public Color TextColor
        {
            get
            {
                // Nice and bright green if we're compatible
                if (CompatibilityResult == CompatibilityResult.Compatible)
                    return Color.green;

                // Red if we're required and not compatible
                if (Required)
                    return Color.red;

                // Gray if it's not required, but also not compatible
                return Color.gray;
            }
        }

        // Display a "Need (version)" prompt for version conflicts in proper full modlist
        public string DisplayName 
        {
            get
            {
                var name = $"{Name}-{Version}";

                // Add the required version to version-based conflicts
                if ((CompatibilityResult == CompatibilityResult.ServerModOutdated || CompatibilityResult == CompatibilityResult.ClientModOutdated) && RequiredVersion != null)
                {
                    name += $" (Need {RequiredVersion})";
                }
                return name;
            }
        }

        public PluginDiff(CompatibilityResult compatibilityResult, bool required, string name, Version version, Version? requiredVersion = null) 
        {
            CompatibilityResult = compatibilityResult;
            Required = required;
            Name = name;
            Version = version;
            RequiredVersion = requiredVersion;
        }
    }
}
