using System.Collections.Generic;
using System.Linq;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;
using LobbyCompatibility.Models;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Color = UnityEngine.Color;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using LobbyCompatibility.Pooling;
using System;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Panel used to display a tooltip for a lobby's diff on hover.
/// </summary>
public class ModListTooltipPanel : MonoBehaviour
{
    public static ModListTooltipPanel? Instance;

    // UI generation settings
    private static readonly Vector2 NotificationWidth = new(0.75f, 1.1f);
    private static readonly float HeaderSpacing = 12f;
    private static readonly float TextSpacing = 11f;
    private static readonly int MaxLines = 12;

    private List<PluginDiffSlot?> _spawnedPluginDiffSlots = new();
    private List<PluginCategorySlot?> _spawnedPluginCategorySlots = new();

    private RectTransform? _panelTransform;
    private TextMeshProUGUI? _titleText;

    private PluginDiffSlotPool? _pluginDiffSlotPool;
    private PluginCategorySlotPool? _pluginCategorySlotPool;

    private void Awake()
    {
        Instance = this;

        // Disable invisible background image (allows us to still interact with below UI elements with the hover menu open)
        var invisibleBackgroundImage = gameObject.GetComponent<Image>();
        if (invisibleBackgroundImage != null)
            invisibleBackgroundImage.enabled = false;

        // Disable background image
        var backgroundImage = transform.Find("Image")?.GetComponent<Image>();
        if (backgroundImage != null)
            backgroundImage.enabled = false;

        // Find actual alert panel so we can modify it
        var panelImage = transform.Find("Panel")?.GetComponent<Image>();
        _panelTransform = panelImage?.rectTransform;
        var panelOutlineImage = _panelTransform?.Find("Image")?.GetComponent<Image>();
        if (panelImage == null || _panelTransform == null || panelOutlineImage == null)
            return;

        // Increase panel opacity to 100% since we disabled the background image
        panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, 1);
        _panelTransform.anchorMin = new Vector2(0, 1);
        _panelTransform.anchorMax = new Vector2(0, 1);

        // Multiply panel element sizes to make the hover notification skinnier
        UIHelper.TryMultiplySizeDelta(_panelTransform, NotificationWidth);
        UIHelper.TryMultiplySizeDelta(panelOutlineImage.transform, NotificationWidth);
        UIHelper.TryMultiplySizeDelta(_panelTransform.Find("NotificationText"), NotificationWidth);

        // Remove "dismiss" button
        _panelTransform.Find("ResponseButton")?.gameObject.SetActive(false);

        // Set to screen's top left corner temporarily and disable panel
        var sizeDelta = _panelTransform.sizeDelta;
        _panelTransform.anchoredPosition = new Vector2(sizeDelta.x / 2, -sizeDelta.y / 2);
        _panelTransform.gameObject.SetActive(false);

