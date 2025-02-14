namespace LobbyCompatibility.Enums;

/// <summary>
///     Specifies the compatibility level of the plugin.
/// </summary>
public enum CompatibilityLevel
{
    /// <summary>
    ///     Mod only impacts the client.
    ///     Mod is not checked at all, VersionStrictness does not apply.
    /// </summary>
    ClientOnly,

    /// <summary>
    ///     Mod only impacts the server, and might implicitly impact the client without the client needing to have it installed
    ///     for it to work.
    ///     Mod is only required by the server. VersionStrictness only applies if the mod is installed on the client.
    /// </summary>
    ServerOnly,

    /// <summary>
    ///     Mod impacts both the client and the server, and adds functionality that requires the mod to be installed on both.
    ///     Mod must be loaded on server and client. Version checking depends on the VersionStrictness.
    /// </summary>
    Everyone,

    /// <summary>
    ///     Not every client needs to have the mod installed, but if it is installed, the server also needs to have it.
    ///     Generally used for mods that add extra (optional) functionality to the client if the server has it installed.
    ///     Mod must be loaded on server. Version checking depends on the VersionStrictness.
    /// </summary>
    ClientOptional,
    
    /// <summary>
    ///     The compatibility level changes depending on the context of the lobby.
    ///     Only very specific mods will ever use this, and it is highly discouraged to use this if you do not know what you are doing.
    ///     If a value is not returned when the <see cref="LobbyCompatibility.Features.VariableCompatibilityCheckDelegate">check</see> is called, it will default to <see cref="CompatibilityLevel.ClientOnly"/>.
    ///     Only usable for mods using the
    ///     <see cref="LobbyCompatibility.Features.PluginHelper.RegisterPlugin(string, System.Version, CompatibilityLevel, VersionStrictness, LobbyCompatibility.Features.VariableCompatibilityCheckDelegate?)">RegisterPlugin</see> method.
    /// </summary>
    Variable,
}