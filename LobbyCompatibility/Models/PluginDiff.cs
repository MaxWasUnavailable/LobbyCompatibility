using LobbyCompatibility.Enums;
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
    public class PluginDiff
    {
        public CompatibilityResult CompatibilityResult { get; set; }
        public bool Required { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
        public Version? RequiredVersion { get; set; } // only applicable when ServerModOutdated / ClientModOutdated

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