        SetupText(_panelTransform, panelOutlineImage);
    }

    // This could/should eventually be moved to a UIHelper method if we want this to look identical to the full mod list panel
    private void SetupText(RectTransform panelTransform, Image panelOutlineImage)
    {
        _titleText = panelTransform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
        if (_titleText == null)
            return;

        // Create new image to put text inside of
        var textContainerImage = Instantiate(panelOutlineImage, panelTransform);
        textContainerImage.sprite = null;
        textContainerImage.color = Color.clear;
        textContainerImage.rectTransform.sizeDelta = panelTransform.sizeDelta;

        // Setup ContentSizeFilter and VerticalLayoutGroup so diff elements are automagically spaced
        UIHelper.AddVerticalLayoutGroup(textContainerImage.gameObject, false);

        // Setup text
        _titleText.transform.SetParent(textContainerImage.transform);
        _titleText.fontSizeMax = 13f;
        _titleText.fontSizeMin = 12f;
        _titleText.rectTransform.anchoredPosition = new Vector2(0, 95f);
        _titleText.gameObject.SetActive(false);

        // Setup PluginDiffSlot template panel
        var pluginDiffSlot = new GameObject("PluginDiffSlot");
        var pluginDiffSlotImage = pluginDiffSlot.AddComponent<Image>();
        var pluginDiffSlotTransform = UIHelper.ApplyParentSize(pluginDiffSlot, textContainerImage.transform);
        pluginDiffSlotTransform.anchoredPosition = Vector2.zero;
        pluginDiffSlotTransform.sizeDelta = new Vector2(1f, TextSpacing);
        pluginDiffSlotImage.color = Color.clear;
        pluginDiffSlot.SetActive(false);

        // Setup text as template
        var pluginNameText =
            UIHelper.SetupTextAsTemplate(_titleText, pluginDiffSlotTransform, _titleText.color, new Vector2(160f, 75f), 13f, 2f, 
            HorizontalAlignmentOptions.Left, new Vector2(0f, 7f));

        // Make the text wrap with compact line spacing
        pluginNameText.lineSpacing = -20f;

        // Finish PluginDiffSlot setup
        var diffSlot = pluginDiffSlot.AddComponent<PluginDiffSlot>();
        diffSlot.SetupText(pluginNameText);

        // Setup PluginCategorySlot template panel, identical except for the height
        var diffSlotClone = Instantiate(diffSlot, pluginDiffSlotTransform.parent);
        diffSlotClone.GetComponent<RectTransform>().sizeDelta = new Vector2(1, HeaderSpacing);
        var categorySlot = diffSlotClone.gameObject.AddComponent<PluginCategorySlot>();

        // Setup all text for PluginCategorySlot
        if (diffSlotClone.PluginNameText != null)
        {
            diffSlotClone.PluginNameText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            categorySlot.SetupText(diffSlotClone.PluginNameText);
        }

        // Remove duplicate PluginDiffSlot on PluginCategorySlot and finish setup
        Destroy(diffSlotClone);
        categorySlot.gameObject.SetActive(false);

        // Duplicate category slot to use as main "title" text
        var titleTextContainer = Instantiate(categorySlot, categorySlot.transform.parent);
        var titleTextContainerTransform = titleTextContainer.GetComponent<RectTransform>();
        titleTextContainer.gameObject.SetActive(true);
        _titleText = titleTextContainer.CategoryNameText;

        if (titleTextContainerTransform == null || titleTextContainer.CategoryNameText == null)
            return;

        // Setup new positioning for the larger title text
        titleTextContainerTransform.sizeDelta = new Vector2(1, HeaderSpacing * 4);
        titleTextContainer.CategoryNameText.rectTransform.anchoredPosition = new Vector2(0, 20f);
        titleTextContainer.CategoryNameText.horizontalAlignment = HorizontalAlignmentOptions.Left;

        // Initialize pools
        var pluginPoolsObject = new GameObject("PluginSlotPools");
        pluginPoolsObject.transform.SetParent(panelTransform);
        _pluginDiffSlotPool = pluginPoolsObject.AddComponent<PluginDiffSlotPool>();
        _pluginDiffSlotPool.InitializeUsingTemplate(diffSlot, diffSlot.transform.parent);
        _pluginCategorySlotPool = pluginPoolsObject.AddComponent<PluginCategorySlotPool>();
        _pluginCategorySlotPool.InitializeUsingTemplate(categorySlot, categorySlot.transform.parent);
    }

    public void DisplayNotification(LobbyDiff lobbyDiff, RectTransform elementTransform,
        Transform elementContainerTransform)
    {
        if (_panelTransform == null)
            return;

        // Turn relative position into something we can use
        Vector2 hoverPanelPosition = RectTransformUtility
            .CalculateRelativeRectTransformBounds(elementContainerTransform, elementTransform).center;

        // If the element is more than ~halfway down the screen, the tooltip should go upwards instead of down
        // TODO: Remove magic number
        var alignWithBottom = hoverPanelPosition.y > -190f;

        // Add the panel's width/height so it's not offset
        var sizeDelta = _panelTransform.sizeDelta;
        hoverPanelPosition += new Vector2(sizeDelta.x, alignWithBottom ? -sizeDelta.y : 0);

        // Add the button size as an additional offset so it's not in the center of the button
        hoverPanelPosition += new Vector2(elementTransform.sizeDelta.x / 2,
            alignWithBottom ? -elementTransform.sizeDelta.y / 2 : 0);

        // Add a very small amount of padding
        // TODO: Replace this with a non-magic number
        hoverPanelPosition += new Vector2(-15f, alignWithBottom ? 4f : 6f);
        _panelTransform.anchoredPosition = hoverPanelPosition;
        _panelTransform.gameObject.SetActive(true);

        if (_panelTransform == null || _titleText == null || _pluginCategorySlotPool == null || _pluginDiffSlotPool == null)
            return;


        DisplayModList(lobbyDiff);
    }

    private void DisplayModList(LobbyDiff lobbyDiff)
    {
        if (_panelTransform == null || _titleText == null || _pluginCategorySlotPool == null || _pluginDiffSlotPool == null)
            return;

        // Despawn old diffs
        UIHelper.ClearSpawnedDiffSlots(_pluginDiffSlotPool, _pluginCategorySlotPool, ref _spawnedPluginDiffSlots, ref _spawnedPluginCategorySlots);

        var incompatibleModsCount = lobbyDiff.PluginDiffs
            .Where(pluginDiff => pluginDiff.PluginDiffResult != PluginDiffResult.Compatible &&
                                 pluginDiff.PluginDiffResult != PluginDiffResult.Unknown).ToList().Count;
        var incompatibleMods = $"({incompatibleModsCount})";

        // Make the incompatible count red if there are any incompatible mods
        if (incompatibleModsCount > 0)
            incompatibleMods = $"<color=red>{incompatibleMods}</color>";

        _titleText.text =
            $"{lobbyDiff.GetDisplayText()}\nTotal Mods: ({lobbyDiff.PluginDiffs.Count})\nIncompatible Mods: {incompatibleMods}\n========================";

        // Spawn new diffslots
        (_spawnedPluginDiffSlots, _spawnedPluginCategorySlots) = UIHelper.GenerateDiffSlotsFromLobbyDiff(
            lobbyDiff, _pluginDiffSlotPool, _pluginCategorySlotPool, null, MaxLines);
    }

    public void HideNotification()
    {
        if (_panelTransform == null)
            return;

        _panelTransform.gameObject.SetActive(false);
    }
}