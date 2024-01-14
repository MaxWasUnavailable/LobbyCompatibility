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
        private static readonly List<(CompatibilityResult, bool?)> SortedCategoryLoadOrder = new List<(CompatibilityResult, bool?)>()
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

        /// <summary>
        ///     Creates a new <see cref="TextMeshProUGUI" /> to be used as a template. Intended to be used as a modlist template.
        /// </summary>
        /// <param name="template"> The <see cref="TextMeshProUGUI" /> to base the template off of. </param>
        /// <param name="color"> Text color. </param>
        /// <param name="size"> The sizeDelta of the text's RectTransform. </param>
        /// <param name="maxFontSize"> The font's maximum size. </param>
        /// <param name="minFontSize"> The font's minimum size. </param>
        /// <param name="alignment"> How to align the text. </param>
        public static TextMeshProUGUI SetupTextAsTemplate(TextMeshProUGUI template, Color color, Vector2 size, float maxFontSize, float minFontSize, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
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

        /// <summary>
        ///     Creates a new <see cref="TextMeshProUGUI" /> from a template. Intended to be used with the modlist.
        /// </summary>
        /// <param name="template"> The <see cref="TextMeshProUGUI" /> to use as a template. </param>
        /// <param name="content"> The text's content. </param>
        /// <param name="yPosition"> Sets the text's <see cref="RectTransform.anchoredPosition.y" />. </param>
        /// <param name="overrideColor"> A color to override the template color with. </param>
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

        /// <summary>
        ///     Creates a list of <see cref="TextMeshProUGUI" /> from a <see cref="LobbyDiff" />'s plugins
        /// </summary>
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
            float padding = startPadding ?? 0f;
            int lines = 0;
            int pluginLines = 0;

            foreach (var (compatibilityResult, required) in SortedCategoryLoadOrder)
            {
                var plugins = lobbyDiff.PluginDiffs.Where(x => x.CompatibilityResult == compatibilityResult && (required == null || x.Required == required)).ToList();
                if (plugins.Count == 0)
                    continue;

                // adds a few units of extra header padding
                padding += headerSpacing - textSpacing;

                if (maxLines != null && lines > maxLines - 1) // end linecount sooner if we're about to create a header - no point in showing a blank header
                    break;

                // Create the category header
                var headerText = CreateTextFromTemplate(headerTextTemplate, MockLobbyHelper.GetCompatibilityCategoryName(compatibilityResult, required ?? true) + ":", -padding);
                generatedText.Add(headerText);
                padding += headerSpacing;
                lines++;

                // Add each plugin
                foreach (var plugin in plugins)
                {
                    if (maxLines != null && lines > maxLines)
                        break;

                    var modText = CreateTextFromTemplate(textTemplate, compactText ? plugin.Name : plugin.DisplayName, -padding, plugin.TextColor);
                    generatedText.Add(modText);
                    padding += textSpacing;
                    lines++;
                    pluginLines++;
                }
            }

            return (generatedText, padding, pluginLines);
        }
    }
}
