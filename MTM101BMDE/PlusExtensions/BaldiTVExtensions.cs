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
    public class BaldiTVExtensionContainer : MonoBehaviour
    {
        public BaldiTVCharacter tvCharacter;
        public Image image;
    }

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
            Image baldiImage = (Image)_baldiImage.GetValue(tv);
            Image clonedImage = GameObject.Instantiate(baldiImage, MTM101BaldiDevAPI.prefabTransform, true); // Fix from AlexBW145: this method is a hack but apparently because I made it a prefab before sending it into the real game.
            clonedImage.gameObject.SetActive(true);
            GameObject.DestroyImmediate(clonedImage.GetComponent<VolumeAnimator>());
            GameObject.DestroyImmediate(clonedImage.GetComponent<Animator>());
            GameObject.DestroyImmediate(clonedImage.GetComponent<AudioSource>());
            GameObject.DestroyImmediate(clonedImage.GetComponent<AudioManager>());
            clonedImage.rectTransform.SetParent(baldiImage.transform.parent, true);
            baldiImage.enabled = false;
            clonedImage.transform.SetAsFirstSibling();
            // get a raw enumerator and pass that into the container ienumerator, and return that ienumerator
            return CharacterEnumeratorContainer(tv, clonedImage, GetRawCharacterEnumerator(id, tv, clonedImage, sound), characters[id]);
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

        private static IEnumerator CharacterEnumeratorContainer(BaldiTV tv, Image toCleanUp, IEnumerator wrap, BaldiTVCharacter character)
        {
            _ResetScreen.Invoke(tv, null);
            toCleanUp.enabled = true;
            ((AudioManager)_baldiTvAudioManager.GetValue(tv)).FlushQueue(true);
            BaldiTVExtensionContainer extContainer = tv.gameObject.AddComponent<BaldiTVExtensionContainer>();
            extContainer.tvCharacter = character;
            extContainer.image = toCleanUp;
            yield return wrap;
            _busy.SetValue(tv, false);
            _QueueCheck.Invoke(tv, null);
            character.CleanupSpeaker(toCleanUp, tv);
            GameObject.Destroy(extContainer);
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

        /// <summary>
        /// Called when the Speaker is abruptly stopped and MUST be cleaned instantly.
        /// Do not perform any animation here, that is what SpeakEnumerator is for.
        /// This should clean up the image and any thing else the speaker may have created.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="tv"></param>
        public virtual void CleanupSpeaker(Image image, BaldiTV tv)
        {
            GameObject.Destroy(image.gameObject);
        }
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

        public virtual CustomImageAnimator SetupAnimator(Image image, BaldiTV tv, AudioManager audMan, SoundObject sound)
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
            volAnim.audioSource = audMan.audioSourceManager.GetAudioSource(sound.soundType);
            volAnim.sensitivity = sensitivity;
            volAnim.volumeMultipler = volumeMultiplier;
            return anim;
        }

        public override IEnumerator SpeakEnumerator(Image image, BaldiTV tv, AudioManager audMan, SoundObject sound)
        {
            CustomImageAnimator anim = SetupAnimator(image, tv, audMan, sound);
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
