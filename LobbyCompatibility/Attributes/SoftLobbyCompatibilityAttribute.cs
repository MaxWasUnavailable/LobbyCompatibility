using System;
using LobbyCompatibility.Enums;

namespace LobbyCompatibility.Attributes;

/// <summary>
///     Specifies the compatibility of a plugin in a soft dependency manor.
/// </summary>
/// <example>
///     <code>
/// [assembly:SoftLobbyCompatibility(typeof(MyPlugin), CompatibilityLevel.ServerOnly, VersionStrictness.Minor)]
/// 
/// namespace ExampleMod;
///
/// public class MyPlugin : BaseUnityPlugin
/// {
/// }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class SoftLobbyCompatibilityAttribute : Attribute, ICompatibilityAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SoftLobbyCompatibilityAttribute" /> class.
    /// </summary>
    /// <param name="plugin">The type of the plugin class.</param>
    /// <param name="compatibilityLevel">The compatibility level.</param>
    /// <param name="versionStrictness">The version strictness.</param>
    public SoftLobbyCompatibilityAttribute(Type plugin,
        CompatibilityLevel compatibilityLevel,
        VersionStrictness versionStrictness)
    {
        Plugin = plugin;
        CompatibilityLevel = compatibilityLevel;
        VersionStrictness = versionStrictness;
    }
    
    /// <summary>
    ///     Gets the plugin type.
    /// </summary>
    /// <value>
    ///     The plugin type.
    /// </value>
    public Type Plugin { get; }

    /// <inheritdoc cref="ICompatibilityAttribute.CompatibilityLevel"/>
    public CompatibilityLevel CompatibilityLevel { get; }
    
    /// <inheritdoc cref="ICompatibilityAttribute.VersionStrictness"/>
    public VersionStrictness VersionStrictness { get; }
}