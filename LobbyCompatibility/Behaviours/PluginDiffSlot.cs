using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours
{
    internal class PluginDiffSlot : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI? _pluginNameText;
        [SerializeField]
        private TextMeshProUGUI? _clientVersionText;
        [SerializeField]
        private TextMeshProUGUI? _serverVersionText;

        public void SetupText(TextMeshProUGUI pluginNameText, TextMeshProUGUI? clientVersionText, TextMeshProUGUI? serverVersionText)
        {
            _pluginNameText = pluginNameText;
            _clientVersionText = clientVersionText;
            _serverVersionText = serverVersionText;

            // Make sure text is enabled and setup properly
            _pluginNameText.gameObject.SetActive(true);
            _clientVersionText?.gameObject.SetActive(true);
            _serverVersionText?.gameObject.SetActive(true);
        }

        public void SetPluginDiff(PluginDiff pluginDiff)
        {
            if (_pluginNameText == null)
                return;

            _pluginNameText.text = pluginDiff.GUID;

            // Check for these after setting the guid, because we still want to support PluginDiffSlots without version text (for the hover menu)
            if (_clientVersionText == null || _serverVersionText == null)
                return;

            // Decide which "Missing" string to use when a version doesn't exist
            // TODO: Something less scary than "X" for compatible mods, but something that still gets the point across
            // TODO: Once we make the unknown lobby changes, all mods should show a "?" for unknown lobbies, because we don't know if they have it or not
            string missingText;
            if (pluginDiff.PluginDiffResult == PluginDiffResult.Unknown)
                missingText = "?";
            else
                missingText = "X";

            _clientVersionText.text = pluginDiff.Version?.ToString() ?? missingText;
            _serverVersionText.text = pluginDiff.RequiredVersion?.ToString() ?? missingText;

            SetTextColor(pluginDiff.GetTextColor());
        }
        
        private void SetTextColor(Color color)
        {
            if (_pluginNameText == null || _clientVersionText == null || _serverVersionText == null)
                return;

            _pluginNameText.color = color;
            _clientVersionText.color = color;
            _serverVersionText.color = color;
        }
    }
}
