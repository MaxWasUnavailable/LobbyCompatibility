using BepInEx;
using LobbyCompatibility.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LobbyCompatibility.Features
{

    /// <summary>
    ///     Helper class for plugin related functions.
    /// </summary>
    internal static class UIHelper
    {
        /// <summary>
        ///     Multiplies a <see cref="Transform" />'s sizeDelta as a <see cref="RectTransform" />. Returns false if transform is invalid.
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
        public static void MultiplySizeDelta(RectTransform rectTransform, Vector2 multiplier)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * multiplier.x, rectTransform.sizeDelta.y * multiplier.y);
        }
    }
}
