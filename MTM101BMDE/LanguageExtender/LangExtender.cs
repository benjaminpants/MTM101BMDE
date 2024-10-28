using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

namespace MTM101BaldAPI.LangExtender
{

	public static class LocalExtensions
	{
		[Obsolete("GetUnmoddedLocalizedText is no longer functional and will be removed soon!", true)]
		public static string GetUnmoddedLocalizedText(this LocalizationManager me, string key, bool trymodonfail = true)
		{
			return key;
		}
	}

	[HarmonyPatch(typeof(LocalizationManager))]
	[HarmonyPatch("LoadLocalizedText")]
	internal class LanguageLoadingPatch
	{
        static void Postfix(LocalizationManager __instance, Language language)
		{
			AssetLoader.LoadAllQueuedLocalization(language);
		}
    }
}
