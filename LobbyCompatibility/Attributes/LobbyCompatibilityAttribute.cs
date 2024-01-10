using System;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Attributes;

/// <summary>
///     Specifies the compatibility of a plugin.
/// </summary>
/// <example>
///     <code>
/// [LobbyCompatibilityAttribute(CompatibilityLevel.ServerOnly, VersionStrictness.Minor)]
/// class MyPlugin : BaseUnityPlugin
/// {
/// }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LobbyCompatibilityAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LobbyCompatibilityAttribute" /> class.
    /// </summary>
    /// <param name="compatibilityLevel">The compatibility level.</param>
    /// <param name="versionStrictness">The version strictness.</param>
    public LobbyCompatibilityAttribute(CompatibilityLevel compatibilityLevel, VersionStrictness versionStrictness)
    {
        CompatibilityLevel = compatibilityLevel;
        VersionStrictness = versionStrictness;
    }

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