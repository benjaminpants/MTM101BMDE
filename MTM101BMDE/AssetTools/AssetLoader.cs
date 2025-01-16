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
using MTM101BaldAPI.OBJImporter;

namespace MTM101BaldAPI.AssetTools
{
    public static class AssetLoader
    {

        private static List<BaseUnityPlugin> queuedModsForLanguage = new List<BaseUnityPlugin>();
        private static List<(Language, string)> queuedFilesForLanguage = new List<(Language, string)>();
        private static FieldInfo _localizedText = AccessTools.Field(typeof(LocalizationManager), "localizedText");
        private static FieldInfo _currentSubLang = AccessTools.Field(typeof(LocalizationManager), "currentSubLang");
        private static List<Func<Language,Dictionary<string,string>>> queuedActionsForLanguage = new List<Func<Language, Dictionary<string, string>>>();

        internal static void LoadAllQueuedLocalization(Language language)
        {
            if (Singleton<LocalizationManager>.Instance == null) return;
            foreach (BaseUnityPlugin plugin in queuedModsForLanguage)
            {
                string rootPath = Path.Combine(GetModPath(plugin), "Language", language.ToString());
                if (!Directory.Exists(rootPath)) continue;
                string[] paths = Directory.GetFiles(rootPath, "*.json");
                foreach (string path in paths)
                {
                    LoadLocaFile(path);
                }
            }
            foreach ((Language, string) pathTuple in queuedFilesForLanguage)
            {
                if (language != pathTuple.Item1) continue;
                string path = pathTuple.Item2;
                LoadLocaFile(path);
            }
            Dictionary<string, string> localizedText = (Dictionary<string, string>)_localizedText.GetValue(LocalizationManager.Instance);
            foreach (Func<Language,Dictionary<string, string>> act in queuedActionsForLanguage)
            {
                Dictionary<string, string> output = act(language);
                foreach (KeyValuePair<string, string> kvp in output)
                {
                    localizedText[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Adds the specified file to be queued for load for the specified language.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lang"></param>
        public static void LocalizationFromFile(string path, Language lang)
        {
            queuedFilesForLanguage.Add((lang, path));
        }

        /// <summary>
        /// Adds the specified function to get called at the end of language loading.
        /// The returned dictionary is added to the localization.
        /// </summary>
        public static void LocalizationFromFunction(Func<Language, Dictionary<string, string>> funcToAdd)
        {
            queuedActionsForLanguage.Add(funcToAdd);
        }

        private static void LoadLocaFile(string path)
        {
            try
            {
                LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(path));
                Dictionary<string, string> localizedText = (Dictionary<string, string>)_localizedText.GetValue(LocalizationManager.Instance);
                foreach (LocalizationItem item in localizationData.items)
                {
                    if (localizedText.ContainsKey(item.key))
                    {
                        localizedText[item.key] = item.value;
                    }
                    else
                    {
                        localizedText.Add(item.key, item.value);
                    }
                }
                MTM101BaldiDevAPI.Log.LogDebug("Loaded " + Path.GetFileName(path) + " successfully!");
            }
            catch (Exception E)
            {
                MTM101BaldiDevAPI.Log.LogError("Given JSON for file: " + Path.GetFileName(path) + " is invalid!");
                MTM101BaldiDevAPI.Log.LogError(E.Message);
            }
        }

        /// <summary>
        /// Automatically queues the appropiate localization file from the specified mod.
        /// Use this if you are porting a mod from a version before 6.0.0.0
        /// (AKA: Put JSON files in Language/English/)
        /// </summary>
        public static void LocalizationFromMod(BaseUnityPlugin plugin)
        {
            queuedModsForLanguage.Add(plugin);
        }


        /// <summary>
        /// Loads a .obj and .mtl from the specified file.
        /// </summary>
        /// <returns>The GameObject containing the model</returns>
        public static GameObject ModelFromFile(string path)
        {
            OBJLoader objLoader = new OBJLoader();
            GameObject obj = objLoader.Load(path, MTM101BaldiDevAPI.AssetMan.Get<Material>("TileBase"));
            obj.name = Path.GetFileNameWithoutExtension(path);
            return obj;
        }

        /// <summary>
        /// Loads an .obj file with the manually specified materials
        /// </summary>
        /// <returns>The GameObject containing the model</returns>
        public static GameObject ModelFromFileManualMaterials(string path, Dictionary<string, Material> materials)
        {
            OBJLoader objLoader = new OBJLoader();
            GameObject obj = objLoader.LoadWithManualMaterials(path, materials, MTM101BaldiDevAPI.AssetMan.Get<Material>("TileBase"));
            obj.name = Path.GetFileNameWithoutExtension(path);
            return obj;
        }

        public static GameObject ModelFromMod(BaseUnityPlugin plugin, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plugin));
            return ModelFromFile(Path.Combine(pathz.ToArray()));
        }

        public static GameObject ModelFromModManualMaterials(BaseUnityPlugin plugin, Dictionary<string, Material> materials, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plugin));
            return ModelFromFileManualMaterials(Path.Combine(pathz.ToArray()), materials);
        }


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
        /// Makes a readable copy of the specified Texture2D
        /// </summary>
        /// <param name="source"></param>
        /// <param name="apply">If the changed texture should be uploaded to the GPU.</param>
        /// <returns></returns>
        public static Texture2D MakeReadableCopy(this Texture2D source, bool apply)
        {
            Texture2D toDump = source;
            toDump = new Texture2D(toDump.width, toDump.height, toDump.format, toDump.mipmapCount > 1);
            toDump.name = source.name;
            toDump.filterMode = source.filterMode;

            if (source.isReadable) // If it's already readable, just copy the pixels from the source to the copy
            {
                toDump.SetPixels(source.GetPixels());
            }
            else
            {
                RenderTexture dummyTex = RenderTexture.GetTemporary(source.width, source.height, 24);
                Graphics.Blit(source, dummyTex);
                dummyTex.filterMode = source.filterMode;

                toDump.ReadPixels(new Rect(0, 0, dummyTex.width, dummyTex.height), 0, 0);

                RenderTexture.ReleaseTemporary(dummyTex);
            }

            if (apply)
                toDump.Apply();

            toDump.name = source.name + "_Readable";
            return toDump;
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
                Texture2D targetTex = foundTextures.First(z => z.name == targetName);
                if (targetTex == null)
                {
                    allSucceeded = false;
                    throw new KeyNotFoundException("Unable to find texture with name: " + targetTex + "!");
                }
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
            Path.Combine("File:///",""),
            Path.Combine("File://","")
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
        /// Generates sprites from the specified spritesheet.
        /// </summary>
        /// <param name="horizontalTiles">How many tiles this spritesheet has horizontally.</param>
        /// <param name="verticalTiles">How many tiles this spritesheet has vertically.</param>
        /// <param name="pixelsPerUnit">The pixels per unit for the created sprites.</param>
        /// <param name="center">The center for the created sprites.</param>
        /// <param name="tex">The texture to use.</param>
        /// <returns>The collection of sprites.</returns>
        public static Sprite[] SpritesFromSpritesheet(int horizontalTiles, int verticalTiles, float pixelsPerUnit, Vector2 center, Texture2D tex)
        {
            int estimatedXsize = tex.width / horizontalTiles;
            int estimatedYsize = tex.height / verticalTiles;

            Sprite[] sprs = new Sprite[horizontalTiles * verticalTiles];
            int i = 0;
            for (int y = verticalTiles - 1; y >= 0; y--)
            {
                for (int x = 0; x < horizontalTiles; x++)
                {
                    Sprite spr = Sprite.Create(tex, new Rect(x * estimatedXsize, y * estimatedYsize, estimatedXsize, estimatedYsize), center, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    spr.name = tex.name + x + "_" + y;
                    sprs[i++] = spr;
                }
            }

            return sprs;
        }

        /// <summary>
        /// Generates sprites from the specified spritesheet.
        /// </summary>
        /// <param name="horizontalTiles">How many tiles this spritesheet has horizontally.</param>
        /// <param name="verticalTiles">How many tiles this spritesheet has vertically.</param>
        /// <param name="pixelsPerUnit">The pixels per unit for the created sprites.</param>
        /// <param name="center">The center for the created sprites.</param>
        /// <param name="tex">The texture to use.</param>
        /// <returns>The collection of sprites as a 2D array</returns>
        public static Sprite[,] SpritesFromSpritesheet2D(int horizontalTiles, int verticalTiles, float pixelsPerUnit, Vector2 center, Texture2D tex)
        {
            int estimatedXsize = tex.width / horizontalTiles;
            int estimatedYsize = tex.height / verticalTiles;

            Sprite[,] sprs = new Sprite[horizontalTiles,verticalTiles];
            for (int y = verticalTiles - 1; y >= 0; y--)
            {
                for (int x = 0; x < horizontalTiles; x++)
                {
                    Sprite spr = Sprite.Create(tex, new Rect(x * estimatedXsize, y * estimatedYsize, estimatedXsize, estimatedYsize), center, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    spr.name = tex.name + x + "_" + y;
                    sprs[x,y] = spr;
                }
            }

            return sprs;
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
        /// <param name="center">The center of the sprite, where 0,0 is the top left and 1,1 is the bottom right.</param>
        /// <param name="pixelsPerUnit">The pixels per unit, a hallway in BB+ is 10 units.</param>
        /// <returns></returns>
        public static Sprite SpriteFromTexture2D(Texture2D tex, Vector2 center, float pixelsPerUnit = 1)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), center, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = "Spr" + tex.name;
            return sprite;
        }

        /// <summary>
        /// Creates a sprite from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to load the texture from.</param>
        /// <param name="center">The center of the sprite, where 0,0 is the top left and 1,1 is the bottom right.</param>
        /// <param name="pixelsPerUnit">The pixels per unit, a hallway in BB+ is 10 units.</param>
        /// <returns></returns>
        public static Sprite SpriteFromFile(string filePath, Vector2 center, float pixelsPerUnit = 1)
        {
            return SpriteFromTexture2D(TextureFromFile(filePath), center, pixelsPerUnit);
        }

        /// <summary>
        /// Load a Sprite from the specified path, starting from the specified mod's mod path.
        /// </summary>
        /// <param name="plug">The mod to load the sprite from</param>
        /// <param name="paths">The additional paths to load</param>
        /// <param name="pixelsPerUnit">The pixels per unit, a hallway in BB+ is 10 units.</param>
        /// <param name="center">The center of the sprite, where 0,0 is the top left and 1,1 is the bottom right.</param>
        /// <returns></returns>
        public static Sprite SpriteFromMod(BaseUnityPlugin plug, Vector2 center, float pixelsPerUnit, params string[] paths)
        {
            List<string> pathz = paths.ToList();
            pathz.Insert(0, GetModPath(plug));
            return SpriteFromFile(Path.Combine(pathz.ToArray()), center, pixelsPerUnit);
        }

        /// <summary>
        /// Load a Language folder from a non-standard place.
        /// </summary>
        /// <param name="rootPath"></param>
        [Obsolete("Use LoadLocalizationFolder instead!")]
        public static void LoadLanguageFolder(string rootPath)
        {
            string[] paths = Directory.GetFiles(rootPath, "*.json");
            foreach (string path in paths)
            {
                queuedFilesForLanguage.Add((Language.English, path));
            }
        }

        /// <summary>
        /// Queues all the files in the specified localization folder to be loaded.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="lang"></param>
        public static void LoadLocalizationFolder(string rootPath, Language lang)
        {
            string[] paths = Directory.GetFiles(rootPath, "*.json");
            foreach (string path in paths)
            {
                queuedFilesForLanguage.Add((lang, path));
            }
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
        /// Unloads the specified custom midi of the specified ID. This CANNOT unload vanilla midis.
        /// </summary>
        /// <param name="id"></param>
        public static void UnloadCustomMidi(string id)
        {
            if (MidiDatas.ContainsKey(id))
            {
                MidiDatas.Remove(id);
                return;
            }
            if (MidisToBeAdded == null) return;
            if (MidisToBeAdded.ContainsKey(id))
            {
                MidisToBeAdded.Remove(id);
            }
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

        // FlipX, FlipY, and CubemapFromTextureLegacy were adapted from FADE

        /// <summary>
        /// Flips the specified Texture2D horizontally.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="apply">If the changed texture should be uploaded to the GPU.</param>
        /// <returns></returns>
        public static void FlipX(this Texture2D source, bool apply)
        {
            Color[] pixels = source.GetPixels();
            int width = source.width, height = source.height;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    source.SetPixel(i, height - j - 1, pixels[j * width + i]);

            if (apply)
                source.Apply();
        }

        /// <summary>
        /// Flips the specified Texture2D vertically.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="apply">If the changed texture should be uploaded to the GPU.</param>
        /// <returns></returns>
        public static void FlipY(this Texture2D source, bool apply)
        {
            Color[] pixels = source.GetPixels();
            int width = source.width, height = source.height;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    source.SetPixel(i, height - j - 1, pixels[j*width+i]);

            if (apply)
                source.Apply();
        }

        /// <summary>
        /// Rotate specified Texture2D by 180 degrees (flip both horizontally and vertically).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="apply">If the changed texture should be uploaded to the GPU.</param>
        /// <returns></returns>
        public static void Rotate180(this Texture2D source, bool apply)
        {
            Color[] pixels = source.GetPixels();
            int pixelCount = pixels.Length;
            Color[] newPixels = new Color[pixelCount];
            pixelCount--;

            for (int i = 0; pixelCount >= 0; i++, pixelCount--)
                newPixels[i] = pixels[pixelCount];

            source.SetPixels(newPixels);

            if (apply)
                source.Apply();
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
            // Convert from FADE layout if texture's aspect ratio is 6x1, will likely be removed later on
            if (texture.width/texture.height == 6)
                return CubemapFromTextureLegacy(texture);

            // MTM101API layout
            int width = texture.width / 4;
            texture.Rotate180(false);

            Cubemap cubemap = new Cubemap(width, TextureFormat.RGB24, false);
            cubemap.name = texture.name;
            cubemap.SetPixels(texture.GetPixels(0, width, width, width), CubemapFace.NegativeZ);
            cubemap.SetPixels(texture.GetPixels(width, width, width, width), CubemapFace.NegativeX);
            cubemap.SetPixels(texture.GetPixels(width * 2, width, width, width), CubemapFace.PositiveZ);
            cubemap.SetPixels(texture.GetPixels(width * 3, width, width, width), CubemapFace.PositiveX);

            cubemap.SetPixels(texture.GetPixels(width, 0, width, width), CubemapFace.NegativeY);
            cubemap.SetPixels(texture.GetPixels(width * 2, 0, width, width), CubemapFace.PositiveY);
            cubemap.Apply();

            // Rotate texture back to normal
            texture.Rotate180(false);

            return cubemap;
        }

        static Cubemap CubemapFromTextureLegacy(Texture2D texture)
        {
            int width = texture.width / 6;
            texture.Rotate180(false);

            Cubemap cubemap = new Cubemap(width, TextureFormat.RGB24, false);
            cubemap.name = texture.name;
            cubemap.SetPixels(texture.GetPixels(0, 0, width, width), CubemapFace.NegativeZ);
            cubemap.SetPixels(texture.GetPixels(width, 0, width, width), CubemapFace.PositiveZ);
            cubemap.SetPixels(texture.GetPixels(width * 2, 0, width, width), CubemapFace.NegativeY);
            cubemap.SetPixels(texture.GetPixels(width * 3, 0, width, width), CubemapFace.PositiveY);
            cubemap.SetPixels(texture.GetPixels(width * 4, 0, width, width), CubemapFace.NegativeX);
            cubemap.SetPixels(texture.GetPixels(width * 5, 0, width, width), CubemapFace.PositiveX);

            texture.Rotate180(false);
            return cubemap;
        }

        private static Color[] _cubemapClearBlock;
        private static int _cubemapWidth = -1;

        /// <summary>
        /// Unwraps a cubemap and outputs it as a decompressed Texture2D.
        /// </summary>
        /// <param name="cubemap"></param>
        /// <returns></returns>
        public static Texture2D TextureFromCubemap(Cubemap cubemap)
        {
            Texture2D unwrapped = new Texture2D(cubemap.width * 4, cubemap.width * 2, cubemap.format, false);
            int width = cubemap.width;

            // Create a block of transparent pixels to be used later
            if (_cubemapWidth < width)
            {
                _cubemapWidth = width;
                _cubemapClearBlock = new Color[width * width];
            }

            // Copy cubemap faces accordingly
            Graphics.CopyTexture(cubemap, 5, 0, 0, 0, width, width, unwrapped, 0, 0, 0, width); // -Z
            Graphics.CopyTexture(cubemap, 1, 0, 0, 0, width, width, unwrapped, 0, 0, width, width); // -X
            Graphics.CopyTexture(cubemap, 4, 0, 0, 0, width, width, unwrapped, 0, 0, width * 2, width); // +Z
            Graphics.CopyTexture(cubemap, 0, 0, 0, 0, width, width, unwrapped, 0, 0, width * 3, width); // +X
            Graphics.CopyTexture(cubemap, 3, 0, 0, 0, width, width, unwrapped, 0, 0, width, 0); // -Y
            Graphics.CopyTexture(cubemap, 2, 0, 0, 0, width, width, unwrapped, 0, 0, width * 2, 0); // +Y

            // Store currently active Render Texture so it can be reactivated after this process
            RenderTexture lastActive = RenderTexture.active;
            RenderTexture dummyTexture = RenderTexture.GetTemporary(unwrapped.width, unwrapped.height, 0, RenderTextureFormat.ARGB32);

            unwrapped.filterMode = FilterMode.Point;
            dummyTexture.filterMode = FilterMode.Point;

            // Blit unwrapped cubemap into the new Render Texture
            Graphics.Blit(unwrapped, dummyTexture);

            // Make the new temporary Render Texture active and copy its pixel data to a new Texture2D
            RenderTexture.active = dummyTexture;
            Texture2D output = new Texture2D(unwrapped.width, unwrapped.height, TextureFormat.ARGB32, false);
            output.ReadPixels(new Rect(0, 0, unwrapped.width, unwrapped.height), 0, 0);

            // Set remainder pixels to transparency
            output.SetPixels(0, 0, width, width, _cubemapClearBlock);
            output.SetPixels(width*3, 0, width,width, _cubemapClearBlock);

            output.Rotate180(true); // Rotate texture by 180 degrees and upload it to the GPU

            output.name = $"{cubemap.name}_Unwrapped";

            UnityEngine.Object.DestroyImmediate(unwrapped);

            // Set active Render Texture back to what it was and release the temporary one
            RenderTexture.active = lastActive;
            RenderTexture.ReleaseTemporary(dummyTexture);
            return output;
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
