using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

namespace MTM101BaldAPI.SaveSystem
{
    public static class ModdedSaveSystem
    {
        private static Dictionary<BaseUnityPlugin, Action<bool, string>> saveLoadActions = new Dictionary<BaseUnityPlugin, Action<bool, string>>();
        private static Dictionary<BaseUnityPlugin, Action<bool, string>> unmanagedSaveLoadActions = new Dictionary<BaseUnityPlugin, Action<bool, string>>();

        public const string UnmanagedFolderName = "!Unmanaged";

        /// <summary>
        /// Allows you to add an action to be called when the game saves and loads, please only use this system if you plan to save data.
        /// The first value passed to the action is whether or not its saving (true if saving, false if loading)
        /// The second is the path allocated to your mod to save your modded save data
        /// </summary>
        /// <param name="p"></param>
        /// <param name="act"></param>
        public static void AddSaveLoadAction(BaseUnityPlugin p, Action<bool, string> act)
        {
            saveLoadActions.Add(p,act);
        }

        /// <summary>
        /// Allows you to add an action to be called when the game saves and loads, please only use this system if you plan to save data.
        /// Unlike AddSaveLoadAction, Unmanaged SaveLoad actions are all given the same directory regardless of who is playing.
        /// The first value passed to the action is whether or not its saving (true if saving, false if loading)
        /// The second is the path allocated to your mod to save your modded save data
        /// </summary>
        /// <param name="p"></param>
        /// <param name="act"></param>
        public static void AddUnmanagedSaveLoadAction(BaseUnityPlugin p, Action<bool, string> act)
        {
            unmanagedSaveLoadActions.Add(p, act);
        }

        internal static void CallSaveLoadAction(PlayerFileManager instance, bool isSave)
        {
            string fName = instance.fileName;
            foreach (KeyValuePair<BaseUnityPlugin, Action<bool, string>> kvp in saveLoadActions)
            {
                string curPath = GetSaveFolder(kvp.Key,fName);
                Directory.CreateDirectory(curPath);
                kvp.Value.Invoke(isSave, curPath);
            }
            foreach (KeyValuePair<BaseUnityPlugin, Action<bool, string>> kvp in unmanagedSaveLoadActions)
            {
                string curPath = GetSaveFolder(kvp.Key, UnmanagedFolderName);
                Directory.CreateDirectory(curPath);
                kvp.Value.Invoke(isSave, curPath);
            }
        }

        public static string GetSaveFolder(BaseUnityPlugin plug, string fileName)
        {
            return Path.Combine(Application.persistentDataPath, "Modded", fileName, plug.Info.Metadata.GUID);
        }

        public static string GetCurrentSaveFolder(BaseUnityPlugin plug)
        {
            return GetSaveFolder(plug, Singleton<PlayerFileManager>.Instance.fileName);
        }

        public static void DeleteFile(PlayerFileManager instance, string toDelete)
        {
            //string fName = instance.fileName;
            Directory.Delete(Path.Combine(Application.persistentDataPath, "Modded", toDelete), true);
        }

        public static void CallSaveLoadAction(BaseUnityPlugin p, bool saveLoad, string path)
        {
            saveLoadActions[p].Invoke(saveLoad,path);
        }

    }


    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("Find")]
    class DisableFindingOnModdedGames
    {
        static bool Prefix()
        {
            if (MTM101BaldiDevAPI.SaveGameHasMods) return false;
            return true;
        }

        // TODO: figure out what. the fuck. is happening
        static Exception Finalizer()
        {
            //MTM101BaldiDevAPI.Log.LogWarning("Why is PlayerFileManager.Find STILL BEING CALLED??? I DONT KNOW.");
            return null;
        }
    }

    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("Save")]
    [HarmonyPatch(new Type[0] {} )]
    class SaveFilePatch
    {
        static void Postfix(PlayerFileManager __instance)
        {
            ModdedSaveSystem.CallSaveLoadAction(__instance, true);
        }
    }

    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("Load")]
    class LoadFilePatch
    {
        static void Postfix(PlayerFileManager __instance)
        {
            ModdedSaveSystem.CallSaveLoadAction(__instance, false);
        }
    }

    [HarmonyPatch(typeof(PlayerFileManager))]
    [HarmonyPatch("Delete")]
    class DeleteFilePatch
    {
        static void Postfix(PlayerFileManager __instance, string deleteName)
        {
            ModdedSaveSystem.DeleteFile(__instance, deleteName);
        }
    }

    // protect save games from some places I missed

    [HarmonyPatch(typeof(MainGameManager))]
    [HarmonyPatch("BeginPlay")]
    class MainBeginPlayPatch
    {
        static void Prefix(out bool? __state)
        {
            if (MTM101BaldiDevAPI.saveHandler == SavedGameDataHandler.Modded)
            {
                __state = Singleton<PlayerFileManager>.Instance.savedGameData.saveAvailable;
                return;
            }
            __state = null;
        }

        static void Postfix(bool? __state)
        {
            if (__state == null) return;
            Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
            Singleton<PlayerFileManager>.Instance.savedGameData.saveAvailable = __state.Value;
            Singleton<PlayerFileManager>.Instance.Save();
        }
    }

    [HarmonyPatch(typeof(PlaceholderWinManager))]
    [HarmonyPatch("BeginPlay")]
    class WinBeginPlayPatch
    {
        static void Prefix(out bool? __state)
        {
            if (MTM101BaldiDevAPI.saveHandler == SavedGameDataHandler.Modded)
            {
                __state = Singleton<PlayerFileManager>.Instance.savedGameData.saveAvailable;
                return;
            }
            __state = null;
        }

        static void Postfix(bool? __state)
        {
            if (__state == null) return;
            Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
            Singleton<PlayerFileManager>.Instance.savedGameData.saveAvailable = __state.Value;
            Singleton<PlayerFileManager>.Instance.Save();
        }
    }
}
