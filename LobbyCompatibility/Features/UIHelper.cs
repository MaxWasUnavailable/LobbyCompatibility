using System;
using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;
using Object = UnityEngine.Object;

namespace LobbyCompatibility.Features;

/// <summary>
///     Helper class for UI-related functions.
/// </summary>
internal static class UIHelper
{
    /// <summary>
    ///     Multiplies a <see cref="Transform" />'s sizeDelta as a <see cref="RectTransform" />. Returns false if transform is
    ///     invalid.
    /// </summary>
    /// <param name="transform"> The <see cref="Transform" /> to modify. </param>
    /// <param name="multiplier"> Amount to multiply the sizeDelta by. </param>
    public static bool TryMultiplySizeDelta(Transform? transform, Vector2 multiplier)
    {
        if (transform == null)
            return false;

        var rectTransform = transform.GetComponent<RectTransform>();
        if (rectTransform == null)
            return false;

        MultiplySizeDelta(rectTransform, multiplier);
        return true;
    }

    /// <summary>
    ///     Multiplies a <see cref="RectTransform" />'s sizeDelta.
    /// </summary>
    /// <param name="rectTransform"> The <see cref="RectTransform" /> to modify. </param>
    /// <param name="multiplier"> Amount to multiply the sizeDelta by. </param>
    private static void MultiplySizeDelta(RectTransform rectTransform, Vector2 multiplier)
    {
        var sizeDelta = rectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(sizeDelta.x * multiplier.x, sizeDelta.y * multiplier.y);
    }

