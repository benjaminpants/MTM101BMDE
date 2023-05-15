using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.IO;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;
using MTM101BaldAPI;

namespace MTM101BaldAPI.LangExtender
{

	static class LocalExtensions
	{
		public static string GetUnmoddedLocalizedText(this LocalizationManager me, string key, bool trymodonfail = true) // todo: why did i make this an extension
		{
			string result = key;
			if (LoaderExtension.OriginalText.ContainsKey(key))
			{
				result = LoaderExtension.OriginalText[key];
			}
			else
			{
				if (trymodonfail)
				{
					result = Singleton<LocalizationManager>.Instance.GetLocalizedText(key);
				}
			}
			return result;
		}
	}


	[HarmonyPatch(typeof(LocalizationManager))]
	[HarmonyPatch("LoadLocalizedText")]
	class LoaderExtension
	{

		public static Dictionary<string, string> OriginalText;

		private static void LoadMod(BaseUnityPlugin plug, Language language, ref Dictionary<string, string> ___localizedText)
		{
			string moddedfolderpath = Path.Combine(AssetManager.AssetManager.GetModPath(plug), "Language", language.ToString());
			if (Directory.Exists(moddedfolderpath))
			{
				string[] dirs = Directory.GetFiles(moddedfolderpath, "*.json");
				if (dirs.Length == 0)
				{
					return;
				}
				for (int i = 0; i < dirs.Length; i++)
				{
					LocalizationData localizationData = null;
					try
					{
						localizationData = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(dirs[i])); //use the base localization data so if BB+ ever changes it this tool will automatically be up to date.
					}
					catch (Exception E)
					{
						UnityEngine.Debug.LogError("Given JSON for file: " + Path.GetFileName(dirs[i]) + " is invalid!");
						UnityEngine.Debug.LogError(E.Message);
						continue;
					}
					for (int j = 0; j < localizationData.items.Length; j++)
					{
						if (!___localizedText.ContainsKey(localizationData.items[j].key))
						{
							___localizedText.Add(localizationData.items[j].key, localizationData.items[j].value);
						}
						else
						{
							___localizedText[localizationData.items[j].key] = localizationData.items[j].value;
						}
					}
#if DEBUG
					UnityEngine.Debug.Log("Loaded all data from " + Path.GetFileName(dirs[i]));
#endif
				}
			}
		}



		static void Finalizer(LocalizationManager __instance, ref Language language, ref Dictionary<string, string> ___localizedText)
		{
			string moddedfolderpath = Path.Combine(Application.streamingAssetsPath, "Modded");
#if DEBUG
			UnityEngine.Debug.Log("Loading Language Extensions...");
#endif
			OriginalText = ___localizedText;
			if (Directory.Exists(moddedfolderpath))
			{
				foreach (BaseUnityPlugin plug in GameObject.FindObjectsOfType<BaseUnityPlugin>())
				{
					LoadMod(plug, language, ref ___localizedText);
				}
			}
			else
			{
				Directory.CreateDirectory(moddedfolderpath);
				return;
			}
#if DEBUG
			UnityEngine.Debug.Log("All language data succesfull loaded!");
#endif
		}
	}
}
