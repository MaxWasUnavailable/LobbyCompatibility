using BepInEx;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace LobbyCompatibility.Features
{

    /// <summary>
    ///     Helper class for plugin related functions.
    /// </summary>
    internal static class UIHelper
    {
        // Kind of a stupid way to do sorted load order but it works for now
        // (CompatibilityResult, Required)
        // required = null means merge all required/nonrequired into one category
        private static List<(CompatibilityResult, bool?)> sortedCategoryLoadOrder = new List<(CompatibilityResult, bool?)>()
        {
            (CompatibilityResult.ClientMissingMod, true),
            (CompatibilityResult.ServerMissingMod, true),
            (CompatibilityResult.ClientModOutdated, true),
            (CompatibilityResult.ServerModOutdated, true),
            (CompatibilityResult.ClientMissingMod, false),
            (CompatibilityResult.ServerMissingMod, false),
            (CompatibilityResult.Compatible, null)
        };

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

        public static TextMeshProUGUI SetupTextAsTemplate(TextMeshProUGUI template, UnityEngine.Color color, Vector2 size, float maxFontSize, float minFontSize, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
        {
            var text = UnityEngine.Object.Instantiate(template, template.transform.parent);

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

        // Fairly slow but it is what it is 
        public static TextMeshProUGUI CreateTextFromTemplate(TextMeshProUGUI template, string content, float yPosition, Color? overrideColor = null)
        {
            var text = UnityEngine.Object.Instantiate(template, template.transform.parent);

            if (overrideColor != null)
                text.color = overrideColor!.Value;

            text.rectTransform.anchoredPosition = new Vector2(0f, yPosition);
            text.text = content;
            text.gameObject.SetActive(true);
            
            return text;
        }

        // generatedText, distance
        // TODO: Replace with pooling if we need the performance from rapid scrolling
        public static (List<TextMeshProUGUI>, float) GenerateTextFromDiff(
            LobbyDiff lobbyDiff, 
            TextMeshProUGUI textTemplate, 
            TextMeshProUGUI headerTextTemplate, 
            float textSpacing, 
            float headerSpacing, 
            float? startPadding = null,
            bool compactText = false)
        {
            List<TextMeshProUGUI> generatedText = new();
            float padding = startPadding ?? 0f;

            foreach (var (compatibilityResult, required) in sortedCategoryLoadOrder)
            {
                var plugins = lobbyDiff.PluginDiffs.Where(x => x.CompatibilityResult == compatibilityResult && (required == null || x.Required == required)).ToList();
                if (plugins.Count == 0)
                    continue;

                // adds a few units of extra header padding
                padding += headerSpacing - textSpacing;

                // Create the category header
                var headerText = CreateTextFromTemplate(headerTextTemplate, MockLobbyHelper.GetCompatibilityCategoryName(compatibilityResult, required ?? true), -padding);
                generatedText.Add(headerText);
                padding += headerSpacing;

                // Add each plugin
                foreach (var plugin in plugins)
                {
                    var modText = CreateTextFromTemplate(textTemplate, compactText ? plugin.Name : plugin.DisplayName, -padding, plugin.TextColor);
                    generatedText.Add(modText);

                    padding += textSpacing;
                }
            }

            return (generatedText, padding);
        }
    }
}
