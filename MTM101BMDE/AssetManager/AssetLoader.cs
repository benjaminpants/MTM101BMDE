using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using BepInEx;
using System.Linq;
using MidiPlayerTK;
using MPTK.NAudio.Midi;
using HarmonyLib;
using MEC;
using System.Reflection;

namespace MTM101BaldAPI.AssetTools
{
    public static class AssetLoader
    {

        public static Texture2D TextureFromFile(string path)
        {
            return TextureFromFile(path, TextureFormat.RGBA32);
        }

        public static Texture2D TextureFromFile(string path, TextureFormat format)
        {
            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(2, 2, format, false);
            ImageConversion.LoadImage(texture2D, array);
            texture2D.filterMode = FilterMode.Point;
            texture2D.name = Path.GetFileNameWithoutExtension(path);
            return texture2D;
        }

        public static bool ReplaceTexture(Texture2D toReplace, Texture2D replacement)
        {
            if (toReplace == null)
            {
                MTM101BaldiDevAPI.Log.LogError("toReplace is null!");
                return false;
            }
            if (replacement == null)
            {
                MTM101BaldiDevAPI.Log.LogError("replacement is null!");
                return false;
            }
            if ((toReplace.width != replacement.width) || (toReplace.height != replacement.height))
            {
                MTM101BaldiDevAPI.Log.LogWarning(String.Format("{0}({1}) and {2}({3}) have mismatched sizes!", toReplace.name, new Vector2Int(toReplace.width, toReplace.height).ToString(), replacement.name, new Vector2Int(replacement.width, replacement.height).ToString()));
                return false;
            }
            if (toReplace.format != replacement.format)
            {
                MTM101BaldiDevAPI.Log.LogWarning(String.Format("{0}({1}) and {2}({3}) have mismatched formats!", toReplace.name, toReplace.format.ToString(), replacement.name, replacement.format.ToString()));
                return false;
            }
            Graphics.CopyTexture(replacement, toReplace);
            return true;
        }

        public static bool ReplaceTexture(string toReplace, Texture2D replacement)
        {
            return ReplaceTexture(Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.name == toReplace).First(), replacement);
        }

        public static Texture2D AttemptConvertTo(Texture2D toConvert, TextureFormat format)
        {
            if (toConvert.format == format) return toConvert;
            Texture2D n = new Texture2D(toConvert.width, toConvert.height, format, false);
            n.SetPixels(toConvert.GetPixels());
            n.Apply();
            n.name = toConvert.name;
            UnityEngine.Object.DestroyImmediate(toConvert); //bye bye!
            return n;
        }

        internal static Texture2D ConvertToRGBA32(Texture2D toConvert)
        {
            return AttemptConvertTo(toConvert, TextureFormat.RGBA32);
        }

        public static bool ReplaceAllTexturesFromFolder(string path)
        {
            Texture2D[] foundTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
            string[] files = Directory.GetFiles(path);
            bool allSucceeded = true;
            files.Do(x =>
            {
                if (Path.GetExtension(x) != ".png")
                {
                    MTM101BaldiDevAPI.Log.LogWarning("Found non PNG while attempting bulk replace: " + x);
                    return;
                }
                string targetName = Path.GetFileNameWithoutExtension(x);
                Texture2D targetTex = foundTextures.Where(z => z.name == targetName).First();
                Texture2D replacement = AssetLoader.TextureFromFile(x, targetTex.format);
                replacement = AttemptConvertTo(replacement, targetTex.format);
                replacement.name = replacement.name + "_REPLACEMENT";
                ReplaceTexture(targetTex, replacement);
            });
            return allSucceeded;
        }

