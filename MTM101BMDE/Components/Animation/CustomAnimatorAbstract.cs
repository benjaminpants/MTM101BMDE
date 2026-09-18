using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MTM101BaldAPI.Components.Animation
{
    public interface ISimpleAnimator
    {
        void Play(string animation, float speed);
        void PlayAtTime(string animation, float speed, float time);

        void Stop();

        float AnimationSpeed { get; set; }
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
            frames = new Frame[0];
            animationLength = 0f;
        }
    }

    public abstract class CustomAnimator<AnimationType, Frame, UnderlyingType> : MonoBehaviour, ISerializationCallbackReceiver, ISimpleAnimator where AnimationType : CustomAnimation<Frame, UnderlyingType>, new() where Frame : CustomAnimationFrame<UnderlyingType>, new()
    {

        protected Dictionary<string, AnimationType> animations = new Dictionary<string, AnimationType>();

        [SerializeField]
        protected string[] animationKeys;

        [SerializeField]
        protected AnimationType[] animationTypes;

        public virtual void OnBeforeSerialize()
        {
            // written kind of weird because before i was already trying to use a string and AnimationType array to get it to serialize, so this code is mostly written like that is still the case
            // i do believe it'd only be a minor refactor to adjust it to be more proper with the dictionary, but i have already spent way too long on this
            animationKeys = new string[animations.Count];
            animationTypes = new AnimationType[animations.Count];
            int index = 0;
            foreach (var item in animations)
            {
                animationKeys[index] = item.Key;
                animationTypes[index] = item.Value;
                index++;
            }
        }

        public virtual void OnAfterDeserialize()
        {
            for (int i = 0; i < animationKeys.Length; i++)
            {
                animations.Add(animationKeys[i], animationTypes[i]);
            }
            animationKeys = null;
            animationTypes = null;
        }

        protected int currentAnimationFrame = 0;
        protected float currentAnimationTime = 0f;

        [SerializeField]
        protected string currentAnimationId = null;
        [SerializeField]
        protected bool looping = false;
        [SerializeField]
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

        public virtual float AnimationSpeed
        {
            get
            {
                if (!useScaledTime)
                {
                    return paused ? 0f : 1f * currentSpeed;
                }
                return paused ? 0f : GetTimeScale() * Time.timeScale * currentSpeed;
            }
            set
            {
                if (value == 0f)
                {
                    paused = true;
                    return;
                }
                paused = false;
                currentSpeed = value;
            }
        }

        public TimeScaleType timeScale = TimeScaleType.Environment;
        public bool useScaledTime = true;
        public EnvironmentController ec;
        public virtual float GetTimeScale()
        {
            if (!useScaledTime) return 1f;
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
        public virtual void LoadAnimations(Dictionary<string, AnimationType> animations)
        {
            this.animations = new Dictionary<string, AnimationType>(animations);
        }

        public void AddAnimation(string key, AnimationType anim)
        {
            animations.Add(key, anim);
        }

        public abstract void ApplyFrame(UnderlyingType frame);

        /// <summary>
        /// Plays the animation with the specified id if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="speed"></param>
        /// <param name="loop"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Play(string id, float speed, bool loop)
        {
            if (speed < 0f) throw new InvalidOperationException("Attempted to play: " + id + " with invalid speed " + speed + " in custom animator!");
            looping = loop;
            currentAnimationId = id;
            currentAnimationTime = 0f;
            currentAnimationFrame = 0;
            if (string.IsNullOrEmpty(currentAnimationId))
            {
                currentAnimationId = null;
                currentAnimation = null;
            }
            else
            {
                currentAnimation = animations[currentAnimationId];
            }
            ChangeSpeed(speed);
        }

        public virtual void PlayAtTime(string id, float speed, float time, bool loop)
        {
            Play(id, speed, loop);
            if (currentAnimation == null) return;
            currentAnimationTime = time;
            while (currentAnimationTime >= currentAnimation.frames[currentAnimationFrame].time)
            {
                currentAnimationTime = Mathf.Max(0f, currentAnimationTime - currentAnimation.frames[currentAnimationFrame].time);
                currentAnimationFrame++;
                if (currentAnimationFrame >= currentAnimation.frames.Length)
                {
                    currentAnimationFrame = currentAnimation.frames.Length - 1;
                    return;
                }
            }
        }

        public void Play(string id, float speed)
        {
            Play(id, speed, false);
        }

        public void PlayAtTime(string id, float speed, float time)
        {
            PlayAtTime(id, speed, time, false);
        }

        /// <summary>
        /// Stops the currently running animation and returns to the default animation if specified.
        /// </summary>
        public virtual void Stop()
        {
            Play(defaultAnimation, defaultSpeed, true);
        }

        public virtual void SetPause(bool paused)
        {
            this.paused = paused;
        }

        public virtual void SetLoop(bool loop)
        {
            looping = loop;
        }

        protected virtual void OnAnimationFinished()
        {

        }

        public void SetDefaultAnimation(string animation, float speed)
        {
            SetDefaultAnimation(animation, speed, false);
        }

        public virtual void SetDefaultAnimation(string animation, float speed, bool play)
        {
            defaultAnimation = animation;
            defaultSpeed = speed;
            if ((currentAnimation == null) || (play))
            {
                Play(animation, speed, true);
            }
        }

        public virtual void ChangeSpeed(float speed)
        {
            currentSpeed = speed;
        }

        void Awake()
        {
            if (defaultAnimation != null)
            {
                Play(defaultAnimation, defaultSpeed, true);
            }
            VirtualAwake();
        }

        protected virtual void VirtualAwake()
        {

        }

        void Update()
        {
            if (currentAnimation == null) { VirtualUpdate(); return; }
            if (animations.Count == 0) { VirtualUpdate(); return; }
            if (paused) { VirtualUpdate(); return; }
            currentAnimationTime += Time.unscaledDeltaTime * AnimationSpeed;
            while (currentAnimationTime >= currentAnimation.frames[currentAnimationFrame].time)
            {
                currentAnimationTime = Mathf.Max(0f, currentAnimationTime - currentAnimation.frames[currentAnimationFrame].time);
                currentAnimationFrame++;
                if (currentAnimationFrame >= currentAnimation.frames.Length)
                {
                    OnAnimationFinished();
                    currentAnimationFrame = 0;
                    if (!looping)
                    {
                        Stop();
                        break;
                    }
                }
            }
            if (currentAnimation == null) return;
            ApplyFrame(currentAnimation.frames[currentAnimationFrame].value);
            VirtualUpdate();
        }

        protected virtual void VirtualUpdate()
        {

        }
    }
}
