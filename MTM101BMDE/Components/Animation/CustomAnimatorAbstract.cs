using System;
using System.Collections.Generic;
using UnityEngine;

namespace MTM101BaldAPI.Components.Animation
{
    public interface ISimpleAnimator
    {
        void Play(string animation, float speed);
    }


    // ########## STUB CLASS TO TRICK THE MODS INTO THINKING THIS IS THE REAL GENERIC ANIMATOR CLASS #########
    [Obsolete("Stub class. Use CustomAnimator instead.", true)]
    public abstract class CustomAnimator<AnimationType, Frame, UnderlyingType> : MonoBehaviour where AnimationType : CustomAnimation, new() where Frame : CustomAnimationFrame, new()
    {
        // LIMITATIONS FROM THIS APPROACH:
        // The class literally cannot have a body for fields or any getters. Unity crashes if so.
        // No method should call the other inside the class; everything is isolated. Otherwise, Unity crashes.
        // Everything needs to be type-safe, since we don't know what's the type being accepted here.

        public void LoadAnimations(Dictionary<string, AnimationType> animations)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                if (animations is Dictionary<string, CustomAnimation> safeAnimations)
                {
                    _customAnimator.LoadAnimations(safeAnimations);
                }
            }
        }

        public void AddAnimation(string key, AnimationType anim)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.AddAnimation(key, anim);
            }
        }

        public void ApplyFrame(UnderlyingType frame)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.ApplyFrame(frame);
            }
        }

        public void Play(string id, float speed, bool loop)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.Play(id, speed, loop);
            }
        }

        public void Play(string id, float speed)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.Play(id, speed);
            }
        }

        public void Stop()
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.Stop();
            }
        }

        public void SetPause(bool paused)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.SetPause(paused);
            }
        }

        public void SetLoop(bool loop)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.SetLoop(loop);
            }
        }

        public void SetDefaultAnimation(string animation, float speed)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.SetDefaultAnimation(animation, speed);
            }
        }

        public void SetDefaultAnimation(string animation, float speed, bool play)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.SetDefaultAnimation(animation, speed, play);
            }
        }

        public virtual void ChangeSpeed(float speed)
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                _customAnimator.ChangeSpeed(speed);
            }
        }

        public string AnimationId
        {
            get
            {
                if (TryGetComponent<CustomAnimator>(out var _customAnimator))
                    return _customAnimator.AnimationId;
                return string.Empty;
            }
        }

        public int AnimationFrame
        {
            get
            {
                if (TryGetComponent<CustomAnimator>(out var _customAnimator))
                    return _customAnimator.AnimationFrame;
                return -1;
            }
        }

        public virtual float AnimationSpeed
        {
            get
            {
                if (TryGetComponent<CustomAnimator>(out var _customAnimator))
                    return _customAnimator.AnimationSpeed;
                return 1f;
            }
        }

        public TimeScaleType timeScale = TimeScaleType.Environment;
        public bool useScaledTime = true;
        public EnvironmentController ec;
        public virtual float GetTimeScale()
        {
            if (TryGetComponent<CustomAnimator>(out var _customAnimator))
            {
                _customAnimator.timeScale = timeScale;
                _customAnimator.ec = ec;
                _customAnimator.useScaledTime = useScaledTime;
                return _customAnimator.GetTimeScale();
            }
            return 1f;
        }
    }

    [Serializable]
    public abstract class CustomAnimationFrame
    {
        public float time;

        // This allows the Animator to access the value without knowing the type
        // It's an object because SpriteArrayFrame exists and it's an array, not something assignable from UnityEngine.Object
        public abstract object RawValue { get; }

        public CustomAnimationFrame() { time = 0f; }
        public CustomAnimationFrame(float time) { this.time = time; }
    }

    [Serializable]
    public abstract class CustomAnimation
    {
        public float animationLength;

        // Abstract method to get frames so the animator can iterate over them
        public abstract CustomAnimationFrame[] GetBaseFrames();

        protected float CalculateLength(CustomAnimationFrame[] frames)
        {
            float length = 0;
            for (int i = 0; i < frames.Length; i++) length += frames[i].time;
            return length;
        }
    }


    // ###### REAL NON-GENERIC CLASS ######
    public abstract class CustomAnimator : MonoBehaviour, ISimpleAnimator
    {
        // Dictionary now uses the concrete Non-Generic CustomAnimation class (and it's serialized!!!)
        [SerializeField]
        protected Dictionary<string, CustomAnimation> animations = new Dictionary<string, CustomAnimation>();

        protected CustomAnimation currentAnimation;
        protected int currentAnimationFrame = 0;
        protected float currentAnimationTime = 0f;

        [SerializeField]
        protected string currentAnimationId = null;
        [SerializeField]
        protected bool looping = false;
        [SerializeField]
        protected float currentSpeed = 1f;

        protected bool paused = false;

        [SerializeField]
        protected string defaultAnimation = null;

        [SerializeField]
        protected float defaultSpeed = 1f;

        public string AnimationId => currentAnimationId;
        public int AnimationFrame => currentAnimationFrame;

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
                case TimeScaleType.Null: return 1f;
                case TimeScaleType.Npc: return ec.NpcTimeScale;
                case TimeScaleType.Player: return ec.PlayerTimeScale;
                case TimeScaleType.Environment: return ec.EnvironmentTimeScale;
            }
            return 1f;
        }

        public void LoadAnimations(Dictionary<string, CustomAnimation> animations)
        {
            this.animations = new Dictionary<string, CustomAnimation>(animations);
        }

        public void AddAnimation(string key, CustomAnimation anim)
        {
            if (!animations.ContainsKey(key))
                animations.Add(key, anim);
        }

        // Abstract method now takes UnityEngine.Object
        public abstract void ApplyFrame(object frame);

        public void Play(string id, float speed, bool loop)
        {
            if (speed < 0f) throw new InvalidOperationException("Attempted to play: " + id + " with invalid speed " + speed + " in custom animator!");
            looping = loop;
            currentAnimationId = id;
            currentAnimationTime = 0f;
            currentAnimationFrame = 0;

            if (string.IsNullOrEmpty(currentAnimationId) || !animations.ContainsKey(currentAnimationId))
            {
                currentAnimationId = null;
                currentAnimation = null;
            }
            else
            {
                currentAnimation = animations[currentAnimationId];
            }
            ChangeSpeed(speed);

            // Immediate update to show first frame
            if (currentAnimation != null)
            {
                CustomAnimationFrame[] frames = currentAnimation.GetBaseFrames();
                if (frames.Length != 0)
                    ApplyFrame(frames[0].RawValue);
            }
        }

        public void Play(string id, float speed)
        {
            Play(id, speed, false);
        }

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

        protected virtual void OnAnimationFinished() { }

        public void SetDefaultAnimation(string animation, float speed)
        {
            SetDefaultAnimation(animation, speed, false);
        }

        public void SetDefaultAnimation(string animation, float speed, bool play)
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

        private void Awake()
        {
            VirtualAwake();
            if (defaultAnimation != null)
            {
                Play(defaultAnimation, defaultSpeed, true);
            }
        }

        protected virtual void VirtualAwake() { }

        void Update()
        {
            if (currentAnimation == null) { VirtualUpdate(); return; }

            var frames = currentAnimation.GetBaseFrames();

            // Skip if the animation or frame is invalid
            if (animations.Count == 0 || paused) { VirtualUpdate(); return; }
            if (frames == null || frames.Length == 0) { VirtualUpdate(); return; }

            currentAnimationTime += Time.unscaledDeltaTime * AnimationSpeed;


            while (currentAnimationTime >= frames[currentAnimationFrame].time)
            {
                currentAnimationTime = Mathf.Max(0f, currentAnimationTime - frames[currentAnimationFrame].time);
                currentAnimationFrame++;

                if (currentAnimationFrame >= frames.Length)
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

            // Update the frame at the end
            ApplyFrame(frames[currentAnimationFrame].RawValue);

            VirtualUpdate();
        }

        protected virtual void VirtualUpdate() { }
    }
}