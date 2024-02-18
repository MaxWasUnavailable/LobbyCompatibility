using System;
using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Behaviours;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using LobbyCompatibility.Pooling;
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
    /// <param name="parent"> The <see cref="Transform" /> to parent the template to. </param>
    /// <param name="color"> Text color. </param>
    /// <param name="size"> The sizeDelta of the text's RectTransform. </param>
    /// <param name="maxFontSize"> The font's maximum size. </param>
    /// <param name="minFontSize"> The font's minimum size. </param>
    /// <param name="alignment"> How to align the text. </param>
    /// <param name="defaultPosition"> The default anchoredPosition of the text's RectTransform. </param>
    public static TextMeshProUGUI SetupTextAsTemplate(TextMeshProUGUI template, Transform parent, Color color, Vector2 size,
        float maxFontSize, float minFontSize, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center, Vector2? defaultPosition = null)
    {
        var text = Object.Instantiate(template, parent);

        // Set text alignment / formatting options
        text.rectTransform.anchoredPosition = defaultPosition ?? new Vector2(0f, 0f);
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
    ///     Apply the full size of a <see cref="RectTransform"/>'s parent.
    ///     This will realign the RectTransform's size, and set the anchor to the top left.
    /// </summary>
    /// <param name="uiElement"> The <see cref="RectTransform" />'s gameObject to resize. </param>
    /// <param name="parent"> The parent to base the size off of. </param>
    /// <returns> The resized <see cref="RectTransform"/>. </returns>
    public static RectTransform ApplyParentSize(GameObject uiElement, Transform parent)
    {
        var rect = uiElement.GetComponent<RectTransform>();
        if (rect == null) 
            rect = uiElement.AddComponent<RectTransform>();

        rect.SetParent(parent);

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;

        return rect;
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

        text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, yPosition);
        text.text = content;
        text.gameObject.SetActive(true);

        return text;
    }
    /// <summary>
    ///     Creates a list of <see cref="PluginDiffSlot" /> and <see cref="PluginCategorySlot" /> from a <see cref="LobbyDiff" />'s plugins
    /// </summary>
    /// <param name="lobbyDiff"> The <see cref="LobbyDiff" /> to generate text from. </param>
    /// <param name="pluginDiffSlotPool"> The <see cref="PluginDiffSlotPool" /> to use for spawning plugin diff slots. </param>
    /// <param name="pluginCategorySlotPool"> The <see cref="PluginCategorySlotPool" /> to use for spawning plugin category slots. </param>
    /// <param name="modListFilter"> The <see cref="ModListFilter" /> to use to decide which lobbies to filter. </param>
    /// <param name="maxLines"> The maximum amount of total text lines to generate. </param>
    /// <returns> A list of spawned <see cref="PluginDiffSlot" /> and <see cref="PluginCategorySlot" />. </returns>
    public static (List<PluginDiffSlot?> pluginDiffSlots, List<PluginCategorySlot?> pluginCategorySlots) GenerateDiffSlotsFromLobbyDiff(
        LobbyDiff lobbyDiff,
        PluginDiffSlotPool pluginDiffSlotPool, 
        PluginCategorySlotPool pluginCategorySlotPool,
        ModListFilter? modListFilter = null,
        int? maxLines = null)
    {
        var spawnedPluginDiffSlots = new List<PluginDiffSlot?>();
        var spawnedPluginCategorySlots = new List<PluginCategorySlot?>();

        // Apply ModListFilter if needed
        var filteredPlugins = modListFilter != null ? PluginHelper.FilterPluginDiffs(lobbyDiff.PluginDiffs, modListFilter!.Value) : lobbyDiff.PluginDiffs;

        int spawnedTextCount = 0;
        int spawnedPluginTextCount = 0; // used to calculate how many plugins are remaining

        // Create categories w/ mods
        foreach (var compatibilityResult in Enum.GetValues(typeof(PluginDiffResult)).Cast<PluginDiffResult>())
        {
            var plugins = filteredPlugins.Where(
                pluginDiff => pluginDiff.PluginDiffResult == compatibilityResult).ToList();

            if (plugins.Count == 0)
                continue;

            // end linecount sooner if we're about to create a header - no point in showing a blank header
            if (maxLines != null && spawnedTextCount >= maxLines - 1)
                break;

            var pluginCategorySlot = pluginCategorySlotPool.Spawn(compatibilityResult);
            spawnedPluginCategorySlots.Add(pluginCategorySlot);
            spawnedTextCount++;

            // Respawn mod diffs
            foreach (var mod in plugins)
            {
                var pluginDiffSlot = pluginDiffSlotPool.Spawn(mod, lobbyDiff.LobbyCompatibilityPresent);
                if (pluginDiffSlot == null)
                    continue;

                spawnedPluginDiffSlots.Add(pluginDiffSlot);
                spawnedTextCount++;
                spawnedPluginTextCount++;

                if (maxLines != null && spawnedTextCount >= maxLines)
                    break;
            }
        }

        // Add cutoff text if necessary
        var remainingPlugins = lobbyDiff.PluginDiffs.Count - spawnedPluginTextCount;
        if (maxLines != null && spawnedTextCount >= maxLines && remainingPlugins > 0)
        {
            var cutoffString = string.Format("{0} more mod{1}...", remainingPlugins, remainingPlugins == 1 ? "" : "s");
            var cutoffDiffSlot = pluginDiffSlotPool.Spawn(cutoffString, "", "", LobbyCompatibilityPlugin.Config?.UnknownColor.Value ?? Color.gray);
            if (cutoffDiffSlot != null)
                spawnedPluginDiffSlots.Add(cutoffDiffSlot);
        }

        return (spawnedPluginDiffSlots, spawnedPluginCategorySlots);
    }

    /// <summary>
    ///     Despawns all <see cref="PluginDiffSlot" /> and <see cref="PluginCategorySlot" /> from their assigned pools.
    /// </summary>
    /// <param name="pluginDiffSlotPool"> The <see cref="PluginDiffSlotPool" /> to use for spawning plugin diff slots. </param>
    /// <param name="pluginCategorySlotPool"> The <see cref="PluginCategorySlotPool" /> to use for spawning plugin category slots. </param>
    /// <param name="existingPluginDiffSlots"> The list of spawned <see cref="PluginDiffSlot" /> to despawn. </param>
    /// <param name="existingPluginCategorySlots"> The list of spawned <see cref="PluginCategorySlot" /> to despawn. </param>
    public static void ClearSpawnedDiffSlots(
        PluginDiffSlotPool pluginDiffSlotPool,
        PluginCategorySlotPool pluginCategorySlotPool,
        ref List<PluginDiffSlot?> existingPluginDiffSlots,
        ref List<PluginCategorySlot?> existingPluginCategorySlots)
    {
        foreach (var pluginDiffSlot in existingPluginDiffSlots)
        {
            if (pluginDiffSlot == null)
                continue;
            pluginDiffSlotPool.Release(pluginDiffSlot);
        }

        foreach (var pluginCategorySlot in existingPluginCategorySlots)
        {
            if (pluginCategorySlot == null)
                continue;
            pluginCategorySlotPool.Release(pluginCategorySlot);
        }

        existingPluginDiffSlots.Clear();
        existingPluginCategorySlots.Clear();
    }

    /// <summary>
    ///     Set up a VerticalLayoutGroup on a UI object to automatically space child objects.
    /// </summary>
    /// <param name="gameObject"> The <see cref="GameObject" /> to apply the layout group to. </param>
    /// <param name="addContentSizeFitter"> Whether or not to add a <see cref="ContentSizeFitter"/> to automatically resize the parent object. </param>
    public static void AddVerticalLayoutGroup(GameObject gameObject, bool addContentSizeFitter = true)
    {
        // Setup ContentSizeFilter and VerticalLayoutGroup so elements are automagically spaced
        var verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childControlHeight = false;
        verticalLayoutGroup.childForceExpandHeight = false;

        if (!addContentSizeFitter)
            return;

        var contentSizeFilter = verticalLayoutGroup.gameObject.AddComponent<ContentSizeFitter>();
        contentSizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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

        // Move the image below the highlight so the hover highlight will always render on top
        buttonImageTransform.SetSiblingIndex(0);

        // Set the image color to be opaque instead of the default semi-transparent
        var color = buttonImage.color;
        buttonImage.color = new Color(color.r, color.g, color.b, 1);

        // Setup sprites on images
        buttonImage.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.Refresh.png");
        selectionHighlightImage.sprite = TextureHelper.FindSpriteInAssembly("LobbyCompatibility.Resources.InvertedRefresh.png");

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

        // Resize other filtering UI options to make room for our new dropdown
        AddToAnchoredPosition(dropdown, new Vector2(adjustment, 0f));
        AddToAnchoredPosition(toggleChallengeSort, new Vector2(adjustment, 0f));

        // Make "Server tag" input box smaller
        serverTagInputField.offsetMax = new Vector2(serverTagInputField.offsetMax.x + adjustment, serverTagInputField.offsetMax.y);

        // Replace "Enter server tag..." with something more compact
        serverTagPlaceholderText.text = "Server tag...";

        // Initalize our custom dropdown
        var customDropdownTransform = Object.Instantiate(dropdown, dropdown.parent, false);
        var customDropdown = customDropdownTransform.GetComponent<TMP_Dropdown>();
        var customDropdownRect = customDropdownTransform.GetComponent<RectTransform>();

        // Move our custom dropdown to the very right side of the panel
        customDropdownRect.anchoredPosition = new Vector2(adjustment, customDropdownRect.anchoredPosition.y);
        customDropdown.captionText.fontSize = 10f;
        customDropdown.itemText.fontSize = 10f;

        // Set custom dropdown options based on Enums/ModdedLobbyFilter
        customDropdown.ClearOptions();
        customDropdown.AddOptions(new List<OptionData>()
        {
            new OptionData("Mods: Compatible first"),
            new OptionData("Mods: Compatible only"),
            new OptionData("Mods: Vanilla and Unknown only"),
            new OptionData("Mods: All"),
        });

        // Add custom component so we can actually use the filter value
        var moddedLobbyFilterDropdown = customDropdown.gameObject.AddComponent<ModdedLobbyFilterDropdown>();
        moddedLobbyFilterDropdown.SetDropdown(customDropdown);

        // Redirect the "On Value Changed" event to go towards our new custom component
        customDropdown.onValueChanged.m_PersistentCalls.Clear();
        customDropdown.onValueChanged.AddListener(moddedLobbyFilterDropdown.ChangeFilterType);
    }
}