        internal static Dictionary<AudioType, string[]> audioExtensions = new Dictionary<AudioType, string[]>
        {
            { AudioType.MPEG, new string[] { "mp3", "mp2" } }, // Unsure if Unity supports MPEG, but it's there ig
            { AudioType.OGGVORBIS, new string[] { "ogg" } },
            { AudioType.WAV, new string[] { "wav" } },
            { AudioType.AIFF, new string[] { "aif", "aiff" } },
            { AudioType.MOD, new string[] { "mod" } },
            { AudioType.IT, new string[] { "it" } },
            { AudioType.S3M, new string[] { "s3m" } },
            { AudioType.XM, new string[] { "xm" } },
            { AudioType.XMA, new string[] { "xma" } }
        };
        
        public static AudioType GetAudioType(string path)
        {
            string extension = Path.GetExtension(path).ToLower().Remove(0, 1).Trim(); // Remove the period provided by default

            foreach (AudioType target in audioExtensions.Keys)
            {
                if (audioExtensions[target].Contains(extension))
                {
                    return target;
                }
            }

            throw new NotImplementedException("Unknown audio file type:" + extension + "!");
        }

        public static AudioClip AudioClipFromFile(string path)
        {
            if (MTM101BaldiDevAPI.Instance == null)
            {
                MTM101BaldiDevAPI.Log.LogWarning("useOldAudioLoad not working properly, todo: FIX! For now, HACK HACK HACK!"); //this message doesnt even work lol
                return AudioClipFromFile(path, GetAudioType(path));
            }
            if (MTM101BaldiDevAPI.Instance.useOldAudioLoad.Value) return AudioClipFromFileLegacy(path);
            return AudioClipFromFile(path, GetAudioType(path));
        }

        private static string[] fallbacks = new string[]
        {
            "",
            "file://",
            "file:///",
            Path.Combine("File:///","")
        };

        public static AudioClip AudioClipFromFile(string path, AudioType type)
        {
            AudioClip clip;
            UnityWebRequest audioClip;
            string errorMessage = "";

            foreach (string fallback in fallbacks)
            {
                using (audioClip = UnityWebRequestMultimedia.GetAudioClip(fallback + path, type))
                {
                    audioClip.SendWebRequest();
                    while (!audioClip.isDone) { }

                    if (audioClip.result != UnityWebRequest.Result.Success)
                    {
                        errorMessage = audioClip.responseCode.ToString() + ": " + audioClip.error;
                        continue;
                    }

                    clip = DownloadHandlerAudioClip.GetContent(audioClip);
                    clip.name = Path.GetFileNameWithoutExtension(path);
                    return clip;
                }
            }

            throw new Exception(errorMessage);
        }

