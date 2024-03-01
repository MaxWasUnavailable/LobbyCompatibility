using System;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Attributes;

/// <summary>
///     Specifies the compatibility of a plugin.
/// </summary>
/// <example>
///     <code>
/// [LobbyCompatibility(CompatibilityLevel.ServerOnly, VersionStrictness.Minor)]
/// class MyPlugin : BaseUnityPlugin
/// {
/// }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LobbyCompatibilityAttribute : Attribute, ICompatibilityAttribute
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

    /// <inheritdoc cref="ICompatibilityAttribute.CompatibilityLevel"/>
    public CompatibilityLevel CompatibilityLevel { get; }
    
    /// <inheritdoc cref="ICompatibilityAttribute.VersionStrictness"/>
    public VersionStrictness VersionStrictness { get; }
}