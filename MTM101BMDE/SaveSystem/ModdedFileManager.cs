using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.SaveSystem
{
    public class ModdedFileManager : Singleton<ModdedFileManager>
    {
        public ModdedSaveGame saveData = new ModdedSaveGame();

        static FieldInfo _cgmbackupItems = AccessTools.Field(typeof(CoreGameManager), "backupItems"); 
        static FieldInfo _cgmrestoreItemsOnSpawn = AccessTools.Field(typeof(CoreGameManager), "restoreItemsOnSpawn");

        public void CreateSavedGameCoreManager(GameLoader loader)
        {
            UnityEngine.Object.Instantiate<CoreGameManager>(loader.cgmPre);
            ModdedSaveGame savedGameData = saveData;
            Singleton<CoreGameManager>.Instance.SetSeed(savedGameData.seed);
            Singleton<CoreGameManager>.Instance.SetLives(savedGameData.lives);
            Singleton<CoreGameManager>.Instance.AddPoints(savedGameData.ytps, 0, false);
            Singleton<CoreGameManager>.Instance.tripPlayed = savedGameData.fieldTripPlayed;
            Singleton<CoreGameManager>.Instance.LoadSavedMap(savedGameData.foundMapTiles.ConvertTo2d(savedGameData.mapSizeX, savedGameData.mapSizeZ));
            //Equivalent to Singleton<CoreGameManager>.Instance.RestoreSavedItems(savedGameData.items);
            List<ItemObject[]> backupItems = (List<ItemObject[]>)_cgmbackupItems.GetValue(Singleton<CoreGameManager>.Instance);
            backupItems.Add(new ItemObject[savedGameData.items.Count]);
            for (int i = 0; i < savedGameData.items.Count; i++)
            {
                backupItems[0][i] = savedGameData.items[i].LocateObject();
            }
            _cgmbackupItems.SetValue(Singleton<CoreGameManager>.Instance,backupItems); //not sure if necessary.
            _cgmrestoreItemsOnSpawn.SetValue(Singleton<CoreGameManager>.Instance, true);
        }

        public void DeleteSavedGame()
        {
            saveData.saveAvailable = false;
            Singleton<PlayerFileManager>.Instance.Save();
        }
    }



    // ******* Patches ******* //

    [HarmonyPatch(typeof(MainModeButtonController))]
    [HarmonyPatch("OnEnable")]
    class DisableSaveButton
    {
        static bool Prefix(MainModeButtonController __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Modded)
            {
                __instance.mainNew.SetActive(!Singleton<ModdedFileManager>.Instance.saveData.saveAvailable);
                __instance.mainContinue.SetActive(Singleton<ModdedFileManager>.Instance.saveData.saveAvailable);
                return false;
            }
            if (!MTM101BaldiDevAPI.SaveGamesEnabled)
            {
                __instance.mainNew.SetActive(true);
                __instance.mainContinue.SetActive(false);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("SetSave")]
    class DisableSave
    {
        static void Prefix(ref bool val)
        {
            val = val & MTM101BaldiDevAPI.SaveGamesEnabled;
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("LoadSavedGame")]
    class LoadModdedSavedGame
    {
        static bool Prefix(GameLoader __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return true;
            Singleton<ModdedFileManager>.Instance.CreateSavedGameCoreManager(__instance);
            Singleton<CursorManager>.Instance.LockCursor();
            __instance.SetMode(0);
            __instance.LoadLevel(__instance.list.scenes[Singleton<ModdedFileManager>.Instance.saveData.levelId]);
            Singleton<ModdedFileManager>.Instance.DeleteSavedGame();
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("Start")]
    class AddModdedFM
    {
        static void Prefix(PlayerFileManager __instance)
        {
            __instance.gameObject.AddComponent<ModdedFileManager>();
        }
    }

    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("ResetSaveData")]
    class ResetModdedData
    {
        static void Postfix()
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
            ModdedFileManager.Instance.saveData = new ModdedSaveGame();
            ModdedSaveGame.ModdedSaveGameHandlers.Do(x =>
            {
                x.Value.Reset();
            });
        }
    }

    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("SaveAndQuit")]
    class SaveAndQuitModdedData
    {
        // override the function completely, if we make sure every reference is referring to ModdedSaveGame, this should leave vanilla games intact.
        static bool Prefix(CoreGameManager __instance, ref int ___lives, ref int ___seed, ref bool[,] ___foundTilesToRestore)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return true;
            ModdedSaveGame newSave = new ModdedSaveGame();
            newSave.saveAvailable = true;
            ItemObject[] itms = __instance.GetPlayer(0).itm.items;
            for (int i = 0; i < itms.Length; i++)
            {
                newSave.items.Add(new ModdedItemIdentifier(itms[i]));
            }
            newSave.levelId = __instance.sceneObject.levelNo;
            newSave.ytps = __instance.GetPoints(0);
            newSave.lives = ___lives;
            newSave.seed = ___seed;
            newSave.saveAvailable = true;
            newSave.fieldTripPlayed = __instance.tripPlayed;
            __instance.BackupMap(Singleton<BaseGameManager>.Instance.Ec.map);
            newSave.foundMapTiles = ___foundTilesToRestore.ConvertTo1d(Singleton<BaseGameManager>.Instance.Ec.map.size.x, Singleton<BaseGameManager>.Instance.Ec.map.size.z);
            newSave.mapSizeX = Singleton<BaseGameManager>.Instance.Ec.map.size.x;
            newSave.mapSizeZ = Singleton<BaseGameManager>.Instance.Ec.map.size.z;
            Singleton<ModdedFileManager>.Instance.saveData = newSave;
            Singleton<PlayerFileManager>.Instance.Save();
            __instance.Quit();
            return false;
        }
    }
}
