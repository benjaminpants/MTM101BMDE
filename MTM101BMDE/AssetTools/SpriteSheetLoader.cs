using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Components.Animation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static MTM101BaldAPI.AssetTools.SpriteSheets.AsepriteSheet;

namespace MTM101BaldAPI.AssetTools.SpriteSheets
{
    public static class SpriteSheetLoader
    {
        /// <summary>
        /// Loads an Aesprite JSON file from the specified file path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static Dictionary<string, SpriteAnimation> LoadAsepriteAnimationsFromFile(string path, float pixelsPerUnit, Vector2 pivot)
        {
            AsepriteSheet sheet = JsonConvert.DeserializeObject<AsepriteSheet>(File.ReadAllText(path));
            sheet.PopulateFrames();
            sheet.texture = AssetLoader.TextureFromFile(Path.Combine(Path.GetDirectoryName(path), sheet.meta.image));
            return AsepriteConvertSheetToAnimations(sheet, pixelsPerUnit, pivot);
        }

        /// <summary>
        /// Loads an Aesprite JSON file from the specified mod path.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <param name="pivot"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Dictionary<string, SpriteAnimation> LoadAsepriteAnimationsFromMod(BaseUnityPlugin plugin, float pixelsPerUnit, Vector2 pivot, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, AssetLoader.GetModPath(plugin));
            return LoadAsepriteAnimationsFromFile(Path.Combine(pathz.ToArray()), pixelsPerUnit, pivot);
        }

        internal static Dictionary<string, SpriteAnimation> AsepriteConvertSheetToAnimations(AsepriteSheet sheet, float pixelsPerUnit, Vector2 pivot)
        {
            Dictionary<string, SpriteAnimation> result = new Dictionary<string, SpriteAnimation>();
            foreach (FrameTag tag in sheet.meta.frameTags)
            {
                result.Add(tag.name, AsepriteCreateAnimationFromTag(sheet.texture, pixelsPerUnit, pivot, sheet.frames, tag));
            }
            return result;
        }

        internal static SpriteAnimation AsepriteCreateAnimationFromTag(Texture2D texture, float pixelsPerUnit, Vector2 pivot, KeyValuePair<string, Frame>[] frames, FrameTag tag)
        {
            KeyValuePair<string, Frame>[] selectedFrames = frames.Skip(tag.from).Take((tag.to - tag.from) + 1).ToArray();
            SpriteFrame[] animationFrames = new SpriteFrame[selectedFrames.Length];
            for (int i = 0; i < selectedFrames.Length; i++)
            {
                KeyValuePair<string, Frame> currentFrame = selectedFrames[i];
                Sprite sprite = Sprite.Create(texture, currentFrame.Value.frame.rect, pivot, pixelsPerUnit);
                sprite.name = currentFrame.Key;
                animationFrames[i] = new SpriteFrame(sprite, currentFrame.Value.duration / 1000f);
            }
            // handle direction and repeat stuff, stupid.
            switch (tag.direction)
            {
                case "forward":
                    break;
                case "reverse":
                case "backward":
                    animationFrames = animationFrames.Reverse().ToArray();
                    break;
                case "pingpong_reverse":
                case "pingpong":
                    List<SpriteFrame> reversedPing = animationFrames.Reverse().ToList();
                    animationFrames = animationFrames.AddRangeToArray(reversedPing.ToArray()).ToArray();
                    if (tag.direction == "pingpong_reverse")
                    {
                        animationFrames = animationFrames.Reverse().ToArray();
                    }
                    break;
                default:
                    MTM101BaldiDevAPI.Log.LogWarning("Unknown tag direction: " + tag.direction);
                    break;
            }
            SpriteFrame[] animationFramesOriginal = animationFrames;
            for (int i = 1; i < tag.repeat; i++)
            {
                animationFrames = animationFrames.AddRangeToArray(animationFramesOriginal);
            }
            return new SpriteAnimation(animationFrames);
        }
    }

#pragma warning disable CS0649
    internal class AsepriteSheet
    {
        [JsonIgnore]
        public Texture2D texture;
        public Metadata meta;

        [JsonProperty("frames")]
        private Dictionary<string, Frame> framesDictionary;

        [JsonIgnore]
        public KeyValuePair<string, Frame>[] frames;

        public void PopulateFrames()
        {
            string[] keys = framesDictionary.Keys.ToArray();
            frames = new KeyValuePair<string, Frame>[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                frames[i] = new KeyValuePair<string, Frame>(keys[i], framesDictionary[keys[i]]);
            }
        }

        internal struct Frame
        {
            public SerializableRect frame;
            public bool rotated;
            public bool trimmed;
            public SerializableRect spriteSourceSize;
            public SerializableWidthHeight sourceSize;
            public float duration;
        }

        internal struct Metadata
        {
            public string app;
            public string version;
            public string image;
            public SerializableWidthHeight size;
            public float scale;
            public FrameTag[] frameTags;
        }

        internal class FrameTag
        {
            public string name;
            public int from;
            public int to;
            public string direction;
            public int repeat = 1;
            public string color;
        }
    }

    internal struct SerializableRect
    {
        public float x, y, w, h;

        [JsonIgnore]
        public Rect rect => new Rect(x, -y, w, -h); //todo: change
    }

    internal struct SerializableWidthHeight
    {
        public float w, h;
    }
#pragma warning restore CS0649
}
