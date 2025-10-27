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
}
