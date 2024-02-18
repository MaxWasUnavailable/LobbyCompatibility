using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LobbyCompatibility.Behaviours;

/// <summary>
///     Extends UnityEngine.Button events. Needed to run custom code on hover
///     Instead of opting to use button.onClick events, we might as well just use IPointerClickHandler since we're here
///     already.
/// </summary>
public class ButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color? _highlightedColor;
    private Sprite? _highlightedSprite;

    private Image? _image;
    private Color? _normalColor;
    private Sprite? _normalSprite;

    /// <summary>
    ///     Called when the button is clicked
    /// </summary>
    /// <param name="eventData"> Pointer event data </param>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

    /// <summary>
    ///     Called when the mouse enters the button
    /// </summary>
    /// <param name="eventData"> Pointer event data </param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverStateChanged?.Invoke(true);
        SetHighlighted(true);
    }

    /// <summary>
    ///     Called when the mouse exits the button
    /// </summary>
    /// <param name="eventData"> Pointer event data </param>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverStateChanged?.Invoke(false);
        SetHighlighted(false);
    }

    public event Action<bool>? OnHoverStateChanged;
    public event Action? OnClick;

    /// <summary>
    ///     Sets the button's image data
    /// </summary>
    /// <param name="image"> Image component </param>
    /// <param name="normalSprite"> Sprite to use when the button is not highlighted </param>
    /// <param name="highlightedSprite"> Sprite to use when the button is highlighted </param>
    /// <param name="normalColor"> Color to use when the button is not highlighted </param>
    /// <param name="highlightedColor"> Color to use when the button is highlighted </param>
    public void SetButtonImageData(Image image, Sprite normalSprite, Sprite highlightedSprite, Color normalColor,
        Color highlightedColor)
    {
        _image = image;
        _normalSprite = normalSprite;
        _highlightedSprite = highlightedSprite;

        SetColor(normalColor, highlightedColor);
        SetHighlighted(false);
    }

    /// <summary>
    ///     Sets the button's color data
    /// </summary>
    /// <param name="normalColor"> Color to use when the button is not highlighted </param>
    /// <param name="highlightedColor"> Color to use when the button is highlighted </param>
    public void SetColor(Color normalColor, Color highlightedColor)
    {
        _normalColor = normalColor;
        _highlightedColor = highlightedColor;

        SetHighlighted(false);
    }

    /// <summary>
    ///     If button is highlighted, set the image to the highlighted sprite/color. Otherwise use the normal sprite/color
    /// </summary>
    /// <param name="highlighted"> Whether the button is highlighted </param>
    private void SetHighlighted(bool highlighted)
    {
        if (_image == null)
            return;

        _image.sprite = highlighted ? _highlightedSprite : _normalSprite;

        var color = highlighted ? _highlightedColor : _normalColor;
        if (color == null)
            return;

        _image.color = color.Value;
    }
}