using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib; //god im hoping i got the right version of harmony
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using MTM101BaldAPI.NameMenu;

namespace MTM101BaldAPI
{
	[HarmonyPatch(typeof(MainModeButtonController))]
	[HarmonyPatch("OnEnable")]
	class DisableSaveButton
	{

		static bool Prefix(MainModeButtonController __instance)
		{
			if (!MTM101BaldiDevAPI.SavesEnabled)
			{
				__instance.mainNew.SetActive(true);
				__instance.mainContinue.SetActive(false);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(CoreGameManager))]
	[HarmonyPatch("SaveAndQuit")]
	class DisableSaveAndQuit
	{

		static bool Prefix(CoreGameManager __instance)
		{
			if (!MTM101BaldiDevAPI.SavesEnabled)
			{
				__instance.Quit();
				return false;
			}
			return true;
		}
	}

}
