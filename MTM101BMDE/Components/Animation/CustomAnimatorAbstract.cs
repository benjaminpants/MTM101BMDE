using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace MTM101BaldAPI.Components.Animation
{
    public interface ISimpleAnimator
    {
        void Play(string animation, float speed);
    }


    [Serializable]
    public abstract class CustomAnimationFrame<T>
    {
        [SerializeField]
        public T value;

        [SerializeField]
        public float time;

        public CustomAnimationFrame()
        {
            value = default;
            time = 0f;
        }

        public CustomAnimationFrame(T value, float time)
        {
            this.value = value;
            this.time = time;
        }
    }

    [Serializable]
    public abstract class CustomAnimation<Frame, UnderlyingType> where Frame : CustomAnimationFrame<UnderlyingType>, new()
    {
        /// <summary>
        /// The amount of frames in the animation
        /// </summary>
        public Frame[] frames;

        /// <summary>
        /// The length of the animation in seconds.
        /// </summary>
        public float animationLength;

        /// <summary>
        /// Create an animation with the specified FPS
        /// </summary>
        /// <param name="fps"></param>
        /// <param name="frames"></param>
        public CustomAnimation(int fps, UnderlyingType[] frames)
        {
            this.frames = new Frame[frames.Length];
            float timePerFrame = 1000f / fps / 1000f;
            for (int i = 0; i < this.frames.Length; i++)
            {
                this.frames[i] = new Frame();
                this.frames[i].value = frames[i];
                this.frames[i].time = timePerFrame;
            }
            animationLength = frames.Length / (float)fps;
        }

        /// <summary>
        /// Create an animation that is totalTime long.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="totalTime"></param>
        public CustomAnimation(UnderlyingType[] frames, float totalTime)
        {
            this.frames = new Frame[frames.Length];
            float timePerFrame = totalTime / frames.Length;
            for (int i = 0; i < this.frames.Length; i++)
            {
                this.frames[i] = new Frame();
                this.frames[i].value = frames[i];
                this.frames[i].time = timePerFrame;
            }
            animationLength = totalTime;
        }

        public CustomAnimation(Frame[] frames)
        {
            this.frames = frames;
            for (int i = 0; i < this.frames.Length; i++)
            {
                animationLength += this.frames[i].time;
            }
        }

        public CustomAnimation()
        {
            this.frames = new Frame[0];
            this.animationLength = 0f;
        }
    }

    public abstract class CustomAnimator<AnimationType, Frame, UnderlyingType> : MonoBehaviour, ISimpleAnimator where AnimationType : CustomAnimation<Frame, UnderlyingType> where Frame : CustomAnimationFrame<UnderlyingType>, new()
    {

        [SerializeField]
        protected string[] animationKeys = new string[0];

        [SerializeField]
        protected AnimationType[] animationsValues;

        protected string currentAnimationId = null;
        protected int currentAnimationFrame = 0;
        protected float currentAnimationTime = 0f;
        protected bool looping = false;
        protected float currentSpeed = 1f;

        protected AnimationType currentAnimation;

        protected bool paused = false;

        [SerializeField]
        protected string defaultAnimation = null;

        [SerializeField]
        protected float defaultSpeed = 1f;

        public string AnimationId
        {
            get
            {
                return currentAnimationId;
            }
        }

        public int AnimationFrame
        {
            get
            {
                return currentAnimationFrame;
            }
        }

        public float AnimationSpeed
        {
            get
            {
                return paused ? 0f : GetTimeScale() * Time.timeScale * currentSpeed;
            }
        }

        protected AnimationType GetAnimation(string key)
        {
            return animationsValues[Array.IndexOf(animationKeys, key)];
        }

        public TimeScaleType timeScale = TimeScaleType.Environment;
        public EnvironmentController ec;
        public float GetTimeScale()
        {
            if (ec == null) return 1f;
            switch (timeScale)
            {
                case TimeScaleType.Null:
                    return 1f;
                case TimeScaleType.Npc:
                    return ec.NpcTimeScale;
                case TimeScaleType.Player:
                    return ec.PlayerTimeScale;
                case TimeScaleType.Environment:
                    return ec.EnvironmentTimeScale;
            }
            return 1f;
        }

        /// <summary>
        /// Loads the animations into the CustomAnimator
        /// </summary>
        /// <param name="animations"></param>
        public void LoadAnimations(Dictionary<string, AnimationType> animations)
        {
            int index = 0;
            animationKeys = new string[animations.Count];
            animationsValues = new AnimationType[animations.Count];
            foreach (var key in animations.Keys)
            {
                animationKeys[index] = key;
                animationsValues[index] = animations[key];
                index++;
            }
        }

        public void AddAnimation(string key, AnimationType anim)
        {
            animationKeys = animationKeys.AddToArray(key);
            animationsValues = animationsValues.AddToArray(anim);
        }

        public abstract void ApplyFrame(UnderlyingType frame);

        /// <summary>
        /// Plays the animation with the specified id if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="loop"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Play(string id, float speed, bool loop)
        {
            if (speed < 0f) throw new InvalidOperationException("Attempted to play: " + id + " with invalid speed " + speed + " in custom animator!");
            looping = loop;
            currentAnimationId = id;
            if (string.IsNullOrEmpty(currentAnimationId))
            {
                currentAnimationId = null;
                currentAnimation = null;
            }
            else
            {
                currentAnimation = GetAnimation(currentAnimationId);
            }
            currentAnimationTime = 0f;
            currentAnimationFrame = 0;
        }

        public void Play(string id, float speed)
        {
            Play(id, speed);
        }

        /// <summary>
        /// Stops the currently running animation and returns to the default animation if specified.
        /// </summary>
        public void Stop()
        {
            Play(defaultAnimation, defaultSpeed, true);
        }

        public void SetPause(bool paused)
        {
            this.paused = paused;
        }

        public void SetLoop(bool loop)
        {
            looping = loop;
        }

        public void SetDefaultAnimation(string animation, float speed)
        {
            defaultAnimation = animation;
            defaultSpeed = speed;
            if (currentAnimation == null)
            {
                Play(animation, speed, true);
            }
        }

        public void ChangeSpeed(float speed)
        {
            currentSpeed = speed;
        }

        void Start()
        {

        }

        void Update()
        {
            if (currentAnimation == null) return;
            if (animationKeys.Length == 0) return;
            if (paused) return;
            currentAnimationTime += Time.deltaTime * GetTimeScale() * AnimationSpeed;
            while (currentAnimationTime >= currentAnimation.frames[currentAnimationFrame].time)
            {
                currentAnimationTime = Mathf.Max(0f, currentAnimationTime - currentAnimation.frames[currentAnimationFrame].time);
                currentAnimationFrame++;
                if (currentAnimationFrame > currentAnimation.frames.Length)
                {
                    if (looping)
                    {
                        currentAnimationFrame = 0;
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
            ApplyFrame(currentAnimation.frames[currentAnimationFrame].value);
        }
    }
}
