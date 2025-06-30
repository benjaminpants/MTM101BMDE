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
                }
            }
            ModdedHighscoreManager.UpdateActiveScores();
            ModdedHighscoreManager.Save(); //resave our file
        }
    }

    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("AddTripScore")]
    internal class HighscoresAddTripScorePatch
    {
        static bool Prefix(out int rank)
        {
            rank = -1;
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.None || MTM101BaldiDevAPI.highscoreHandler == SavedGameDataHandler.Unset) return false;
            return true;
        }
        static void Postfix(HighScoreManager __instance)
        {
            for (int x = 0; x < __instance.tripNames.GetLength(0); x++)
            {
                for (int y = 0; y < __instance.tripNames.GetLength(1); y++)
                {
                    ModdedHighscoreManager.tripNames[x, y] = __instance.tripNames[x, y];
                }
            }
            for (int x = 0; x < __instance.tripScores.GetLength(0); x++)
            {
                for (int y = 0; y < __instance.tripScores.GetLength(1); y++)
                {
                    ModdedHighscoreManager.tripScores[x, y] = __instance.tripScores[x, y];
                }
            }
            ModdedHighscoreManager.Save(); // i hate this i hate you. mystman12 i dont hate you but please fix the field trip score saving system this is A NIGHTMARE
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

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("ResetEndless")]
    internal class OptionsMenuResetEndlessScores
    {
        static void Postfix(OptionsMenu __instance)
        {
            if (MTM101BaldiDevAPI.HighscoreHandler != SavedGameDataHandler.Modded) return;
            for (int i = ModdedHighscoreManager.activeModdedScores.Count - 1; i >= 0; i--)
            {
                ModdedHighscoreManager.moddedScores.Remove(ModdedHighscoreManager.activeModdedScores[i]);
                ModdedHighscoreManager.activeModdedScores.RemoveAt(i);
                ModdedHighscoreManager.UpdateActiveScores();
            }
        }
    }
    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("ResetTrip")]
    internal class OptionsMenuResetTripScores
    {
        static void Postfix(OptionsMenu __instance)
        {
            if (MTM101BaldiDevAPI.HighscoreHandler != SavedGameDataHandler.Modded) return;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    ModdedHighscoreManager.tripScores[j, i] = 0;
                    ModdedHighscoreManager.tripNames[j, i] = "Baldi";
                }
            }
            ModdedHighscoreManager.UpdateRealHighscoreManager();
        }
    }
}
