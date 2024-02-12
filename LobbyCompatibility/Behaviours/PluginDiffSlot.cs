using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Slot used to show a plugindiff's compatibility status in a <see cref="ModListPanel"/>.
/// </summary>
internal class PluginDiffSlot : MonoBehaviour
{
    [field: SerializeField]
    public TextMeshProUGUI? PluginNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ClientVersionText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ServerVersionText { get; private set; }

    /// <summary>
    ///     Set up the slot using existing text objects.
    /// </summary>
    /// <param name="pluginNameText"> Text to use to display the plugin's name. </param>
    /// <param name="clientVersionText"> Text to use to display the client's plugin version. </param>
    /// <param name="serverVersionText"> Text to use to display the server's plugin version. </param>
    public void SetupText(TextMeshProUGUI pluginNameText, TextMeshProUGUI? clientVersionText = null, TextMeshProUGUI? serverVersionText = null)
    {
        PluginNameText = pluginNameText;
        ClientVersionText = clientVersionText;
        ServerVersionText = serverVersionText;

        // Make sure text is enabled and setup properly
        PluginNameText.gameObject.SetActive(true);
        ClientVersionText?.gameObject.SetActive(true);
        ServerVersionText?.gameObject.SetActive(true);
    }

    /// <summary>
    ///     Set the slot's text based on a <see cref="PluginDiff"/>.
    /// </summary>
    /// <param name="pluginDiff"> PluginDiff to display. </param>
    public void SetPluginDiff(PluginDiff pluginDiff)
    {
        if (PluginNameText == null)
            return;

        PluginNameText.text = pluginDiff.GUID;

        // Decide which "Missing" string to use when a version doesn't exist
        // TODO: Something less scary than "X" for compatible mods, but something that still gets the point across
        // TODO: Once we make the unknown lobby changes, all mods should show a "?" for unknown lobbies, because we don't know if they have it or not
        string missingText;
        if (pluginDiff.PluginDiffResult == PluginDiffResult.Unknown)
            missingText = "?";
        else
            missingText = "X";

        SetTextColor(pluginDiff.GetTextColor());

        // Check for these after setting the guid, because we still want to support PluginDiffSlots without version text (for the hover menu)
        if (ClientVersionText == null || ServerVersionText == null)
            return;

        ClientVersionText.text = pluginDiff.Version?.ToString() ?? missingText;
        ServerVersionText.text = pluginDiff.RequiredVersion?.ToString() ?? missingText;
    }

    /// <summary>
    ///     Manually set the slot's text.
    /// </summary>
    /// <param name="pluginNameText"> The plugin's name to display. </param>
    /// <param name="clientVersionText"> The client's plugin version to display. </param>
    /// <param name="serverVersionText"> The server's plugin version to display. </param>
    /// <param name="color"> Color to display. </param>
    public void SetText(string pluginNameText, string clientVersionText, string serverVersionText, Color color)
    {
        if (PluginNameText == null)
            return;

        PluginNameText.text = pluginNameText;
        SetTextColor(color);

        if (ClientVersionText == null || ServerVersionText == null)
            return;

        ClientVersionText.text = clientVersionText;
        ServerVersionText.text = serverVersionText;
    }

    /// <summary>
    ///     Set the slot's text color.
    /// </summary>
    /// <param name="color"> Color to display. </param>
    private void SetTextColor(Color color)
    {
        if (PluginNameText == null)
            return;

        PluginNameText.color = color;

        if (ClientVersionText == null || ServerVersionText == null)
            return;

        ClientVersionText.color = color;
        ServerVersionText.color = color;
    }
}
