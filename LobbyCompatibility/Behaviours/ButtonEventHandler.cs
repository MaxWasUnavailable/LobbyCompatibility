using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LobbyCompatibility.Behaviours
{
    // Extends UnityEngine.Button events. Needed to run custom code on hover
    // Instead of opting to use button.onClick events, we might as well just use IPointerClickHandler since we're here already.
    public class ButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public event Action<bool>? OnHoverStateChanged;
        public event Action? OnClick;

        private Image? _image;
        private Sprite? _normalSprite;
        private Sprite? _highlightedSprite;
        private Color? _normalColor;
        private Color? _highlightedColor;

        public void SetButtonImageData(Image image, Sprite normalSprite, Sprite highlightedSprite, Color normalColor, Color highlightedColor)
        {
            _image = image;
            _normalSprite = normalSprite;
            _highlightedSprite = highlightedSprite;
            _normalColor = normalColor;
            _highlightedColor = highlightedColor;

            SetVisuals(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverStateChanged?.Invoke(false);
            SetVisuals(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverStateChanged?.Invoke(true);
            SetVisuals(true);
        }

        // If button is highlighted, set the image to the highlighted sprite/color. Otherwise use the normal sprite/color
        private void SetVisuals(bool highlighted)
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
}
