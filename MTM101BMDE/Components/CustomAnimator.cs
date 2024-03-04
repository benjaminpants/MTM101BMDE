using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace MTM101BaldAPI.Components
{
    public class CustomAnimation<T>
    {
        public CustomAnimationFrame<T>[] frames { private set; get; }

        public float animationLength { private set; get; }

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

    public interface ICustomAnimator<TType, TAnimation, TFrame> where TAnimation : CustomAnimation<TFrame>
    {
        TType affectedObject { get; }

        void Play(string name, float speed);

        void SetPause(bool paused);

        void ChangeSpeed(float speed);

        void Stop();

    }
    /// <summary>
    /// A custom sprite animator, please note that currently, this does not properly save the animations property, so if you are using this in an NPC,
    /// I advise defining the animations in its Initialization function, as putting them in the prefab won't work.
    /// </summary>
    public class CustomSpriteAnimator : MonoBehaviour, ICustomAnimator<SpriteRenderer, CustomAnimation<Sprite>, Sprite>
    {
        public SpriteRenderer affectedObject { 
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
        public Dictionary<string, CustomAnimation<Sprite>> animations = new Dictionary<string, CustomAnimation<Sprite>>();
        private float currentFrameTime = 0f;
        private float currentAnimTime = 0f;
        private int currentFrameIndex = 0;
        private string currentAnim = "";
        private string defaultAnim = "";
        private float defaultAnimSpeed = 1f;
        private bool paused = false;
        
        private CustomAnimation<Sprite> currentAnimation { 
            get
            {
                if (currentAnim == "") return null;
                return animations[currentAnim];
            }
        }

        private CustomAnimationFrame<Sprite> currentFrame
        {
            get
            {
                return currentAnimation.frames[Mathf.Min(currentFrameIndex, currentAnimation.frames.Length - 1)];
            }
        }

        private float _currentSpeed = 1f;
        public float Speed { 
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

        public void ChangeSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        public void SetPause(bool pause)
        {
            paused = pause;
        }

        public void SetDefaultAnimation(string newDef, float newDefSpeed)
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
            if (currentAnim == "") return;
            float delta = Time.deltaTime * Speed;
            currentAnimTime += delta;
            currentFrameTime += delta;
            if (currentFrameTime >= currentFrame.frameTime)
            {
                currentFrameIndex++;
                currentFrameTime = 0f;
                UpdateFrame();
            }
            if (currentAnimTime >= currentAnimation.animationLength)
            {
                Stop();
            }
        }

        private void UpdateFrame()
        {
            affectedObject.sprite = currentFrame.value;
        }

        public void Play(string name, float speed)
        {
            if (animations.ContainsKey(name))
            {
                currentAnim = name;
                currentAnimTime = 0f;
                currentFrameTime = 0f;
                currentFrameIndex = 0;
                UpdateFrame();
                return;
            }
            Debug.LogError("Attempted to play non-existant animation " + name + "!");
        }

        public void Stop()
        {
            currentAnim = defaultAnim;
            if (defaultAnim != "")
            {
                Play(defaultAnim, defaultAnimSpeed);
            }
        }
    }
}
