using HarmonyLib;
using MTM101BaldAPI.Components;
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
        public static Dictionary<string, CustomAnimation<Sprite>> LoadAsepriteAnimationsFromFile(string path, float pixelsPerUnit, Vector2 pivot)
        {
            AsepriteSheet sheet = JsonConvert.DeserializeObject<AsepriteSheet>(File.ReadAllText(path));
            sheet.PopulateFrames();
            sheet.texture = AssetLoader.TextureFromFile(Path.Combine(Path.GetDirectoryName(path), sheet.meta.image));
            return AsepriteConvertSheetToAnimations(sheet, pixelsPerUnit, pivot);
        }

        internal static Dictionary<string, CustomAnimation<Sprite>> AsepriteConvertSheetToAnimations(AsepriteSheet sheet, float pixelsPerUnit, Vector2 pivot)
        {
            Dictionary<string, CustomAnimation<Sprite>> result = new Dictionary<string, CustomAnimation<Sprite>>();
            foreach (FrameTag tag in sheet.meta.frameTags)
            {
                result.Add(tag.name, AsepriteCreateAnimationFromTag(sheet.texture, pixelsPerUnit, pivot, sheet.frames, tag));
            }
            return result;
        }

        internal static CustomAnimation<Sprite> AsepriteCreateAnimationFromTag(Texture2D texture, float pixelsPerUnit, Vector2 pivot, KeyValuePair<string, Frame>[] frames, FrameTag tag)
        {
            KeyValuePair<string, Frame>[] selectedFrames = frames.Skip(tag.from).Take((tag.to - tag.from) + 1).ToArray();
            CustomAnimationFrame<Sprite>[] animationFrames = new CustomAnimationFrame<Sprite>[selectedFrames.Length];
            for (int i = 0; i < selectedFrames.Length; i++)
            {
                KeyValuePair<string, Frame> currentFrame = selectedFrames[i];
                Sprite sprite = Sprite.Create(texture, currentFrame.Value.frame.rect, pivot, pixelsPerUnit);
                sprite.name = currentFrame.Key;
                animationFrames[i] = new CustomAnimationFrame<Sprite>(sprite, currentFrame.Value.duration / 1000f);
            }
            // handle direction and repeat stuff, stupid.
            switch (tag.direction)
            {
                case "forward":
                    break;
                case "backward":
                    animationFrames = animationFrames.Reverse().ToArray();
                    break;
                case "pingpong":
                    animationFrames = animationFrames.AddRangeToArray(animationFrames.Reverse().ToArray()).ToArray();
                    break;
                case "pingpong_reverse":
                    animationFrames = animationFrames.AddRangeToArray(animationFrames.Reverse().ToArray()).Reverse().ToArray();
                    break;
                default:
                    MTM101BaldiDevAPI.Log.LogWarning("Unknown tag direction: " + tag.direction);
                    break;
            }
            CustomAnimationFrame<Sprite>[] animationFramesOriginal = animationFrames;
            for (int i = 1; i < tag.repeat; i++)
            {
                animationFrames = animationFrames.AddRangeToArray(animationFramesOriginal);
            }
            return new CustomAnimation<Sprite>(animationFrames);
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

        internal struct FrameTag
        {
            public string name;
            public int from;
            public int to;
            public string direction;
            public int repeat;
            public string color;
        }
    }

    internal struct SerializableRect
    {
        public float x, y, w, h;

        [JsonIgnore]
        public Rect rect => new Rect(x, -y, w, -h);
    }

    internal struct SerializableWidthHeight
    {
        public float w, h;
    }
#pragma warning restore CS0649
}
