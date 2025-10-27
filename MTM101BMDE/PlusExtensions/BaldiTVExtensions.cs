using HarmonyLib;
using MTM101BaldAPI.Components.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.PlusExtensions
{
    public static class BaldiTVExtensionHandler
    {
        private static Dictionary<string, BaldiTVCharacter> characters = new Dictionary<string, BaldiTVCharacter>();

        /// <summary>
        /// Adds a BaldiTVCharacter with the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="character"></param>
        /// <exception cref="Exception"></exception>
        public static void AddCharacter(string id, BaldiTVCharacter character)
        {
            if (id == "baldi") throw new Exception("Can't override TVCharacterBaldi!");
            characters.Add(id, character);
        }

        private static IEnumerator GetRawCharacterEnumerator(string id, BaldiTV tv, Image sprite, SoundObject obj)
        {
            if (id == "baldi") throw new Exception("Baldi has no RawCharacterEnumerator!");
            BaldiTVCharacter tvCharacter = characters[id];
            return tvCharacter.SpeakEnumerator(sprite, tv, (AudioManager)_baldiTvAudioManager.GetValue(tv), obj);
        }

        static FieldInfo _baldiImage = AccessTools.Field(typeof(BaldiTV), "baldiImage");
        static FieldInfo _busy = AccessTools.Field(typeof(BaldiTV), "busy");
        static FieldInfo _baldiTvAudioManager = AccessTools.Field(typeof(BaldiTV), "baldiTvAudioManager");
        static MethodInfo _ResetScreen = AccessTools.Method(typeof(BaldiTV), "ResetScreen");
        static MethodInfo _QueueCheck = AccessTools.Method(typeof(BaldiTV), "QueueCheck");
        static MethodInfo _QueueEnumerator = AccessTools.Method(typeof(BaldiTV), "QueueEnumerator");
        static MethodInfo _BaldiSpeaks = AccessTools.Method(typeof(BaldiTV), "BaldiSpeaks");

        internal static IEnumerator GetCharacterEnumerator(string id, BaldiTV tv, SoundObject sound)
        {
            if (id == "baldi") return BaldiSpeaksWrapper(tv);
            // create image
            // todo: i should probably just create this instead of copying BaldiTV
            Image clonedImage = GameObject.Instantiate<Image>((Image)_baldiImage.GetValue(tv), tv.transform);
            clonedImage.name = id + "_Image";
            clonedImage.gameObject.SetActive(true);
            GameObject.Destroy(clonedImage.GetComponent<VolumeAnimator>());
            GameObject.Destroy(clonedImage.GetComponent<Animator>());
            GameObject.Destroy(clonedImage.GetComponent<AudioSource>());
            GameObject.Destroy(clonedImage.GetComponent<AudioManager>());
            clonedImage.transform.SetAsFirstSibling();
            // get a raw enumerator and pass that into the container ienumerator, and return that ienumerator
            return CharacterEnumeratorContainer(tv, clonedImage, GetRawCharacterEnumerator(id, tv, clonedImage, sound));
        }

        /// <summary>
        /// Makes the specified character speak.
        /// </summary>
        /// <param name="tv">The BaldiTV</param>
        /// <param name="id">The character id. Pass in "baldi" to make Baldi speak, pass in a null or empty string to make it automatically choose the right character.</param>
        /// <param name="sound">The SoundObject to play.</param>
        public static void QueueCharacterSpeaks(this BaldiTV tv, string id, SoundObject sound)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = GetCharacterForSoundObject(sound);
            }
            _QueueEnumerator.Invoke(tv, new object[] { GetCharacterEnumerator(id, tv, sound) });
        }

        /// <summary>
        /// A wrapper for BaldiSpeaks so that the patch doesn't override it. Used for when "baldi" is specified in QueueCharacterSpeaks.
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        private static IEnumerator BaldiSpeaksWrapper(BaldiTV tv)
        {
            yield return (IEnumerator)_BaldiSpeaks.Invoke(tv, null);
            yield break;
        }

        public static string GetCharacterForSoundObject(SoundObject obj)
        {
            foreach (var kvp in characters)
            {
                if (kvp.Value.SoundBelongsToCharacter(obj)) return kvp.Key;
            }
            return "baldi";
        }

        private static IEnumerator CharacterEnumeratorContainer(BaldiTV tv, Image toCleanUp, IEnumerator wrap)
        {
            _ResetScreen.Invoke(tv, null);
            toCleanUp.enabled = true;
            ((AudioManager)_baldiTvAudioManager.GetValue(tv)).FlushQueue(true);
            yield return wrap;
            _busy.SetValue(tv, false);
            _QueueCheck.Invoke(tv, null);
            GameObject.Destroy(toCleanUp.gameObject);
            yield break;
        }
    }

    public abstract class BaldiTVCharacter
    {
        /// <summary>
        /// Returns true if the specified sound belongs to this BaldiTVCharacter.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool SoundBelongsToCharacter(SoundObject obj);

        /// <summary>
        /// The IEnumerator that is used to handle the character speaking animation.
        /// The AudioManager hasn't started playing any audio yet, but it has been flushed.
        /// Once the IEnumerator ends the character will stop talking, and the image will be destroyed.
        /// There is no need to set busy or call tv.QueueCheck as that is handled automatically.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="tv"></param>
        /// <param name="audMan"></param>
        /// <param name="sound"></param>
        /// <returns></returns>
        public abstract IEnumerator SpeakEnumerator(Image image, BaldiTV tv, AudioManager audMan, SoundObject sound);
    }

    public class SimpleBaldiTVCharacter : BaldiTVCharacter
    {
        public List<SoundObject> sounds;
        public Sprite[] animation;
        public float volumeMultiplier = 1f;
        public AnimationCurve sensitivity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public SimpleBaldiTVCharacter(List<SoundObject> ownedSounds, Sprite[] talkFrames)
        {
            sounds = ownedSounds;
            animation = talkFrames;
        }

        public SimpleBaldiTVCharacter(List<SoundObject> ownedSounds, Sprite[] talkFrames, float volumeMultiplier) : this(ownedSounds, talkFrames)
        {
            this.volumeMultiplier = volumeMultiplier;
        }

        public SimpleBaldiTVCharacter(List<SoundObject> ownedSounds, Sprite[] talkFrames, float volumeMultiplier, AnimationCurve sensitivity) : this(ownedSounds, talkFrames, volumeMultiplier)
        {
            this.sensitivity = sensitivity;
        }

        public override bool SoundBelongsToCharacter(SoundObject obj)
        {
            return sounds.Contains(obj);
        }

        public override IEnumerator SpeakEnumerator(Image image, BaldiTV tv, AudioManager audMan, SoundObject sound)
        {
            CustomVolumeAnimator volAnim = image.gameObject.AddComponent<CustomVolumeAnimator>();
            CustomImageAnimator anim = image.gameObject.AddComponent<CustomImageAnimator>();
            volAnim.animations = new string[animation.Length];
            for (int i = 0; i < animation.Length; i++)
            {
                anim.AddAnimation(i.ToString(), new SpriteAnimation(new Sprite[1] { animation[i] }, 1f));
                volAnim.animations[i] = i.ToString();
            }
            anim.image = image;
            volAnim.animator = anim;
            volAnim.audioSource = audMan.audioDevice;
            volAnim.sensitivity = sensitivity;
            volAnim.volumeMultipler = volumeMultiplier;
            audMan.QueueAudio(sound);
            anim.Play("0", 1f);
            yield return null;
            while (audMan.QueuedAudioIsPlaying)
            {
                yield return null;
            }
            yield break;
        }
    }
}
