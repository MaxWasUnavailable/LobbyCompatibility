namespace LobbyCompatibility.Enums;

/// <summary>
///     The result of a lobby diff.
///     Indicates for a comparison between a server and client mod list, what the result of the comparison is.
/// </summary>
public enum LobbyDiffResult
{
    /// <summary>
    ///     Mod list contains no conflicts, and lobby should be fully compatible.
    /// </summary>
    Compatible,

    /// <summary>
    ///     Mod list has explicit conflicts, and lobby is not compatible.
    /// </summary>
    Incompatible,

    /// <summary>
    ///     Mod list is compatible, except for unknown mods.
    /// </summary>
    PresumedCompatible,

    /// <summary>
    ///     Mod list information is not available, server does not have LobbyCompatibility installed, or other unknown cause.
    ///     Also applies to vanilla lobbies.
    /// </summary>
    Unknown
}