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

namespace MTM101BaldAPI.AssetManager
{
    public static class AssetManager
    {

		public static Texture2D TextureFromFile(string path)
		{
			byte[] array = File.ReadAllBytes(path);
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			ImageConversion.LoadImage(texture2D, array);
			texture2D.filterMode = FilterMode.Point;
			return texture2D;
		}

		public static AudioClip AudioClipFromFile(string path)
        {
			AudioType typeToUse = AudioType.UNKNOWN;
			string fileType = Path.GetExtension(path).ToLower().Remove(0,1).Trim(); //what the fuck WHY DOES GET EXTENSION ADD THE FUCKING PERIOD.
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
			return DownloadHandlerAudioClip.GetContent(request);
			//return WavDataUtility.ToAudioClip(File.ReadAllBytes(path),Path.GetFileNameWithoutExtension(path));
        }

		public static Sprite SpriteFromTexture2D(Texture2D tex)
        {
			return SpriteFromTexture2D(tex, new Vector2(0.5f, 0.5f));
		}

        public static Sprite SpriteFromTexture2D(Texture2D tex, Vector2 center, float pixelsPerUnit = 1)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), center, pixelsPerUnit);
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
		/// Creates a midi from a file. It returns the id assigned to the midi, which is an altered version of the id you pass to avoid conflicts
		/// </summary>
		
		public static Dictionary<string, byte[]> MidiDatas = new Dictionary<string, byte[]>();
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

        private static void MidiFromBytes(string id, byte[] data)
        {
            if (MidiPlayerGlobal.Instance == null)
            {
                throw new Exception("Attempted to add Midi before MidiPlayerGlobal is created!");
            }
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
            byte[] data = AssetManager.MidiDatas.GetValueSafe(__instance.MPTK_MidiName);
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
