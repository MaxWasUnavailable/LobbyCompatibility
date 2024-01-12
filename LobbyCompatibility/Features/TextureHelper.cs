using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace LobbyCompatibility.Features
{
    // https://github.com/monkeymanboy/BeatSaberMarkupLanguage/blob/8b469be2b3136ea2362ce7bb6b319ec32d573edd/BeatSaberMarkupLanguage/Utilities.cs

    /// <summary>
    ///     Helper class for texture and sprite related functions.
    /// </summary>
    internal static class TextureHelper
    {
        internal static Dictionary<string, Sprite?> SpriteCache = new();

        /// <summary>
        ///     Load a sprite from an embedded resource in the specified assembly, or the currently loaded assembly if not specified.
        ///     Uses an internal cache to avoid loading sprites multiple times.
        /// </summary>
        /// <param name="path"> The path of the embedded sprite. </param>
        /// <param name="assembly"> The assembly from which to load the embedded resource (Current assembly by default). </param>
        /// <returns> A <see cref="Sprite"/> created from the embedded image file, null otherwise. </returns>
        public static Sprite? FindSpriteInAssembly(string path, Assembly? assembly = null)
        {
            if (SpriteCache.ContainsKey(path))
                return SpriteCache[path];

            if (assembly == null) 
                assembly = Assembly.GetExecutingAssembly();

            if (assembly == null)
                return null;

            Sprite? sprite = null;

            try
            {
                if (assembly.GetManifestResourceNames().Contains(path))
                {
                    sprite = LoadSpriteRaw(GetResource(assembly, path));
                }
            }
            catch (Exception ex)
            {
                LobbyCompatibilityPlugin.Logger?.LogError("Unable to find texture in assembly! Exception: " + ex);
            }

            SpriteCache.Add(path, sprite);
            return sprite;
        }

        /// <summary>
        ///     Gets the content of a resource as a byte array.
        /// </summary>
        /// <param name="assembly"> Assembly containing the resource. </param>
        /// <param name="resource"> Full path to the resource. </param>
        /// <returns> The contents of the resource as a byte array. </returns>
        /// <exception cref="FileNotFoundException"> Thrown if the resource specified by <paramref name="resource"/> cannot be found in <paramref name="assembly"/>. </exception>
        public static byte[] GetResource(Assembly assembly, string resource)
        {
            using Stream resourceStream = assembly.GetManifestResourceStream(resource) ?? throw new FileNotFoundException(resource);
            using MemoryStream memoryStream = new(new byte[resourceStream.Length], true);

            resourceStream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        public static Sprite? LoadSpriteRaw(byte[] image, float pixelsPerUnit = 100.0f)
        {
            var spriteTexture = LoadTextureRaw(image);
            if (spriteTexture == null)
                return null;

            return LoadSpriteFromTexture(spriteTexture, pixelsPerUnit);
        }

        public static Texture2D? LoadTextureRaw(byte[] file)
        {
            if (file.Length > 0)
            {
                Texture2D tex2D = new(0, 0, TextureFormat.RGBA32, false, false);
                if (tex2D.LoadImage(file))
                {
                    return tex2D;
                }
            }

            return null;
        }

        public static Sprite? LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100.0f)
        {
            if (spriteTexture == null)
            {
                return null;
            }

            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit);
            sprite.name = spriteTexture.name;
            return sprite;
        }
    }
}
