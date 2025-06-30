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
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None || MTM101BaldiDevAPI.highscoreHandler == SavedGameDataHandler.Unset) return false;
            ModdedHighscoreManager.Save();
            return false;
        }
    }

    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("AddScore")]
    internal class HighscoresAddScorePatch
    {
        static bool Prefix(out int rank)
        {
            rank = -1;
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None || MTM101BaldiDevAPI.highscoreHandler == SavedGameDataHandler.Unset) return false;
            return true;
        }
        static void Postfix(HighScoreManager __instance, int score, int seed, string name)
        {
            if (MTM101BaldiDevAPI.HighscoreHandler != SavedGameDataHandler.Modded) return;
            EndlessScore standardScore = __instance.endlessScores.Find(x => (x.score == score && x.seed == seed && x.name == name && x.levelId == __instance.currentLevelId));
            if (standardScore != null)
            {
                ModdedHighscoreManager.moddedScores.Add(new ModdedHighscoreManager.ModdedEndlessScore()
                {
                    guidAndTags = ModdedHighscoreManager.GetNewCurrentTagDict(),
                    levelId = __instance.currentLevelId,
                    name = standardScore.name,
                    score = standardScore.score,
                    seed = standardScore.seed
                });
            }
            // now, search for any scores that might've gotten removed
            for (int i = ModdedHighscoreManager.activeModdedScores.Count - 1; i >= 0; i--)
            {
                ModdedHighscoreManager.ModdedEndlessScore targetScore = ModdedHighscoreManager.activeModdedScores[i];
                // we couldn't find it
                if (__instance.endlessScores.Find(x => x.score == targetScore.score && x.name == targetScore.name && x.seed == targetScore.seed && x.levelId == targetScore.levelId) == null)
                {
                    ModdedHighscoreManager.activeModdedScores.Remove(targetScore);
                    ModdedHighscoreManager.moddedScores.Remove(targetScore);
                    Debug.Log("Pushed score out!");
                }
            }
            ModdedHighscoreManager.UpdateActiveScores();
            ModdedHighscoreManager.Save(); //resave our file
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
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None || MTM101BaldiDevAPI.highscoreHandler == SavedGameDataHandler.Unset) return false;
            ModdedHighscoreManager.Load();
            return false;
        }
    }
}
