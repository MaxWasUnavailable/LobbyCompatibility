using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours;

internal class PluginDiffSlot : MonoBehaviour
{
    [field: SerializeField]
    public TextMeshProUGUI? PluginNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ClientVersionText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ServerVersionText { get; private set; }

    public void SetupText(TextMeshProUGUI pluginNameText, TextMeshProUGUI? clientVersionText, TextMeshProUGUI? serverVersionText)
    {
        PluginNameText = pluginNameText;
        ClientVersionText = clientVersionText;
        ServerVersionText = serverVersionText;

        // Make sure text is enabled and setup properly
        PluginNameText.gameObject.SetActive(true);
        ClientVersionText?.gameObject.SetActive(true);
        ServerVersionText?.gameObject.SetActive(true);
    }

    public void SetPluginDiff(PluginDiff pluginDiff)
    {
        if (PluginNameText == null)
            return;

        PluginNameText.text = pluginDiff.GUID;

        // Check for these after setting the guid, because we still want to support PluginDiffSlots without version text (for the hover menu)
        if (ClientVersionText == null || ServerVersionText == null)
            return;

        // Decide which "Missing" string to use when a version doesn't exist
        // TODO: Something less scary than "X" for compatible mods, but something that still gets the point across
        // TODO: Once we make the unknown lobby changes, all mods should show a "?" for unknown lobbies, because we don't know if they have it or not
        string missingText;
        if (pluginDiff.PluginDiffResult == PluginDiffResult.Unknown)
            missingText = "?";
        else
            missingText = "X";

        ClientVersionText.text = pluginDiff.Version?.ToString() ?? missingText;
        ServerVersionText.text = pluginDiff.RequiredVersion?.ToString() ?? missingText;

        SetTextColor(pluginDiff.GetTextColor());
    }
    
    private void SetTextColor(Color color)
    {
        if (PluginNameText == null || ClientVersionText == null || ServerVersionText == null)
            return;

        PluginNameText.color = color;
        ClientVersionText.color = color;
        ServerVersionText.color = color;
    }
}
