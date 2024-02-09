namespace LobbyCompatibility.Enums;

/// <summary>
///     Specifies what PluginDiff types to show in the ModListPanel.
/// </summary>
public enum ModListFilter
{
    /// <summary>
    ///     ModList shows every plugin, regardless of its PluginDiffResult.
    /// </summary>
    All,

    /// <summary>
    ///     ModList only shows plugins with the following PluginDiffResult:
    ///     Compatible.
    /// </summary>
    Compatible,

    /// <summary>
    ///     ModList only shows plugins with the following PluginDiffResult:
    ///     ServerMissingMod, ClientMissingMod, ModVersionMismatch.
    /// </summary>
    Incompatible,

    /// <summary>
    ///     ModList only shows plugins with the following PluginDiffResult:
    ///     Unknown.
    /// </summary>
    Unknown
}