using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using TMPro;
using UnityEngine;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Slot used to show a <see cref="PluginDiffResult"/> header in a <see cref="ModListPanel"/>.
/// </summary>
internal class PluginCategorySlot : MonoBehaviour
{
    [field: SerializeField]
    public TextMeshProUGUI? CategoryNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ClientVersionCategoryNameText { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI? ServerVersionCategoryNameText { get; private set; }

    /// <summary>
    ///     Set up the slot using existing text objects.
    /// </summary>
    /// <param name="categoryNameText"> Text to use to display the category's name. </param>
    /// <param name="clientVersionCategoryNameText"> Text to use to display the category's client plugin version header. </param>
    /// <param name="serverVersionCategoryNameText"> Text to use to display the category's server plugin version header. </param>
    public void SetupText(TextMeshProUGUI categoryNameText, TextMeshProUGUI? clientVersionCategoryNameText = null, TextMeshProUGUI? serverVersionCategoryNameText = null)
    {
        CategoryNameText = categoryNameText;
        ClientVersionCategoryNameText = clientVersionCategoryNameText;
        ServerVersionCategoryNameText = serverVersionCategoryNameText;

        // Make sure text is enabled and setup properly
        categoryNameText.gameObject.SetActive(true);
    }

    /// <summary>
    ///     Set the slot's text based on a <see cref="PluginDiffResult"/>.
    /// </summary>
    /// <param name="pluginDiffResult"> PluginDiffResult to display. </param>
    public void SetPluginDiffResult(PluginDiffResult pluginDiffResult)
    {
        if (CategoryNameText == null)
            return;

        CategoryNameText.text = LobbyHelper.GetCompatibilityHeader(pluginDiffResult);

        SetText(CategoryNameText.text);
    }

    /// <summary>
    ///     Manually set the slot's text.
    /// </summary>
    /// <param name="text"> The category header to display. </param>
    private void SetText(string text)
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