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
    // Extends Button events (we can't easily do hover events with custom behaviour without this - the game uses an animation)
    // Instead of opting to use button.onClick events, we might as well just use IPointerClickHandler since we're here already.
    public class ButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Image? image;
        private Sprite? normalSprite;
        private Sprite? highlightedSprite;
        private Color? normalColor;
        private Color? highlightedColor;

        public event Action<bool>? OnHoverStateChanged;
        public event Action? OnClick;

        public void SetButtonImageData(Image image, Sprite normalSprite, Sprite highlightedSprite, Color normalColor, Color highlightedColor)
        {
            this.image = image;
            this.normalSprite = normalSprite;
            this.highlightedSprite = highlightedSprite;
            this.normalColor = normalColor;
            this.highlightedColor = highlightedColor;

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

        private void SetVisuals(bool highlighted)
        {
            if (image == null) 
                return;

            image.sprite = highlighted ? highlightedSprite : normalSprite;

            var color = highlighted ? highlightedColor : normalColor;
            if (color == null) 
                return;

            image.color = color.Value;
        }
    }
}
