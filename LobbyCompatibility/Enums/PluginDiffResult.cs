﻿// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace LobbyCompatibility.Enums;

/// <summary>
///     The result of a plugin diff.
///     Indicates for a comparison between a server plugin and a client plugin, what the result of the comparison is.
/// </summary>
public enum PluginDiffResult
{
    /// <summary>
    ///     Plugins are compatible.
    /// </summary>
    Compatible,

    /// <summary>
    ///     Server is missing a mod that the client has.
    /// </summary>
    ServerMissingMod,

    /// <summary>
    ///     Client is missing a mod that the server has.
    /// </summary>
    ClientMissingMod,

    /// <summary>
    ///     Server has an outdated version of a mod that the client has.
    /// </summary>
    ServerModOutdated,

    /// <summary>
    ///     Client has an outdated version of a mod that the server has.
    /// </summary>
    ClientModOutdated
}