    /// <summary>
    ///     Adds a Vector2 to a <see cref="RectTransform" />'s anchoredPosition.
    ///     Automatically gets (and null checks) the RectTransform from the <see cref="Transform" />
    /// </summary>
    /// <param name="transform"> The <see cref="Transform" /> to modify. </param>
    /// <param name="addition"> Amount to add to the <see cref="RectTransform" />'s anchoredPosition. </param>
    public static void AddToAnchoredPosition(Transform? transform, Vector2 addition)
    {
        if (transform == null)
            return;

        // First try to parse the Transform as a rectTransform. If we fail, use GetComponent
        // If we don't have a RectTransform at that point, it's safe to say this is not a UI object, and we can cancel.
        if (transform is not RectTransform rectTransform)
            rectTransform = transform.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + addition.x, rectTransform.anchoredPosition.y + addition.y);
    }

    /// <summary>
    ///     Creates a new <see cref="TextMeshProUGUI" /> to be used as a template. Intended to be used as a modlist template.
    /// </summary>
    /// <param name="template"> The <see cref="TextMeshProUGUI" /> to base the template off of. </param>
    /// <param name="color"> Text color. </param>
    /// <param name="size"> The sizeDelta of the text's RectTransform. </param>
    /// <param name="maxFontSize"> The font's maximum size. </param>
    /// <param name="minFontSize"> The font's minimum size. </param>
    /// <param name="alignment"> How to align the text. </param>
    public static TextMeshProUGUI SetupTextAsTemplate(TextMeshProUGUI template, Color color, Vector2 size,
        float maxFontSize, float minFontSize, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
    {
        var text = Object.Instantiate(template, template.transform.parent);

        // Set text alignment / formatting options
        text.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        text.rectTransform.sizeDelta = size;
        text.horizontalAlignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMax = maxFontSize;
        text.fontSizeMin = minFontSize;
        text.enableWordWrapping = false;
        text.color = color;

        // Deactivate so we can use as a template later
        text.gameObject.SetActive(false);
        return text;
    }

    /// <summary>
    ///     Creates a new <see cref="TextMeshProUGUI" /> from a template. Intended to be used with the modlist.
    /// </summary>
    /// <param name="template"> The <see cref="TextMeshProUGUI" /> to use as a template. </param>
    /// <param name="content"> The text's content. </param>
    /// <param name="yPosition"> Sets the text's <see cref="RectTransform.anchoredPosition.y" />. </param>
    /// <param name="overrideColor"> A color to override the template color with. </param>
    public static TextMeshProUGUI CreateTextFromTemplate(TextMeshProUGUI template, string content, float yPosition,
        Color? overrideColor = null)
    {
        var text = Object.Instantiate(template, template.transform.parent);

        if (overrideColor != null)
            text.color = overrideColor!.Value;

        text.rectTransform.anchoredPosition = new Vector2(0f, yPosition);
        text.text = content;
        text.gameObject.SetActive(true);

        return text;
    }

    /// <summary>
    ///     Creates a list of <see cref="TextMeshProUGUI" /> from a <see cref="LobbyDiff" />'s plugins
    /// </summary>
    /// <param name="lobbyDiff"> The <see cref="LobbyDiff" /> to generate text from. </param>
    /// <param name="textTemplate"> The <see cref="TextMeshProUGUI" /> to use for the mod text. </param>
    /// <param name="headerTextTemplate"> The <see cref="TextMeshProUGUI" /> to use for the category header text. </param>
    /// <param name="textSpacing"> The amount of spacing around mod text. </param>
    /// <param name="headerSpacing"> The amount of padding around category header text. </param>
    /// <param name="startPadding"> The amount of padding to start with. </param>
    /// <param name="compactText"> Whether to remove versions from mod names. </param>
    /// <param name="maxLines"> The maximum amount of total text lines to generate. </param>
    /// <returns> The <see cref="TextMeshProUGUI" /> list, the total UI length, and the amount of plugins used in generation. </returns>
    public static (List<TextMeshProUGUI>, float, int) GenerateTextFromDiff(
        LobbyDiff lobbyDiff,
        TextMeshProUGUI textTemplate,
        TextMeshProUGUI headerTextTemplate,
        float textSpacing,
        float headerSpacing,
        float? startPadding = null,
        bool compactText = false,
        int? maxLines = null)
    {
        // TODO: Replace with pooling if we need the performance from rapid scrolling
        // TODO: Replace this return type with an actual type lol. Need to refactor this a bit

        List<TextMeshProUGUI> generatedText = new();
        var padding = startPadding ?? 0f;
        var lines = 0;
        var pluginLines = 0;

        foreach (var compatibilityResult in Enum.GetValues(typeof(PluginDiffResult)).Cast<PluginDiffResult>())
        {
            var plugins = lobbyDiff.PluginDiffs.Where(
                pluginDiff => pluginDiff.PluginDiffResult == compatibilityResult).ToList();

            if (plugins.Count == 0)
                continue;

            // adds a few units of extra header padding
            padding += headerSpacing - textSpacing;

            // end linecount sooner if we're about to create a header - no point in showing a blank header
            if (maxLines != null && lines > maxLines - 1)
                break;

            // Create the category header
            var headerText = CreateTextFromTemplate(headerTextTemplate,
                LobbyHelper.GetCompatibilityHeader(compatibilityResult) + ":", -padding);

            generatedText.Add(headerText);
            padding += headerSpacing;
            lines++;

            // Add each plugin
            foreach (var plugin in plugins)
            {
                if (lines > maxLines)
                    break;

                var modText = CreateTextFromTemplate(textTemplate, compactText ? plugin.GUID : plugin.GetDisplayText(),
                    -padding, plugin.GetTextColor());
                modText.richText = false; // Disable rich text to avoid people injecting weird text into the modlist
                generatedText.Add(modText);
                padding += textSpacing;
                lines++;
                pluginLines++;
            }
        }

        return (generatedText, padding, pluginLines);
    }

    /// <summary>
    ///     Reskins the lobby list's "[Refresh]" button to use an image instead of text.
    ///     Used to increase the amount of usable space for the filter dropdowns.
    /// </summary>
    /// <param name="refreshButton"> The <see cref="Button" /> the game uses to refresh the lobby list. </param>
    public static void ReskinRefreshButton(Button refreshButton)
    {
        // Setup necessary references for modifying the refresh button
        var text = refreshButton?.transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
        var selectionHighlightTransform = refreshButton?.transform.Find("SelectionHighlight")?.GetComponent<RectTransform>();
        var selectionHighlightImage = selectionHighlightTransform?.GetComponent<Image>();

        if (refreshButton == null || text == null || selectionHighlightTransform == null || selectionHighlightImage == null)
            return;

        // Set new positioning
        selectionHighlightTransform.anchoredPosition = new Vector2(140f, 17.5f);
        selectionHighlightTransform.sizeDelta = new Vector2(34, 43.5f);

        // Create a new image to use instead of the [Refresh] text
        var buttonImageTransform = Object.Instantiate(selectionHighlightTransform, text.transform.parent, false);
        var buttonImage = buttonImageTransform.GetComponent<Image>();
        buttonImageTransform.SetSiblingIndex(0);

        // Change new image color to opaque
        var color = buttonImage.color;
        buttonImage.color = new Color(color.r, color.g, color.b, 1);

        // Setup sprites on images
        buttonImage.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.Refresh.png");
        selectionHighlightTransform.GetComponent<Image>().sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.InvertedRefresh.png");

        // Disable text so we can use our new image for the click/hover hitbox
        text.enabled = false;
    }

    /// <summary>
    ///     Adds a custom dropdown filter to the LobbyList top panel.
    ///     Also shifts all other filter UI elements to the left.
    /// </summary>
    /// <param name="listPanel"> The <see cref="Transform" /> that contains the UI elements for the LobbyList's filtering panel. </param>
    /// <param name="adjustment"> How many units (<see cref="RectTransform.anchoredPosition"/>) to shift all other filter UI elements. </param>
    public static void AddCustomFilterToLobbyList(Transform listPanel, float adjustment = -40f)
    {
        // Setup necessary references for modifying the lobbylist's positioning and creating a dropdown
        var dropdown = listPanel.Find("Dropdown");
        var toggleChallengeSort = listPanel.Find("ToggleChallengeSort");
        var serverTagInputField = listPanel.Find("ServerTagInputField")?.GetComponent<RectTransform>();
        var serverTagPlaceholderText = serverTagInputField?.Find("Text Area/Placeholder")?.GetComponent<TextMeshProUGUI>();

        if (dropdown == null || toggleChallengeSort == null || serverTagInputField == null || serverTagPlaceholderText == null)
            return;

        // Resize sorting/filtering objects to make room for our new dropdown
        AddToAnchoredPosition(dropdown, new Vector2(adjustment, 0f));
        AddToAnchoredPosition(toggleChallengeSort, new Vector2(adjustment, 0f));

        // Make "Server tag" input box smaller
        serverTagInputField.offsetMax = new Vector2(serverTagInputField.offsetMax.x + adjustment, serverTagInputField.offsetMax.y);

        // Replace placeholder text to be more compact
        serverTagPlaceholderText.text = "Server tag...";

        // Initalize our custom dropdown
        var customDropdownTransform = UnityEngine.Object.Instantiate(dropdown, dropdown.parent, false);
        var customDropdown = customDropdownTransform.GetComponent<TMP_Dropdown>();
        var customDropdownRect = customDropdownTransform.GetComponent<RectTransform>();

        customDropdownRect.anchoredPosition = new Vector2(adjustment, customDropdownRect.anchoredPosition.y);
        customDropdown.captionText.fontSize = 10f;
        customDropdown.itemText.fontSize = 10f;

        // Set custom dropdown options
        customDropdown.ClearOptions();
        customDropdown.AddOptions(new List<OptionData>()
        {
            new OptionData("Mods: Compatible first"),
            new OptionData("Mods: Compatible only"),
            new OptionData("Mods: All"),
        });
    }
}