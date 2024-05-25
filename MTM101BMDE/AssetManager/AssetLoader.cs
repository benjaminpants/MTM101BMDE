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
        /// <summary>
        /// Load textures from a specified folder.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="search"></param>
        /// <returns>The array of textures created using the images found.</returns>
        public static Texture2D[] TexturesFromFolder(string path, string search = "*.png")
        {
            string[] paths = Directory.GetFiles(Path.Combine(path), search);
            Texture2D[] textures = new Texture2D[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                textures[i] = AssetLoader.TextureFromFile(paths[i]);
            }
            return textures;
        }

        /// <summary>
        /// Gets textures from a specified folder, starting from the mod's StreamingAssets path.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="search"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Texture2D[] TexturesFromMod(BaseUnityPlugin plugin, string search, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plugin));
            return TexturesFromFolder(Path.Combine(pathz.ToArray()), search);
        }

        /// <summary>
        /// Convert an array of texture into sprites
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite[] ToSprites(this Texture2D[] textures, float pixelsPerUnit)
        {
            return (from tex in textures select SpriteFromTexture2D(tex, pixelsPerUnit)).ToArray();
        }

        /// <summary>
        /// Load a texture from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D TextureFromFile(string path)
        {
            return TextureFromFile(path, TextureFormat.RGBA32);
        }

        /// <summary>
        /// Load a texture from a file with the specified format.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Texture2D TextureFromFile(string path, TextureFormat format)
        {
            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(2, 2, format, false);
            ImageConversion.LoadImage(texture2D, array);
            texture2D.filterMode = FilterMode.Point;
            texture2D.name = Path.GetFileNameWithoutExtension(path);
            return texture2D;
        }

        /// <summary>
        /// Replace the image data of one texture with another.
        /// They must be the exact same size.
        /// </summary>
        /// <param name="toReplace">The texture to override the texture of.</param>
        /// <param name="replacement">The replacement texture.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Find a texture with the specified name and replace it's image data with the data of replacement.
        /// </summary>
        /// <param name="toReplace"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static bool ReplaceTexture(string toReplace, Texture2D replacement)
        {
            return ReplaceTexture(Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.name == toReplace).First(), replacement);
        }

        /// <summary>
        /// Attempt to convert a texture to another format, deleting the original.
        /// </summary>
        /// <param name="toConvert"></param>
        /// <param name="format"></param>
        /// <returns>The texture converted to the new format.</returns>
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

        /// <summary>
        /// Go through a folder and replace the image data of all textures sharing the same name of the png file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load an audio clip from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load an audio clip from a file with the specified AudioType.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Create a sprite from a Texture2D with the origin of the image being the center.
        /// </summary>
        /// <param name="tex">The texture to use.</param>
        /// <param name="pixelsPerUnit">The pixels per unit, a hallway in BB+ is 10 units.</param>
        /// <returns></returns>
        public static Sprite SpriteFromTexture2D(Texture2D tex, float pixelsPerUnit)
        {
            return SpriteFromTexture2D(tex, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        /// <summary>
        /// Convert a stream to a byte array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Create a sprite from a Texture2D.
        /// </summary>
        /// <param name="tex">The texture to use.</param>
        /// <param name="center">The pixels per unit, a hallway in BB+ is 10 units.</param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite SpriteFromTexture2D(Texture2D tex, Vector2 center, float pixelsPerUnit = 1)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), center, pixelsPerUnit);
            sprite.name = "Spr" + tex.name;
            return sprite;
        }

        static FieldInfo localizedText = AccessTools.Field(typeof(LocalizationManager), "localizedText");

        /// <summary>
        /// Load a Language folder from a non-standard place.
        /// </summary>
        /// <param name="path"></param>
        public static void LoadLanguageFolder(string path)
        {
            LangExtender.LoaderExtension.LoadFolder(path, (Dictionary<string, string>)localizedText.GetValue(Singleton<LocalizationManager>.Instance));
        }

        /// <summary>
        /// Load a Texture2D from the specified path, starting from the specified mod's mod path.
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Texture2D TextureFromMod(BaseUnityPlugin plug, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return TextureFromFile(Path.Combine(pathz.ToArray()));
        }

        /// <summary>
        /// Load an Audioclip from the specified path, starting from the specified mod's mod path.
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static AudioClip AudioClipFromMod(BaseUnityPlugin plug, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return AudioClipFromFile(Path.Combine(pathz.ToArray()));
        }

        /// <summary>
        /// Get a mod's mod path. (Currently StreamingAssets/Modded/[MOD GUID])
        /// </summary>
        /// <param name="plug"></param>
        /// <returns></returns>
        public static string GetModPath(BaseUnityPlugin plug)
        {
            return Path.Combine(Application.streamingAssetsPath, "Modded", plug.Info.Metadata.GUID);
        }

        internal static Dictionary<string, byte[]> MidiDatas = new Dictionary<string, byte[]>();
        public static Dictionary<string, byte[]> MidisToBeAdded = new Dictionary<string, byte[]>();
        /// <summary>
        /// Loads a midi file with the specified path.
        /// </summary>
        /// <param name="path">The filepath of the midi.</param>
        /// <param name="id">The ID of the midi, used as a starting point for creating the return value.</param>
        /// <returns>The string that can be used in the midi player to play the midi.</returns>
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

        /// <summary>
        /// Loads a midi file with the specified path starting from the mod path.
        /// </summary>
        /// <param name="id">The ID of the midi, used as a starting point for creating the return value.</param>
        /// <param name="plug">The modpath to get.</param>
        /// <param name="paths">The folders to go through starting from the modpath.</param>
        /// <returns>The string that can be used in the midi player to play the midi.</returns>
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

        // FlipX, FlipY, and CubemapFromTexture2D were all taken from FADE
        static Texture2D FlipX(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(width - i - 1, j, texture.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return flipped;
        }

        static Texture2D FlipY(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(i, height - j - 1, texture.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return flipped;
        }

        /// <summary>
        /// Creates a cubemap from a texture in the specified path, starting from the mod path.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Cubemap CubemapFromMod(BaseUnityPlugin plugin, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plugin));
            return CubemapFromFile(Path.Combine(pathz.ToArray()));
        }

        /// <summary>
        /// Creates a cubemap from a texture in the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Cubemap CubemapFromFile(string path)
        {
            return CubemapFromTexture(TextureFromFile(path));
        }

        /// <summary>
        /// Create a cubemap from a Texture2D.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Cubemap CubemapFromTexture(Texture2D texture)
        {
            texture = FlipX(texture);
            texture = FlipY(texture);

            int cubemapWidth = texture.width / 6;
            Cubemap cubemap = new Cubemap(cubemapWidth, TextureFormat.ARGB32, false);
            cubemap.SetPixels(texture.GetPixels(0 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeZ);
            cubemap.SetPixels(texture.GetPixels(1 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveZ);
            cubemap.SetPixels(texture.GetPixels(2 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeY);
            cubemap.SetPixels(texture.GetPixels(3 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveY);
            cubemap.SetPixels(texture.GetPixels(4 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeX);
            cubemap.SetPixels(texture.GetPixels(5 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveX);
            cubemap.Apply();
            return cubemap;
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
