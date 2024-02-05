using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours;

internal class PluginCategorySlot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI? _categoryNameText;

    public void SetupText(TextMeshProUGUI categoryNameText)
    {
        _categoryNameText = categoryNameText;

        // Make sure text is enabled and setup properly
        categoryNameText.gameObject.SetActive(true);
    }

    public void SetPluginDiffResult(PluginDiffResult pluginDiffResult)
    {
        if (_categoryNameText == null)
            return;

        _categoryNameText.text = LobbyHelper.GetCompatibilityHeader(pluginDiffResult);

        SetText(_categoryNameText.text, Color.green);
    }

    // In case we want more "general" categories, instead of the detailed headers
    // This would probably be better with a switch statement for PluginDiffResult, but I don't want to mess with Unknown due to the impending rework
    public void SetLobbyDiffResult(LobbyDiffResult lobbyDiffResult)
    {
        if (_categoryNameText == null)
            return;

        _categoryNameText.text = lobbyDiffResult.ToString();

        SetText(_categoryNameText.text, Color.green);
    }

    private void SetText(string text, Color color)
    {
        if (_categoryNameText == null)
            return;

        _categoryNameText.text = text + ":";

        // TODO: Use color for a little colored icon or something?
    }
}