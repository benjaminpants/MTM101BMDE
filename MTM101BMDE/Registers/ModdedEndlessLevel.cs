using BepInEx;
using MTM101BaldAPI.SaveSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Registers
{


    public class ModdedEndlessLevel
    {
        protected PluginInfo _info;
        protected LevelType _type;
        protected string _key;
        protected Dictionary<string, SceneObject> _sizes;
        public PluginInfo pluginInfo => _info;
        public string key => _key;
        public LevelType type => type;
        public Dictionary<string, SceneObject> sizes => _sizes;

        public ModdedEndlessLevel(PluginInfo info, string key, LevelType type, Dictionary<string, SceneObject> sizes)
        {
            _info = info;
            _key = key;
            _type = type;
            _sizes = new Dictionary<string, SceneObject>(sizes);
        }

        public EndlessLevelTypeContainer ConvertToContainer()
        {
            EndlessLevelTypeContainer edl = new EndlessLevelTypeContainer();
            edl.type = _type;
            edl.typeKey = _key;
            List<EndlessLevelSizeContainer> sizeContainers = new List<EndlessLevelSizeContainer>();
            foreach (KeyValuePair<string, SceneObject> kvp in _sizes)
            {
                sizeContainers.Add(new EndlessLevelSizeContainer()
                {
                    scene = kvp.Value,
                    sizeKey = kvp.Key,
                });
            }
            edl.size = sizeContainers.ToArray();
            return edl;
        }
    }


    public static class EndlessModeManagement
    {
        internal static List<ModdedEndlessLevel> medList = new List<ModdedEndlessLevel>();
        internal static Dictionary<PluginInfo, Action<List<EndlessLevelTypeContainer>>> endActions = new Dictionary<PluginInfo, Action<List<EndlessLevelTypeContainer>>>();

        public const byte version = 0;
        private static int selectedFile = 0;
        public static string highscorePath => Path.Combine(highscoreFolderPath, "Highscores.bbapih");
        public static string highscoreFolderPath => Path.Combine(Application.persistentDataPath, "Modded", ModdedSaveSystem.UnmanagedFolderName, MTM101BaldiDevAPI.ModGUID);
        public static void Save(HighScoreManager instance)
        {
            if (!Directory.Exists(highscoreFolderPath))
            {
                Directory.CreateDirectory(highscoreFolderPath);
            }
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(highscorePath));
            writer.Write(version);
            string[] guids = GetAssociatedHighscoreGUIDs();
            writer.Write(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                writer.Write(guids[i]);
            }
            string[] tags = GetAssociatedHighscoreTags();
            writer.Write(tags.Length);
            for (int i = 0; i < tags.Length; i++)
            {
                writer.Write(tags[i]);
            }

            writer.Write(instance.endlessScores.Count);
            for (int i = 0; i < instance.endlessScores.Count; i++)
            {
                EndlessScore scr = instance.endlessScores[i];
                writer.Write(scr.name);
                writer.Write(scr.levelId);
                writer.Write(scr.seed);
                writer.Write(scr.score);
            }

            // placeholder, will be replaced in the future
            int[] tripScores = instance.tripScores.ConvertTo1d(16, 5);
            writer.Write(tripScores.Length);
            for (int i = 0; i < tripScores.Length; i++)
            {
                writer.Write(tripScores[i]);
            }

            string[] tripNames = instance.tripNames.ConvertTo1d(16, 5);
            writer.Write(tripNames.Length);
            for (int i = 0; i < tripNames.Length; i++)
            {
                writer.Write(tripNames[i]);
            }

            writer.Close();
        }

        public static void Load(HighScoreManager instance)
        {
            if (!Directory.Exists(highscoreFolderPath))
            {
                return;
            }
        }

        public static string[] GetAssociatedHighscoreGUIDs()
        {
            List<string> tags = new List<string>();
            foreach (ModdedEndlessLevel level in medList)
            {
                tags.Add(level.pluginInfo.Metadata.GUID);
            }
            foreach (KeyValuePair<PluginInfo, Action<List<EndlessLevelTypeContainer>>> kvp in endActions)
            {
                tags.Add(kvp.Key.Metadata.GUID);
            }
            return tags.Distinct().ToArray();
        }

        public static string[] GetAssociatedHighscoreTags()
        {
            return new string[0];
        }

        /// <summary>
        /// Adds an entirely new level to endless mode.
        /// </summary>
        /// <param name="data"></param>
        public static void AddEndlessLevel(ModdedEndlessLevel data)
        {
            if (medList.Contains(data)) return;
            medList.Add(data);
        }

        /// <summary>
        /// Adds an action to be called when the EndlessLevelTypeContainer array is being finalized.
        /// Use this to add sizes to existing levels or re-order things.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="act"></param>
        public static void AddEndlessAction(PluginInfo key, Action<List<EndlessLevelTypeContainer>> act)
        {
            if (endActions.ContainsKey(key))
            {
                endActions[key] = act;
                return;
            }
            endActions.Add(key, act);
        }

        internal static void UpdateContainerList(List<EndlessLevelTypeContainer> containList)
        {
            for (int i = 0; i < medList.Count; i++)
            {
                containList.Add(medList[i].ConvertToContainer());
            }
            foreach (KeyValuePair<PluginInfo, Action<List<EndlessLevelTypeContainer>>> item in endActions)
            {
                item.Value.Invoke(containList);
            }
        }
    }
}
