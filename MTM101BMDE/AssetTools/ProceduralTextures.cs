using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace MTM101BaldAPI.AssetTools
{
    /// <summary>
    /// A builder for procedural textures, AKA textures generated with other textures.
    /// <b>This is slow! Please avoid using this for real time applications, preferably use when loading your mod!</b>
    /// </summary>
    public class ProceduralTextureBuilder
    {
        Dictionary<string, List<Texture2D>> internalTextures = new Dictionary<string, List<Texture2D>>();


        private void AddTextureWithTag(Texture2D tex, string tag)
        {
            if (tex == null) return;
            if (!internalTextures.ContainsKey(tag))
            {
                internalTextures.Add(tag, new List<Texture2D>());
            }
            tex.name = tag + internalTextures[tag].Count;
            internalTextures[tag].Add(tex);
        }

        /// <summary>
        /// Gets all currently stored textures with the specified tag.
        /// <b>Do not use this to get your finished textures! Use ExportAll for that!</b>
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Texture2D[] GetAllTexturesWithTag(string tag)
        {
            if (!internalTextures.ContainsKey(tag)) return new Texture2D[0];
            return internalTextures[tag].ToArray();
        }

        private void PerformActionWithAllTaggedTextures(string tag, string resultTag, Func<Texture2D, Texture2D> action)
        {
            Texture2D[] textures = GetAllTexturesWithTag(tag);
            for (int i = 0; i < textures.Length; i++)
            {
                AddTextureWithTag(action(textures[i]), resultTag);
            }
        }

        /// <summary>
        /// Clip the texture using all textures in the tag as masks, and put the results into the resultTag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="toClip"></param>
        /// <param name="resultTag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder Clip(string tag, Texture2D toClip, string resultTag)
        {
            Texture2D readableClip = MakeReadableCopy(toClip);
            Texture2D[] masks = GetAllTexturesWithTag(tag);
            for (int i = 0; i < masks.Length; i++)
            {
                AddTextureWithTag(AlphaClip(toClip, masks[i]), resultTag);
            }
            UnityEngine.Object.Destroy(readableClip);
            return this;
        }


        /// <summary>
        /// Splits the specified texture into multiple textures via color, and puts all the resulting textures into the specified tag.
        /// <b>Only use this in specially designed images with few colors!</b>
        /// </summary>
        /// <param name="toSplit"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder SplitViaColor(Texture2D toSplit, string tag)
        {
            Dictionary<Color, Texture2D> generatedTextures = new Dictionary<Color, Texture2D>();
            Texture2D readableCopy = MakeReadableCopy(toSplit);

            int width = readableCopy.width;
            int height = readableCopy.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = readableCopy.GetPixel(x, y);
                    if (!generatedTextures.ContainsKey(color))
                    {
                        generatedTextures.Add(color, MakeNewTextureCopy(toSplit));
                    }
                    generatedTextures[color].SetPixel(x, y, color);
                }
            }
            MTM101BaldiDevAPI.Log.LogInfo("Generated: " + generatedTextures.Count + " textures!");
            foreach (Texture2D tex in generatedTextures.Values)
            {
                AddTextureWithTag(tex, tag);
            }
            UnityEngine.Object.Destroy(readableCopy);
            return this;
        }

        /// <summary>
        /// Trims all the textures so that their bounds are only where there are visible pixels.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="resultTag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder Trim(string tag, string resultTag)
        {
            PerformActionWithAllTaggedTextures(tag, resultTag, Trim);
            return this;
        }

        private Texture2D Trim(Texture2D toTrim)
        {
            return Trim(toTrim,1f);
        }

        private Texture2D Overlay(Texture2D texture1, Texture2D texture2)
        {
            if (!texture1.isReadable) throw new ArgumentException("Passed a non-readable texture! (1)");
            if (!texture2.isReadable) throw new ArgumentException("Passed a non-readable texture! (2)");
            return null;
        }


        public ProceduralTextureBuilder RemoveAllEmpty(string tagToPurge, float alphaTolerance = 1f)
        {
            Texture2D[] texes = GetAllTexturesWithTag(tagToPurge);
            List<Texture2D> texturesToKill = new List<Texture2D>();
            foreach (Texture2D tex in texes)
            {
                int width = tex.width;
                int height = tex.height;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color colorAt = tex.GetPixel(x, y);
                        if (colorAt.a >= alphaTolerance)
                        {
                            goto TheEnd;
                        }
                    }
                }
                texturesToKill.Add(tex);
                TheEnd:;

            }

            texturesToKill.Do(x => GameObject.Destroy(x));

            internalTextures[tagToPurge].RemoveAll(x => texturesToKill.Contains(x));

            return this;
        }

        // Trims the respect texture 2D so that there are no pixels with an alpha less than alpha tolerance at the edges.
        private Texture2D Trim(Texture2D toTrim, float alphaTolerance = 1f)
        {
            if (!toTrim.isReadable) throw new ArgumentException("Passed a non-readable texture!");
            Rect bounds = new Rect();
            bool foundOrigin = false;
            int width = toTrim.width;
            int height = toTrim.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color colorAt = toTrim.GetPixel(x, y);
                    if (colorAt.a < alphaTolerance) break;
                    if (!foundOrigin)
                    {
                        bounds.x = x;
                        bounds.y = y;
                        foundOrigin = true;
                    }
                    bounds.xMax = x;
                    bounds.yMax = y;
                }
            }
            if (bounds.size == Vector2.zero) return null;
            RenderTexture tempTex = RenderTexture.GetTemporary(toTrim.width,toTrim.height);
            RenderTexture.active = tempTex;
            Texture2D newTexture = MakeNewTextureCopy(toTrim);
            newTexture.Resize((int)bounds.width, (int)bounds.height);
            newTexture.ReadPixels(bounds,0,0);
            //newTexture.Apply();
            tempTex.Release();
            return newTexture;
        }

        /// <summary>
        /// Perform a custom texture action on all textures with the specified tag.
        /// Be sure to delete any textures you create that you dont add to the ProceduralTextureBuilder!
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="customAction"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder CustomActionWithAll(string tag, Action<ProceduralTextureBuilder, Texture2D> customAction)
        {
            Texture2D[] taggedTex = GetAllTexturesWithTag(tag);
            for (int i = 0; i < taggedTex.Length; i++)
            {
                customAction(this, taggedTex[i]);
            }
            return this;
        }

        /// <summary>
        /// Export all textures with the specified tag, destroying all other textures.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Texture2D[] ExportAll(string tag)
        {
            Texture2D[] taggedTextures = GetAllTexturesWithTag(tag);
            for (int i = 0; i < taggedTextures.Length; i++)
            {
                taggedTextures[i].Apply();
            }
            foreach (KeyValuePair<string, List<Texture2D>> kvp in internalTextures)
            {
                if (kvp.Key == tag) continue;
                kvp.Value.ForEach(x =>
                {
                    UnityEngine.Object.Destroy(x);
                });
            }
            internalTextures.Clear(); // its all done
            return taggedTextures;
        }

        /// <summary>
        /// Loads a texture into the procedural texture builder with the specified tag.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder LoadTexture(Texture2D tex, string tag)
        {
            AddTextureWithTag(MakeReadableCopy(tex), tag);
            return this;
        }

        
        /// <summary>
        /// Flips all textures with the specified tag horizontally, storing the results into the resulting tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="resultTag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder FlipAllHorizontal(string tag, string resultTag)
        {
            PerformActionWithAllTaggedTextures(tag, resultTag, FlipX);
            return this;
        }

        /// <summary>
        /// Flips all textures with the specified tag vertically, storing the results into the resulting tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="resultTag"></param>
        /// <returns></returns>
        public ProceduralTextureBuilder FlipAllVertical(string tag, string resultTag)
        {
            PerformActionWithAllTaggedTextures(tag, resultTag, FlipY);
            return this;
        }

        private Texture2D MakeNewTextureCopy(Texture2D propertiesToCopy)
        {
            Texture2D newTex = new Texture2D(propertiesToCopy.width, propertiesToCopy.height, propertiesToCopy.format, propertiesToCopy.mipmapCount > 1);
            for (int x = 0; x < newTex.width; x++)
            {
                for (int y = 0; y < newTex.height; y++)
                {
                    newTex.SetPixel(x, y, Color.clear);
                }
            }
            newTex.Apply();
            return newTex;
        }

        private Texture2D AlphaClip(Texture2D baseTexture, Texture2D clipWith)
        {
            Texture2D newTexture = MakeNewTextureCopy(baseTexture);

            int width = newTexture.width;
            int height = newTexture.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newTexture.SetPixel(x, y, baseTexture.GetPixel(x, y) * new Color(1f, 1f, 1f, clipWith.GetPixel(x, y).a));
                }
            }
            //newTexture.Apply();

            return newTexture;

        }
        private Texture2D FlipX(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(width - i - 1, j, texture.GetPixel(i, j));
                }
            }
            //flipped.Apply();

            return flipped;
        }

        private Texture2D FlipY(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(i, height - j - 1, texture.GetPixel(i, j));
                }
            }
            //flipped.Apply();

            return flipped;
        }

        private Texture2D MakeReadableCopy(Texture2D source)
        {
            return AssetLoader.MakeReadableCopy(source, false);
        }
    }
}
