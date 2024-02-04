// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace LobbyCompatibility.Enums;

/// <summary>
///     Metadata keys for use when setting lobby data.
/// </summary>
public static class LobbyMetadata
{
    /// <summary>
    ///     The tag for the lobby name.
    /// </summary>
    public const string Name = "name";

    /// <summary>
    ///     The tag for the host's custom lobby tag.
    /// </summary>
    public const string Tag = "tag";

    /// <summary>
    ///     The tag for the host's game version.
    /// </summary>
    /// <remarks>
    ///     Users who try to join compare against this. Mismatches cause a failure to join.
    ///     This should generally not be overridden.
    /// </remarks>
    public const string Version = "vers";

    /// <summary>
    ///     The tag for the lobby being joinable for vanilla clients.
    /// </summary>
    public const string Joinable = "joinable";

    /// <summary>
    ///     The tag for the lobby being modded.
    /// </summary>
    public const string Modded = "modded";

    /// <summary>
    ///     The tag for the lobby being joinable for modded clients.
    /// </summary>
    public const string JoinableModded = "_joinable";

    /// <summary>
    ///     The tag for plugin information.
    /// </summary>
    public const string Plugins = "plugins";

    /// <summary>
    ///     The required plugins checksum to filter against.
    /// </summary>
    public const string RequiredChecksum = "checksum";
}