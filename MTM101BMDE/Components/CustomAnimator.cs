using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MTM101BaldAPI.Components
{
    public class CustomAnimation<T>
    {
        public CustomAnimationFrame<T>[] frames { private set; get; }

        public float animationLength { private set; get; }

        /// <summary>
        /// Create an animation with the specified FPS
        /// </summary>
        /// <param name="fps"></param>
        /// <param name="frames"></param>
        public CustomAnimation(int fps, T[] frames)
        {
            this.frames = new CustomAnimationFrame<T>[frames.Length];
            float timePerFrame = ((1000f / (float)fps) / 1000f);
            for (int i = 0; i < this.frames.Length; i++)
            {
                this.frames[i] = new CustomAnimationFrame<T>(frames[i], timePerFrame);
            }
            animationLength = ((float)frames.Length / (float)fps);
        }

        /// <summary>
        /// Create an animation that is totalTime long.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="totalTime"></param>
        public CustomAnimation(T[] frames, float totalTime)
        {
            this.frames = new CustomAnimationFrame<T>[frames.Length];
            float timePerFrame = totalTime / frames.Length;
            for (int i = 0; i < this.frames.Length; i++)
            {
                this.frames[i] = new CustomAnimationFrame<T>(frames[i], timePerFrame);
            }
            animationLength = totalTime;
        }

        public CustomAnimation(CustomAnimationFrame<T>[] frames)
        {
            this.frames = frames;
            for (int i = 0; i < this.frames.Length; i++)
            {
                animationLength += this.frames[i].frameTime;
            }
        }
    }

    public struct CustomAnimationFrame<T>
    {
        public T value { private set; get; }
        public float frameTime { private set; get; }

        public CustomAnimationFrame(T val, float time)
        {
            value = val;
            frameTime = time;
        }
    }

    public interface IAnimationPlayer
    {
        void Play(string name, float speed);
    }

    /// <summary>
    /// A base for a monobehavior implementation of ICustomAnimator, including setting a default animation.
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TAnimation"></typeparam>
    /// <typeparam name="TFrame"></typeparam>
    public abstract class CustomAnimatorMono<TType, TAnimation, TFrame> : MonoBehaviour, IAnimationPlayer where TAnimation : CustomAnimation<TFrame>
    {
        /// <summary>
        /// Determines if this CustomAnimator should be affected by timescale.
        /// </summary>
        public bool useUnscaledTime = false;
        public Dictionary<string, TAnimation> animations = new Dictionary<string, TAnimation>();
        protected float currentFrameTime = 0f;
        protected float currentAnimTime = 0f;
        protected int _currentFrameIndex = 0;
        protected string currentAnim = "";
        protected string defaultAnim = "";
        protected float defaultAnimSpeed = 1f;
        protected bool paused = false;

        /// <summary>
        /// Populate the animations dictionary with a dictionary of keys and frame arrays with the specified FPS.
        /// </summary>
        /// <param name="animations"></param>
        /// <param name="fps"></param>
        public virtual void PopulateAnimations(Dictionary<string, TFrame[]> animations, int fps)
        {
            foreach (KeyValuePair<string, TFrame[]> frameC in animations)
            {
                this.animations.Add(frameC.Key, (TAnimation)new CustomAnimation<TFrame>(fps, frameC.Value));
            }
        }

        public TAnimation currentAnimation
        {
            get
            {
                if (currentAnim == "") return null;
                return animations[currentAnim];
            }
        }

        public string currentAnimationName => currentAnim;

        public CustomAnimationFrame<TFrame> currentFrame
        {
            get
            {
                return currentAnimation.frames[Mathf.Min(_currentFrameIndex, currentAnimation.frames.Length - 1)];
            }
        }

        protected float _currentSpeed = 1f;
        public virtual float Speed
        {
            get
            {
                return paused ? 0f : _currentSpeed;
            }
        }

        public float CurrentAnimationTime
        {
            get
            {
                return currentAnimTime / Speed;
            }
        }

        public abstract TType affectedObject { get; set; }
        public int currentFrameIndex
        {
            get
            {
                return _currentFrameIndex;
            }
            set
            {
                _currentFrameIndex = value;
                currentFrameTime = 0f;
            }
        }

        public virtual void ChangeSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public virtual void SetPause(bool pause)
        {
            paused = pause;
        }

        public virtual void SetDefaultAnimation(string newDef, float newDefSpeed)
        {
            defaultAnim = newDef;
            defaultAnimSpeed = newDefSpeed;
            if (currentAnim == "")
            {
                Play(newDef, newDefSpeed);
            }
        }

        void Update()
        {
            VirtualUpdate();
        }

        protected virtual float GetDeltaTime()
        {
            return (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        protected virtual void VirtualUpdate()
        {
            if (currentAnim == "") return;
            float delta = GetDeltaTime() * Speed;
            currentAnimTime += delta;
            currentFrameTime += delta;
            if (currentFrameTime >= currentFrame.frameTime)
            {
                _currentFrameIndex++;
                currentFrameTime = 0f;
                UpdateFrame();
            }
            if (currentAnimTime >= currentAnimation.animationLength)
            {
                Stop();
            }
        }

        protected abstract void UpdateFrame();

        public virtual void Play(string name, float speed)
        {
            if (animations.ContainsKey(name))
            {
                currentAnim = name;
                currentAnimTime = 0f;
                currentFrameTime = 0f;
                _currentFrameIndex = 0;
                UpdateFrame();
                return;
            }
            Debug.LogError("Attempted to play non-existant animation " + name + "! (" + gameObject.name + ")");
        }

        public virtual void Stop()
        {
            currentAnim = defaultAnim;
            if (defaultAnim != "")
            {
                Play(defaultAnim, defaultAnimSpeed);
            }
        }
    }

    /// <summary>
    /// A custom animator for sprites.
    /// Note that the animations do not serialize, so initialize them in the entities Initialize function.
    /// </summary>
    public class CustomSpriteAnimator : CustomAnimatorMono<SpriteRenderer, CustomAnimation<Sprite>, Sprite>
    {
        public override SpriteRenderer affectedObject
        {
            get
            {
                return this.spriteRenderer;
            }
            set
            {
                this.spriteRenderer = value;
            }
        }
        public SpriteRenderer spriteRenderer;

        protected override void UpdateFrame()
        {
            affectedObject.sprite = currentFrame.value;
        }

    }

    /// <summary>
    /// A custom animator for images.
    /// Note that the animations do not serialize, so initialize them in the entities Initialize function.
    /// </summary>
    public class CustomImageAnimator : CustomAnimatorMono<Image, CustomAnimation<Sprite>, Sprite>
    {
        public override Image affectedObject
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
            }
        }
        public Image image;

        protected override void UpdateFrame()
        {
            affectedObject.sprite = currentFrame.value;
        }

    }
}
