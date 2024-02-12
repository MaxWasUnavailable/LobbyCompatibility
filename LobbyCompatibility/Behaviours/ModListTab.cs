using LobbyCompatibility.Enums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyCompatibility.Behaviours;

public class ModListTab : MonoBehaviour
{
    public ModListFilter ModListFilter;
    private Image? _tabBackground;
    private Image? _tabOutline;
    private Button? _button;
    private TextMeshProUGUI? _buttonText;
    private Color _selectedColor;
    private Color _unselectedColor;

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

    public void SetupEvents(Action<ModListFilter> action)
    {
        if (_button == null)
            return;

        // Clear original onClick events
        _button.onClick.m_PersistentCalls.Clear();
        _button.onClick.RemoveAllListeners();

        _button.onClick.AddListener(() => action(ModListFilter));
    }

    public void SetSelectionStatus(bool selected)
    {
        if (_tabBackground == null || _tabOutline == null)
            return;

        _tabBackground.color = selected ? _selectedColor : _unselectedColor;
        _tabOutline.color = new Color(_tabOutline.color.r, _tabOutline.color.g, _tabOutline.color.b, selected ? 1f : 0.3f);
    }
}
