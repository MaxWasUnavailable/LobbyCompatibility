using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyCompatibility.Enums
{
    // Idk what to name this but I need to get the logic in for UI
    // Feel free to change this around
    // Should probably be made internal to avoid confusion
    public enum CompatibilityResult
    {
        Compatible,
        ServerMissingMod,
        ClientMissingMod,
        ServerModOutdated,
        ClientModOutdated,
    }
}
