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
    [field: SerializeField]
    public TextMeshProUGUI? CategoryNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ClientVersionCategoryNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ServerVersionCategoryNameText { get; private set; }

    public void SetupText(TextMeshProUGUI categoryNameText, TextMeshProUGUI clientVersionCategoryNameText, TextMeshProUGUI serverVersionCategoryNameText)
    {
        CategoryNameText = categoryNameText;
        ClientVersionCategoryNameText = clientVersionCategoryNameText;
        ServerVersionCategoryNameText = serverVersionCategoryNameText;

        // Make sure text is enabled and setup properly
        categoryNameText.gameObject.SetActive(true);
    }

    public void SetPluginDiffResult(PluginDiffResult pluginDiffResult)
    {
        if (CategoryNameText == null)
            return;

        CategoryNameText.text = LobbyHelper.GetCompatibilityHeader(pluginDiffResult);

        SetText(CategoryNameText.text, Color.green);
    }

    // In case we want more "general" categories, instead of the detailed headers
    // This would probably be better with a switch statement for PluginDiffResult, but I don't want to mess with Unknown due to the impending rework
    public void SetLobbyDiffResult(LobbyDiffResult lobbyDiffResult)
    {
        if (CategoryNameText == null)
            return;

        CategoryNameText.text = lobbyDiffResult.ToString();

        SetText(CategoryNameText.text, Color.green);
    }

    private void SetText(string text, Color color)
    {
        if (CategoryNameText == null)
            return;

        CategoryNameText.text = text + ":";

        if (ClientVersionCategoryNameText == null || ServerVersionCategoryNameText == null)
            return;

        ClientVersionCategoryNameText.text = "You";
        ServerVersionCategoryNameText.text = "Lobby";

        // TODO: Use color for a little colored icon or something?
    }
}