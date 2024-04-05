using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.SaveSystem
{
    public class ModdedFileManager : Singleton<ModdedFileManager>
    {
        public ModdedSaveGame saveData = new ModdedSaveGame();
        public Dictionary<int, PartialModdedSavedGame> saveDatas = new Dictionary<int, PartialModdedSavedGame>();
        public int saveIndex { get; internal set; }
        public string saveName { get; internal set; }
        public List<int> saveIndexes = new List<int>();
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

        public int FindAppropiateSaveGame(string myPath, bool ignoreAlready, string name)
        {
            if (name != saveName)
            {
                saveIndex = 0;
            }
            if ((!ignoreAlready) && (saveIndex != 0))
            {
                return saveIndex;
            }
            saveName = name;
            saveIndexes.Clear();
            saveDatas.Clear();
            if (File.Exists(Path.Combine(myPath, "availableSlots.txt")))
            {
                saveIndexes.AddRange(File.ReadAllLines(Path.Combine(myPath, "availableSlots.txt")).Select(x => int.Parse(x)));
            }
            else
            {
                if (File.Exists(Path.Combine(myPath, "savedgame0.bbapi")))
                {
                    File.Move(Path.Combine(myPath, "savedgame0.bbapi"), Path.Combine(myPath, "savedgame1.bbapi"));
                    saveIndexes.Add(1);
                    FileStream fs = File.OpenRead(Path.Combine(myPath, "savedgame1.bbapi"));
                    BinaryReader reader = new BinaryReader(fs);
                    saveDatas.Add(1, ModdedSaveGame.PartialLoad(reader));
                    reader.Close();
                    return 1;
                }
                int validIndex = 1;
                saveIndexes.Add(validIndex);
                saveDatas.Add(validIndex, new PartialModdedSavedGame());
                saveIndexes.Sort();
                return validIndex;
            }
            for (int i = 0; i < saveIndexes.Count; i++)
            {
                if (!File.Exists(Path.Combine(myPath, "savedgame" + saveIndexes[i] + ".bbapi"))) continue;
                FileStream fs = File.OpenRead(Path.Combine(myPath, "savedgame" + saveIndexes[i] + ".bbapi"));
                BinaryReader reader = new BinaryReader(fs);
                saveDatas.Add(saveIndexes[i], ModdedSaveGame.PartialLoad(reader));
                reader.Close();
            }
            // a list of kvps that has every file that shares the same mods, might include files with less mods
            KeyValuePair<int, PartialModdedSavedGame>[] containsAllMods = saveDatas.Where(x =>
            {
                for (int i = 0; i < x.Value.mods.Length; i++)
                {
                    if (!ModdedSaveGame.ModdedSaveGameHandlers.ContainsKey(x.Value.mods[i]))
                    {
                        return false;
                    }
                }
                return true;
            }).ToArray();
            containsAllMods.Select(x => x.Value).Do(x => x.canBeMoved = true);
            KeyValuePair<int, PartialModdedSavedGame>[] containsExactMods = containsAllMods.Where(x =>
            {
                int mods = 0;
                for (int i = 0; i < x.Value.mods.Length; i++)
                {
                    if (ModdedSaveGame.ModdedSaveGameHandlers.ContainsKey(x.Value.mods[i]))
                    {
                        mods++;
                    }
                }
                return mods == ModdedSaveGame.ModdedSaveGameHandlers.Count;
            }).ToArray();
            if (containsExactMods.Length > 1)
            {
                MTM101BaldiDevAPI.Log.LogError("Dirty hacker! Found duplicate files with same mods! Unfortunately, can't do anything about this, but SHAME! SHAMMEEE!");
            }
            if (containsExactMods.Length == 0)
            {
                int validIndex = 1;
                while (saveDatas.ContainsKey(validIndex)) validIndex++;
                saveIndexes.Add(validIndex);
                saveDatas.Add(validIndex, new PartialModdedSavedGame());
                saveIndexes.Sort();
                return validIndex;
            }
            containsExactMods[0].Value.canBeMoved = false;
            return containsExactMods[0].Key;
        }

        public void SaveFileList(string myPath)
        {
            string toPrint = "";
            saveIndexes.Do(x => toPrint += (x + "\n"));
            toPrint = toPrint.Trim();
            File.WriteAllText(Path.Combine(myPath, "availableSlots.txt"), toPrint);
        }

        public void UpdateCurrentPartialSave()
        {
            saveDatas[saveIndex] = new PartialModdedSavedGame(saveData);
        }

        public void DeleteIndexedGame(int index)
        {
            string myPath = ModdedSaveSystem.GetSaveFolder(MTM101BaldiDevAPI.Instance, Singleton<PlayerFileManager>.Instance.fileName);
            File.Delete(Path.Combine(myPath, "savedgame" + index + ".bbapi"));
            saveDatas.Remove(index);
            saveIndexes.Remove(index);
            SaveFileList(myPath);
        }

        public void SaveGameWithIndex(string path, int index)
        {
            FileStream fs = File.OpenWrite(Path.Combine(path, "savedgame" + index + ".bbapi"));
            fs.SetLength(0); // make sure to clear the contents before writing to it!
            BinaryWriter writer = new BinaryWriter(fs);
            saveData.Save(writer);
            writer.Close();
        }

        public void LoadGameWithIndex(string path, int index)
        {
            if (!File.Exists(Path.Combine(path, "savedgame" + index + ".bbapi"))) return;
            FileStream fs = File.OpenRead(Path.Combine(path, "savedgame" + index + ".bbapi"));
            BinaryReader reader = new BinaryReader(fs);
            ModdedSaveLoadStatus status = Singleton<ModdedFileManager>.Instance.saveData.Load(reader);
            reader.Close();
            switch (status)
            {
                default:
                    break;
                case ModdedSaveLoadStatus.MissingHandlers:
                    MTM101BaldiDevAPI.Log.LogWarning("Failed to load save because one or more mod handlers were missing!");
                    Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
                    break;
                case ModdedSaveLoadStatus.MissingItems:
                    if (ItemMetaStorage.Instance.All().Length == 0) break; //item metadata hasnt loaded yet!
                    MTM101BaldiDevAPI.Log.LogWarning("Failed to load save because one or more items couldn't be found!");
                    Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
                    break;
                case ModdedSaveLoadStatus.NoSave:
                    MTM101BaldiDevAPI.Log.LogInfo("No save data was found.");
                    break;
                case ModdedSaveLoadStatus.Success:
                    MTM101BaldiDevAPI.Log.LogInfo("Modded Savedata was succesfully loaded!");
                    break;
            }
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
            ModdedSaveGame.ModdedSaveGameHandlers.Do(x =>
            {
                x.Value.OnCGMCreated(Singleton<CoreGameManager>.Instance, true);
            });
            return false;
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("Initialize")]
    class LoadStandardGame
    {
        static void Postfix(GameLoader __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
            ModdedSaveGame.ModdedSaveGameHandlers.Do(x =>
            {
                x.Value.OnCGMCreated(Singleton<CoreGameManager>.Instance, false);
            });
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
            Singleton<ModdedFileManager>.Instance.saveName = "";
            Singleton<ModdedFileManager>.Instance.saveIndex = 0; //force a reload next time we try to grab data
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
