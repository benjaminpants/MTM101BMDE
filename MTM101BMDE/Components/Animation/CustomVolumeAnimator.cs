using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components.Animation
{
    public class CustomVolumeAnimator : MonoBehaviour
    {
        private void Update()
        {
            if (audioSource == null) return;
            if (audioSource.isPlaying)
            {
                if (audioSource.clip != currentClip)
                {
                    currentClip = audioSource.clip;
                    clipData = new float[currentClip.samples * currentClip.channels];
                    currentClip.GetData(clipData, 0);
                    lastSample = 0;
                    sampleBuffer = Mathf.RoundToInt(currentClip.samples / currentClip.length * bufferTime);
                }
                volume = 0f;
                int num = Mathf.Max(lastSample - sampleBuffer, 0);
                while (num < audioSource.timeSamples * currentClip.channels && num < clipData.Length)
                {
                    if (sensitivity == null)
                    {
                        potentialVolume = Mathf.Abs(clipData[num]);
                    }
                    else
                    {
                        potentialVolume = sensitivity.Evaluate(Mathf.Abs(clipData[num]));
                    }
                    if (potentialVolume > volume)
                    {
                        volume = potentialVolume;
                    }
                    num++;
                }
                lastSample = audioSource.timeSamples * currentClip.channels;
                animator.Play(animations[Mathf.RoundToInt(Mathf.Clamp(volume * volumeMultipler, 0f, 1f) * (animations.Length - 1))], 1f);
                wasPlayingLastFrame = true;
                return;
            }
            if (wasPlayingLastFrame)
            {
                wasPlayingLastFrame = false;
                animator.Play(animations[0], 1f);
            }
        }

        public string[] animations;

        private bool wasPlayingLastFrame = false;

        public float volumeMultipler = 3f;

        /// <summary>
        /// An optional animation curve to control the sensitivity.
        /// </summary>
        public AnimationCurve sensitivity;

        [SerializeField]
        private Component _animator;

        public ISimpleAnimator animator
        {
            get
            {
                return (ISimpleAnimator)_animator;
            }
            set
            {
                _animator = (Component)value;
            }
        }

        public AudioSource audioSource;

        private AudioClip currentClip;

        public float bufferTime = 0.1f;

        private float[] clipData;

        private float volume;

        private float potentialVolume;

        private int lastSample;

        private int sampleBuffer;
    }
}
