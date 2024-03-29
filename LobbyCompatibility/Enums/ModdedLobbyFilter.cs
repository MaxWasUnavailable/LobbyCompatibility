﻿namespace LobbyCompatibility.Enums;

/// <summary>
///     Specifies what type of filter to apply when searching for a public lobby.
/// </summary>
public enum ModdedLobbyFilter
{
    /// <summary>
    ///     Uses the hashfilter (if applicable) to show compatible lobbies first.
    ///     Default value.
    /// </summary>
    CompatibleFirst,

    /// <summary>
    ///     Uses the hashfilter (if applicable) to only show explicitly compatible lobbies.
    /// </summary>
    CompatibleOnly,

    /// <summary>
    ///     Do not show any lobbies that register as modded.
    ///     Does not use the hashfilter.
    /// </summary>
    /// <remarks>
    ///     Could contain modded lobbies that don't have LobbyCompatibility installed.
    /// </remarks>
    VanillaAndUnknownOnly,

    /// <summary>
    ///     Shows all modded lobbies, regardless of compatibility state.
    ///     Does not use the hashfilter.
    /// </summary>
    All,
}