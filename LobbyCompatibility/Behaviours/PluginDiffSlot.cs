using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours
{
    internal class PluginDiffSlot : MonoBehaviour
    {

        private TextMeshProUGUI? _pluginNameText;
        private TextMeshProUGUI? _clientVersionText;
        private TextMeshProUGUI? _serverVersionText;

        public void SetupText(TextMeshProUGUI pluginNameText, TextMeshProUGUI clientVersionText, TextMeshProUGUI serverVersionText)
        {
            _pluginNameText = pluginNameText;
            _clientVersionText = clientVersionText;
            _serverVersionText = serverVersionText;

            // Make sure text is enabled and setup properly
            _pluginNameText.gameObject.SetActive(true);
            _clientVersionText.gameObject.SetActive(true);
            _serverVersionText.gameObject.SetActive(true);
        }

        public void SetPluginDiff(PluginDiff pluginDiff)
        {
            if (_pluginNameText == null || _clientVersionText == null || _serverVersionText == null)
                return;

            _pluginNameText.text = pluginDiff.GUID;

            // Decide which "Missing" string to use when a version doesn't exist
            // TODO: Something less scary than "X" for compatible mods, but something that still gets the point across
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
