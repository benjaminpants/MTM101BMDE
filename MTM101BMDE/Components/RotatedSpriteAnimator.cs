using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{
    public class RotatedSpriteAnimator : CustomAnimatorMono<SpriteRotator, CustomAnimation<Sprite[]>, Sprite[]>
    {
        public override SpriteRotator affectedObject
        {
            get
            {
                return this.rotator;
            }
            set
            {
                this.rotator = value;
            }
        }
        public SpriteRotator rotator;

        protected override void UpdateFrame()
        {
            UpdateSprites(currentFrame.value);
        }

        static FieldInfo _angleRange = AccessTools.Field(typeof(SpriteRotator), "angleRange");
        static FieldInfo _sprites = AccessTools.Field(typeof(SpriteRotator), "sprites");
        void UpdateSprites(Sprite[] sprites)
        {
            _sprites.SetValue(affectedObject, sprites);
            _angleRange.SetValue(affectedObject, (float)(360 / sprites.Length));
        }
    }
}
