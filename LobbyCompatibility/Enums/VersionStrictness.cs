namespace LobbyCompatibility.Enums;

public enum VersionStrictness
{
    /// <summary>
    ///     No version check is done
    /// </summary>
    None,

    /// <summary>
    ///     Mod must have the same Major version
    /// </summary>
    Major,

    /// <summary>
    ///     Mods must have the same Minor version
    /// </summary>
    Minor,

    /// <summary>
    ///     Mods must have the same Patch version
    /// </summary>
    Patch
}