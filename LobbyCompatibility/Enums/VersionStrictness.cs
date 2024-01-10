namespace LobbyCompatibility.Enums;

public enum VersionStrictness
{
    /// <summary>
    ///     No version check is done
    /// </summary>
    None = 0,

    /// <summary>
    ///     Mod must have the same Major version
    /// </summary>
    Major = 1,

    /// <summary>
    ///     Mods must have the same Minor version
    /// </summary>
    Minor = 2,

    /// <summary>
    ///     Mods must have the same Patch version
    /// </summary>
    Patch = 3
}