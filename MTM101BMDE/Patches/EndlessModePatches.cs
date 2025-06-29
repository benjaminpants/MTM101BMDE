using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI.Registers;
using System.IO;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(EndlessMapOverview))]
    [HarmonyPatch("Initialize")]
    internal class EndlessMapOverviewInitPass
    {
        internal class EndlessMapAlreadyInit : MonoBehaviour
        {

        }

        static void Prefix(EndlessMapOverview __instance, ref EndlessLevelTypeContainer[] ___level)
        {
            if (__instance.gameObject.GetComponent<EndlessMapAlreadyInit>() != null) return;
            List<EndlessLevelTypeContainer> containerList = ___level.ToList();
            EndlessModeManagement.UpdateContainerList(containerList);
            ___level = containerList.ToArray();
            __instance.gameObject.AddComponent<EndlessMapAlreadyInit>();
        }
    }

    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("Save")]
    internal class HighscoresSavePatch
    {
        static bool Prefix(HighScoreManager __instance)
        {
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.Vanilla) return true;
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None) return false;
            
            return false;
        }
    }

    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("Load")]
    internal class HighscoresLoadPatch
    {
        static bool Prefix(HighScoreManager __instance)
        {
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.Vanilla) return true;
            __instance.SetDefaultsEndless();
            __instance.SetDefaultsTrips();
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None) return false;
            return false;
        }
    }
}
