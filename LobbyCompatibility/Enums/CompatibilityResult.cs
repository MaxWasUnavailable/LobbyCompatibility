// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace LobbyCompatibility.Enums;

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
