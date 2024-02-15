using LobbyCompatibility.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Mod list tab used to change the filtering of a <see cref="ModListPanel"/>.
/// </summary>
public class ModListTab : MonoBehaviour
{
    public ModListFilter ModListFilter;
    private Image? _tabBackground;
    private Image? _tabOutline;
    private Button? _button;
    private TextMeshProUGUI? _buttonText;
    private Color _selectedColor;
    private Color _unselectedColor;

    /// <summary>
    ///     Set up our tab based on precreated UI elements.
    /// </summary>
    /// <param name="tabBackground"> The image to use as the tab background. </param>
    /// <param name="tabOutline"> The image to use as the tab background's outline. </param>
    /// <param name="button"> The button to use on the tab. </param>
    /// <param name="buttonText"> The button's text. </param>
    /// <param name="modListFilter"> The <see cref="ModListFilter"/> to apply when clicking the tab. </param>
    /// <param name="selectedColor"> Color to use when the tab is selected. </param>
    /// <param name="unselectedColor"> Color to use when the tab is not selected. </param>
    public void Setup(Image tabBackground, Image tabOutline, Button button, TextMeshProUGUI buttonText, ModListFilter modListFilter, Color selectedColor, Color unselectedColor)
    {
        ModListFilter = modListFilter;
        _tabBackground = tabBackground;
        _tabOutline = tabOutline;
        _button = button;
        _buttonText = buttonText;
        _selectedColor = selectedColor;
        _unselectedColor = unselectedColor;

        // Need to use TMP rich text to set size, because buttons have a custom style that cannot be overridden with normal text properties
        _buttonText.text = "<size=13px><cspace=-0.04em>" + modListFilter.ToString();
    }

    /// <summary>
    ///     Set up events that will be called when the tab is clicked.
    /// </summary>
    /// <param name="action"> The action that will be called when the tab is clicked. </param>
    public void SetupEvents(Action<ModListFilter> action)
    {
        if (_button == null)
            return;

        // Clear original onClick events
        _button.onClick.m_PersistentCalls.Clear();
        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(() => action(ModListFilter));
    }

    /// <summary>
    ///     Set if the tab visuals should appear selected.
    /// </summary>
    /// <param name="active"> Whether or not the tab should appear selected. </param>
    public void SetSelectionStatus(bool selected)
    {
        if (_tabBackground == null || _tabOutline == null)
            return;

        _tabBackground.color = selected ? _selectedColor : _unselectedColor;
        _tabOutline.color = new Color(_tabOutline.color.r, _tabOutline.color.g, _tabOutline.color.b, selected ? 1f : 0.3f);
    }
}
