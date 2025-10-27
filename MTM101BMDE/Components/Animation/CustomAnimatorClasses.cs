using System;
using System.Collections.Generic;
using System.Text;
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

    public class CustomSpriteRendererAnimator : CustomAnimator<SpriteAnimation, SpriteFrame, Sprite>
    {
        public SpriteRenderer renderer;
        public override void ApplyFrame(Sprite frame)
        {
            renderer.sprite = frame;
        }
    }

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
