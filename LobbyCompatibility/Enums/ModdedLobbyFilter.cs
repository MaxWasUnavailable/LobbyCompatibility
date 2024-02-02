namespace LobbyCompatibility.Enums;

/// <summary>
///     Specifies what type of modded lobbies to search for when looking for a public lobby.
/// </summary>
public enum ModdedLobbyFilter
{
    /// <summary>
    ///     Uses the hashfilter (if applicable) to show compatible lobbies first.
    ///     Default value.
    /// </summary>
    CompatibleFirst,

    /// <summary>
    ///     Only shows explicitly compatible lobbies.
    ///     Uses the hashfilter.
    /// </summary>
    CompatibleOnly,


    /// <summary>
    ///     Do not show any lobbies that register as modded.
    ///     Does not use the hashfilter.
    ///     NOTE: *Could* contain modded lobbies without LobbyCompatibility installed.
    /// </summary>
    UnmoddedOnly,

    /// <summary>
    ///     Shows all modded lobbies, regardless of compatibility state.
    ///     Does not use the hashfilter.
    /// </summary>
    All,
}