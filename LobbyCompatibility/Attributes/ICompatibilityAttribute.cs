using System;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Attributes;

/// <summary>
///     The Compatibility Attribute interface.
/// </summary>
internal interface ICompatibilityAttribute
{
    /// <summary>
    ///     Gets the compatibility level.
    /// </summary>
    /// <value>
    ///     The compatibility level.
    /// </value>
    public CompatibilityLevel CompatibilityLevel { get; }

    /// <summary>
    ///     Gets the version strictness.
    /// </summary>
    /// <value>
    ///     The version strictness.
    /// </value>
    public VersionStrictness VersionStrictness { get; }
}