using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.Components.Animation
{
    [Serializable]
    public class SpriteFrame : CustomAnimationFrame
    {
        public Sprite value;
        public override object RawValue => value;

        public SpriteFrame() : base() { }
        public SpriteFrame(Sprite value, float time) : base(time) { this.value = value; }
    }

    [Serializable]
    public class SpriteAnimation : CustomAnimation
    {
        public SpriteFrame[] frames;
        public override CustomAnimationFrame[] GetBaseFrames() => frames;

        public SpriteAnimation() { frames = new SpriteFrame[0]; }

        public SpriteAnimation(SpriteFrame[] frames)
        {
            this.frames = frames;
            for (int i = 0; i < this.frames.Length; i++)
            {
                animationLength += this.frames[i].time;
            }
        }
        public SpriteAnimation(int fps, Sprite[] spriteValues)
        {
            frames = new SpriteFrame[spriteValues.Length];
            float timePerFrame = 1f / fps;
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new SpriteFrame(spriteValues[i], timePerFrame);
            animationLength = spriteValues.Length / (float)fps;
        }

        public SpriteAnimation(Sprite[] spriteValues, float totalTime)
        {
            frames = new SpriteFrame[spriteValues.Length];
            float timePerFrame = totalTime / spriteValues.Length;
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new SpriteFrame(spriteValues[i], timePerFrame);
            animationLength = totalTime;
        }
    }

    [Serializable]
    public class Texture2DFrame : CustomAnimationFrame
    {
        public Texture2D value;
        public override object RawValue => value;

        public Texture2DFrame() : base() { }
        public Texture2DFrame(Texture2D value, float time) : base(time) { this.value = value; }
    }

    [Serializable]
    public class Texture2DAnimation : CustomAnimation
    {
        public Texture2DFrame[] frames;
        public override CustomAnimationFrame[] GetBaseFrames() => frames;

        public Texture2DAnimation() { frames = new Texture2DFrame[0]; }

        public Texture2DAnimation(Texture2DFrame[] frames)
        {
            this.frames = frames;
            for (int i = 0; i < this.frames.Length; i++)
            {
                animationLength += this.frames[i].time;
            }
        }

        public Texture2DAnimation(Texture2D[] frames, float totalTime)
        {
            this.frames = new Texture2DFrame[frames.Length];
            float timePerFrame = totalTime / frames.Length;
            for (int i = 0; i < frames.Length; i++)
                this.frames[i] = new Texture2DFrame(frames[i], timePerFrame);
            animationLength = totalTime;
        }

        public Texture2DAnimation(int fps, Texture2D[] textureValues)
        {
            frames = new Texture2DFrame[textureValues.Length];
            float timePerFrame = 1f / fps;
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new Texture2DFrame(textureValues[i], timePerFrame);
            animationLength = textureValues.Length / (float)fps;
        }
    }

    [Serializable]
    public class SpriteArrayFrame : CustomAnimationFrame
    {
        public Sprite[] value;
        public override object RawValue => value;

        public SpriteArrayFrame() : base() { }
        public SpriteArrayFrame(Sprite[] value, float time) : base(time) { this.value = value; }
    }

    [Serializable]
    public class SpriteArrayAnimation : CustomAnimation
    {
        public SpriteArrayFrame[] frames;
        public override CustomAnimationFrame[] GetBaseFrames() => frames;

        public SpriteArrayAnimation() { frames = new SpriteArrayFrame[0]; }

        public SpriteArrayAnimation(SpriteArrayFrame[] frames)
        {
            this.frames = frames;
            for (int i = 0; i < this.frames.Length; i++)
            {
                animationLength += this.frames[i].time;
            }
        }

        public SpriteArrayAnimation(Sprite[][] frames, float totalTime)
        {
            this.frames = new SpriteArrayFrame[frames.Length];
            float timePerFrame = totalTime / frames.Length;
            for (int i = 0; i < frames.Length; i++)
                this.frames[i] = new SpriteArrayFrame(frames[i], timePerFrame);
            animationLength = totalTime;
        }

        public SpriteArrayAnimation(int fps, Sprite[][] spriteArrays)
        {
            frames = new SpriteArrayFrame[spriteArrays.Length];
            float timePerFrame = 1f / fps;
            for (int i = 0; i < frames.Length; i++)
                frames[i] = new SpriteArrayFrame(spriteArrays[i], timePerFrame);
            animationLength = spriteArrays.Length / (float)fps;
        }
    }

    [Serializable]
    public class CustomRotatedSpriteAnimator : CustomAnimator
    {
        public SpriteRotator rotator;

        static FieldInfo _angleRange = AccessTools.Field(typeof(SpriteRotator), "angleRange");
        static FieldInfo _sprites = AccessTools.Field(typeof(SpriteRotator), "sprites");
        public override void ApplyFrame(object frame)
        {
            if (!(frame is Sprite[] spriteArray)) return;

            _sprites.SetValue(rotator, spriteArray);
            _angleRange.SetValue(rotator, (float)(360 / spriteArray.Length));
        }

        protected override void VirtualAwake()
        {
            SpriteArrayFrame[] frames = null;
            if (string.IsNullOrEmpty(defaultAnimation))
            {
                frames = animations.First().Value.GetBaseFrames() as SpriteArrayFrame[];
                MTM101BaldiDevAPI.Log.LogWarning(string.Format("CustomRotatedSpriteAnimator: {0} did not have a defaultAnimation assigned, sprite may be seemingly random until an animation plays.", name));
                if (frames != null)
                    ApplyFrame(frames[0].value);
                return;
            }
            frames = animations[defaultAnimation].GetBaseFrames() as SpriteArrayFrame[];
            if (frames != null)
                ApplyFrame(frames[0].value);
        }
    }

    [Serializable]
    public class CustomSpriteRendererAnimator : CustomAnimator
    {
        public SpriteRenderer renderer;
        public override void ApplyFrame(object frame)
        {
            // Can be null or not
            renderer.sprite = frame as Sprite;
        }
    }

    [Serializable]
    public class CustomImageAnimator : CustomAnimator
    {
        public Image image;
        public override void ApplyFrame(object frame)
        {
            image.sprite = frame as Sprite;
        }
    }

    public class CustomRawImageAnimator : CustomAnimator
    {
        public RawImage image;
        public override void ApplyFrame(object frame)
        {
            image.texture = frame as Texture2D;
        }
    }
}