        private static AudioClip AudioClipFromFileLegacy(string path)
        {
            AudioType typeToUse;
            string fileType = Path.GetExtension(path).ToLower().Remove(0, 1).Trim(); //what the fuck WHY DOES GET EXTENSION ADD THE FUCKING PERIOD.
            switch (fileType)
            {
                case "mp2":
                case "mp3":
                    typeToUse = AudioType.MPEG;
                    break;
                case "wav":
                    typeToUse = AudioType.WAV;
                    break;
                case "ogg":
                    typeToUse = AudioType.OGGVORBIS;
                    break;
                case "aiff":
                    typeToUse = AudioType.AIFF;
                    break;
                default:
                    throw new NotImplementedException("Unknown audio file type:" + fileType + "!");
            }
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("File:///", path), typeToUse);
            request.SendWebRequest();
            while (!request.isDone) { };
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = Path.GetFileNameWithoutExtension(path);
            return clip;
        }

        [Obsolete("Please use SpriteFromTexture2D(Texture2D tex, float pixelsPerUnit) instead!")]
        public static Sprite SpriteFromTexture2D(Texture2D tex)
        {
            return SpriteFromTexture2D(tex, new Vector2(0.5f, 0.5f));
        }

        public static Sprite SpriteFromTexture2D(Texture2D tex, float pixelsPerUnit)
        {
            return SpriteFromTexture2D(tex, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            if (!stream.CanRead) throw new InvalidOperationException("Can't convert stream that we can't read!");
            byte[] bytes = new byte[stream.Length];
            long oldPos = stream.Position;
            stream.Position = 0;
            stream.Read(bytes, 0, bytes.Length);
            stream.Position = oldPos;
            return bytes;
        }

        public static Sprite SpriteFromTexture2D(Texture2D tex, Vector2 center, float pixelsPerUnit = 1)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), center, pixelsPerUnit);
            sprite.name = "Spr" + tex.name;
            return sprite;
        }

        static FieldInfo localizedText = AccessTools.Field(typeof(LocalizationManager), "localizedText");
        public static void LoadLanguageFolder(string path)
        {
            LangExtender.LoaderExtension.LoadFolder(path, (Dictionary<string, string>)localizedText.GetValue(Singleton<LocalizationManager>.Instance));
        }

        public static Texture2D TextureFromMod(BaseUnityPlugin plug, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return TextureFromFile(Path.Combine(pathz.ToArray()));
        }

        public static AudioClip AudioClipFromMod(BaseUnityPlugin plug, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return AudioClipFromFile(Path.Combine(pathz.ToArray()));
        }

        public static string GetModPath(BaseUnityPlugin plug)
        {
            return Path.Combine(Application.streamingAssetsPath, "Modded", plug.Info.Metadata.GUID);
        }

        /// <summary>
        /// Creates a midi from a file. It returns the id assigned to the midi, which is an altered version of the id you pass to avoid conflicts.
        /// </summary>

        internal static Dictionary<string, byte[]> MidiDatas = new Dictionary<string, byte[]>();
        public static Dictionary<string, byte[]> MidisToBeAdded = new Dictionary<string, byte[]>();
        public static string MidiFromFile(string path, string id)
        {
            string idToUse = "custom_" + id;
            while (MidiDatas.ContainsKey(idToUse))
            {
                idToUse += "_";
            }
            MidiFromBytes(idToUse, File.ReadAllBytes(path));
            return idToUse;
        }

        public static string MidiFromMod(string id, BaseUnityPlugin plug, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return MidiFromFile(Path.Combine(pathz.ToArray()), id);
        }

        internal static void MidiFromBytes(string id, byte[] data)
        {
            if (MidiPlayerGlobal.Instance == null)
            {
                //throw new Exception("Attempted to add Midi before MidiPlayerGlobal is created!");
#if DEBUG
                MTM101BaldiDevAPI.Log.LogInfo(String.Format("Midi with ID: {0} has been added to the midi queue.", id));
#endif
                MidisToBeAdded.Add(id, data);
                return;
            }
            MTM101BaldiDevAPI.Log.LogInfo("Adding midi with ID: " + id);
            if (!MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Contains(id))
            {
                MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Add(id);
                MidiPlayerGlobal.BuildMidiList();
                MidiDatas.Add(id, data);
            }
        }
    }

    [HarmonyPatch(typeof(MidiFilePlayer))]
    [HarmonyPatch("MPTK_Play", new Type[0] { })]
    static class MusicPrefix
    {
        private static bool Prefix(MidiFilePlayer __instance)
        {
            byte[] data = AssetLoader.MidiDatas.GetValueSafe(__instance.MPTK_MidiName);
            if (data == null || data.Length == 0)
            {
                //we don't need to do anything here
                return true;
            }

            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (__instance.MPTK_IsPaused)
                    {
                        __instance.MPTK_UnPause();
                    }
                    else if (!__instance.MPTK_IsPlaying)
                    {
                        __instance.MPTK_InitSynth(16);
                        __instance.MPTK_StartSequencerMidi();
                        if (__instance.MPTK_CorePlayer)
                        {
                            Routine.RunCoroutine(__instance.ThreadCorePlay(data, 0f, 0f).CancelWith(__instance.gameObject), Segment.RealtimeUpdate);
                        }
                        else
                        {
                            Routine.RunCoroutine(__instance.ThreadLegacyPlay(data, 0f, 0f).CancelWith(__instance.gameObject), Segment.RealtimeUpdate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return false;
        }
    }

}
