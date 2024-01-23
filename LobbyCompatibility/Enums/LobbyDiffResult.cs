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
    ///     Mod list does not exist, and lobby might be compatible.
    ///     Also applies to vanilla lobbies.
    /// </summary>
    Unknown
}