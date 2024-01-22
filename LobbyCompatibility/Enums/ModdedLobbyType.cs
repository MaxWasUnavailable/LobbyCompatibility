namespace LobbyCompatibility.Enums;

/// <summary>
///     Specifies the mod compatibility level of a lobby.
/// </summary>
public enum ModdedLobbyType
{
    /// <summary>
    ///     Mod list contains no conflicts, and lobby should be fully compatible.
    /// </summary>
    Compatible = 0,

    /// <summary>
    ///     Mod list has explicit conflicts, and lobby is not compatible.
    /// </summary>
    Incompatible = 1,

    /// <summary>
    ///     Mod list does not exist, and lobby might be compatible.
    ///     Also applies to vanilla lobbies.
    /// </summary>
    Unknown = 2
}