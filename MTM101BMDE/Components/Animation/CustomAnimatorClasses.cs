using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.Components.Animation
{
    [Serializable]
    public class SpriteFrame : CustomAnimationFrame<Sprite>
    {
        public SpriteFrame()
        {
        }

        public SpriteFrame(Sprite value, float time) : base(value, time)
        {
        }
    }

    [Serializable]
    public class SpriteAnimation : CustomAnimation<SpriteFrame, Sprite>
    {
        public SpriteAnimation()
        {
        }

        public SpriteAnimation(SpriteFrame[] frames) : base(frames)
        {
        }

        public SpriteAnimation(int fps, Sprite[] frames) : base(fps, frames)
        {
        }

        public SpriteAnimation(Sprite[] frames, float totalTime) : base(frames, totalTime)
        {
        }
    }

    [Serializable]
    public class Texture2DFrame : CustomAnimationFrame<Texture2D>
    {
        public Texture2DFrame()
        {
        }

        public Texture2DFrame(Texture2D value, float time) : base(value, time)
        {
        }
    }

    [Serializable]
    public class Texture2DAnimation : CustomAnimation<Texture2DFrame, Texture2D>
    {
        public Texture2DAnimation()
        {
        }

        public Texture2DAnimation(Texture2DFrame[] frames) : base(frames)
        {
        }

        public Texture2DAnimation(int fps, Texture2D[] frames) : base(fps, frames)
        {
        }

        public Texture2DAnimation(Texture2D[] frames, float totalTime) : base(frames, totalTime)
        {
        }
    }

    [Serializable]
    public class SpriteArrayFrame : CustomAnimationFrame<Sprite[]>
    {
        public SpriteArrayFrame()
        {
        }

        public SpriteArrayFrame(Sprite[] value, float time) : base(value, time)
        {
            
        }
    }

    [Serializable]
    public class SpriteArrayAnimation : CustomAnimation<SpriteArrayFrame, Sprite[]>
    {
        public SpriteArrayAnimation()
        {
        }

        public SpriteArrayAnimation(SpriteArrayFrame[] frames) : base(frames)
        {
        }

        public SpriteArrayAnimation(int fps, Sprite[][] frames) : base(fps, frames)
        {
        }

        public SpriteArrayAnimation(Sprite[][] frames, float totalTime) : base(frames, totalTime)
        {
        }
    }

    [Serializable]
    public class SpriteRotatedAnimation : CustomAnimation<SpriteFrame, Sprite> // I don't get it... It's used in one part.
    {
        public readonly int angleCount;
        public SpriteRotatedAnimation(int angleCount)
        {
            this.angleCount = angleCount;
        }
        public SpriteRotatedAnimation(SpriteFrame[] frames, int angleCount) : base(frames)
        {
            this.angleCount = angleCount;
        }
        public SpriteRotatedAnimation(int fps, Sprite[] frames, int angleCount) : base(fps, frames)
        {
            this.angleCount = angleCount;
        }

        public SpriteRotatedAnimation(Sprite[] frames, float totalTime, int angleCount) : base(frames, totalTime)
        {
            this.angleCount = angleCount;
        }
    }

    [Serializable]
    public class CustomRotatedSpriteAnimator : CustomAnimator<SpriteArrayAnimation, SpriteArrayFrame, Sprite[]>
    {
        public SpriteRotator rotator;

        static FieldInfo _angleRange = AccessTools.Field(typeof(SpriteRotator), "angleRange");
        static FieldInfo _sprites = AccessTools.Field(typeof(SpriteRotator), "sprites");
        public override void ApplyFrame(Sprite[] frame)
        {
            _sprites.SetValue(rotator, frame);
            _angleRange.SetValue(rotator, (float)(360 / frame.Length));
        }

        protected override void VirtualAwake()
        {
            if (string.IsNullOrEmpty(defaultAnimation))
            {
                MTM101BaldiDevAPI.Log.LogWarning(string.Format("CustomRotatedSpriteAnimator: {0} did not have a defaultAnimation assigned, sprite may be seemingly random until an animation plays.", name));
                ApplyFrame(animations.First().Value.frames[0].value);
                return;
            }
            ApplyFrame(animations[defaultAnimation].frames[0].value);
        }
    }

    [Serializable]
    public class CustomSpriteRotatorAnimator : CustomAnimator<SpriteAnimation, SpriteFrame, Sprite>
    {
        public AnimatedSpriteRotator renderer;

        private static FieldInfo
            _renderer = AccessTools.DeclaredField(typeof(AnimatedSpriteRotator), "renderer"),
            _spriteSheet = AccessTools.DeclaredField(typeof(SpriteRotationMap), "spriteSheet"),
            _spriteMap = AccessTools.DeclaredField(typeof(AnimatedSpriteRotator), "spriteMap");

        [SerializeField] private List<SpriteRotationMap> spriteMap = new List<SpriteRotationMap>();

        public override void ApplyFrame(Sprite frame) => renderer.targetSprite = frame; // Any front facing sprites is the frame that is applied to the component.

        private List<Sprite> AddAngledAnimation(int angleCount, List<Sprite> frames)
        {
            var map = new SpriteRotationMap()
            {
                angleCount = angleCount,
            };
            _spriteSheet.SetValue(map, frames.ToArray());
            spriteMap.Add(map);
            List<Sprite> frontFrames = new List<Sprite>(frames);
            for (int i = frontFrames.Count - 1; i >= 0; i--)
            {
                if (!((i % angleCount) == 0))
                    frontFrames.RemoveAt(i);
            }
            return frontFrames;
        }

        public void AddAngledAnimation(string key, int angleCount, List<Sprite> frames, int fps) => animations.Add(key, new SpriteAnimation(fps, AddAngledAnimation(angleCount, frames).ToArray()));
        public void AddAngledAnimation(string key, int angleCount, List<Sprite> frames, float totalTime) => animations.Add(key, new SpriteAnimation(AddAngledAnimation(angleCount, frames).ToArray(), totalTime));
        /// <summary>
        /// Loads the animations into the CustomAnimator
        /// </summary>
        public void LoadAngledAnimations(Dictionary<string, SpriteRotatedAnimation> animations)
        {
            this.animations = new Dictionary<string, SpriteAnimation>();
            foreach (var animation in animations)
                this.animations.Add(animation.Key, new SpriteAnimation(AddAngledAnimation(animation.Value.angleCount, animation.Value.frames.Select(x => x.value).ToList()).ToArray(), animation.Value.animationLength));
        }

        public void SetSpriteRenderer(SpriteRenderer sprRenderer) => _renderer.SetValue(renderer, sprRenderer);

        protected override void VirtualAwake()
        {
            if (renderer != null)
                _spriteMap.SetValue(renderer, spriteMap.ToArray());
        }
    }

    [Serializable]
    public class CustomSpriteRendererAnimator : CustomAnimator<SpriteAnimation, SpriteFrame, Sprite>
    {
        public SpriteRenderer renderer;
        public override void ApplyFrame(Sprite frame)
        {
            renderer.sprite = frame;
        }
    }

    [Serializable]
    public class CustomImageAnimator : CustomAnimator<SpriteAnimation, SpriteFrame, Sprite>
    {
        public Image image;
        public override void ApplyFrame(Sprite frame)
        {
            image.sprite = frame;
        }
    }

    public class CustomRawImageAnimator : CustomAnimator<Texture2DAnimation, Texture2DFrame, Texture2D>
    {
        public RawImage image;
        public override void ApplyFrame(Texture2D frame)
        {
            image.texture = frame;
        }
    }
}
