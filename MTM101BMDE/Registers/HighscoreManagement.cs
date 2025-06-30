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


    public static class ModdedHighscoreManager
    {
        public static List<ModdedEndlessScore> moddedScores = new List<ModdedEndlessScore>();
        
        public static List<ModdedEndlessScore> activeModdedScores = new List<ModdedEndlessScore>();

        public class ModdedEndlessScore
        {
            public string name;
            public int score;
            public int seed;
            public string levelId;
            public Dictionary<string, string[]> guidAndTags = new Dictionary<string, string[]>();
        }

        internal static Dictionary<PluginInfo, string[]> tagList = new Dictionary<PluginInfo, string[]>();

        /// <summary>
        /// Adds the specified mod to the highscore list. Do this if your mod modifies endless mode but doesn't directly add any levels itself.
        /// Or, if you want to add tags. You can call this whenever you want to change tags.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="tags"></param>
        public static void AddModToList(PluginInfo info, string[] tags = null)
        {
            if (tagList.ContainsKey(info)) return;
            tagList.Add(info, tags == null ? new string[0] : tags);
        }

        public static Dictionary<string, string[]> GetNewCurrentTagDict()
        {
            Dictionary<string, string[]> newTagDic = new Dictionary<string, string[]>();
            foreach (KeyValuePair<PluginInfo, string[]> kvp in tagList)
            {
                newTagDic.Add(kvp.Key.Metadata.GUID, kvp.Value.ToArray());
            }
            return newTagDic;
        }

        public const byte version = 0;

        public static string highscorePath => Path.Combine(highscoreFolderPath, "Highscores.bbapih");
        public static string highscoreFolderPath => Path.Combine(Application.persistentDataPath, "Modded", ModdedSaveSystem.UnmanagedFolderName, MTM101BaldiDevAPI.ModGUID);
        public static void Save()
        {
            if (!Directory.Exists(highscoreFolderPath))
            {
                Directory.CreateDirectory(highscoreFolderPath);
            }
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(highscorePath));
            ModdedHighscoreManager.Write(writer);
            writer.Close();
        }

        public static void UpdateActiveScores()
        {
            activeModdedScores.Clear();
            for (int i = 0; i < moddedScores.Count; i++)
            {
                ModdedEndlessScore score = moddedScores[i];
                bool shouldSkip = false;
                if (tagList.Keys.Count != score.guidAndTags.Count) continue; // no need for any deeper comparison, the key counts dont match so there is a mod missing or an extra mod
                foreach (KeyValuePair<string, string[]> kvp in score.guidAndTags)
                {
                    if (!tagList.Keys.Any(x => x.Metadata.GUID == kvp.Key)) { shouldSkip = true; break; } // the mod this belongs to is missing from the list
                    PluginInfo info = tagList.Keys.First(x => x.Metadata.GUID == kvp.Key);
                    if (tagList[info].Length != kvp.Value.Length) { shouldSkip = true; break; } // skip earlier because once again, length mismatch means a tag is missing or there is a new one
                    // todo: rewrite with a for loop?
                    if (!tagList[info].OrderBy(x => x).SequenceEqual(kvp.Value.OrderBy(x => x))) { shouldSkip = true; break; } // the tags are non-matching
                }
                if (shouldSkip) continue;
                activeModdedScores.Add(score);
            }
        }

        public static void UpdateRealHighscoreManager()
        {
            HighScoreManager.Instance.SetDefaultsEndless();
            HighScoreManager.Instance.SetDefaultsTrips();
            UpdateActiveScores();
            for (int i = 0; i < activeModdedScores.Count; i++)
            {
                ModdedEndlessScore score = activeModdedScores[i];
                HighScoreManager.Instance.endlessScores.Add(new EndlessScore(score.score, score.seed, score.name, score.levelId));
            }

            // dumb
            HighScoreManager.Instance.tripNames = tripNames;
            HighScoreManager.Instance.tripScores = tripScores;
        }

        public static void Load()
        {
            if (!Directory.Exists(highscoreFolderPath))
            {
                return;
            }
            BinaryReader reader = new BinaryReader(File.OpenRead(highscorePath));
            Read(reader);
            reader.Close();
            UpdateActiveScores();
            UpdateRealHighscoreManager();
        }

        private static void Read(BinaryReader reader)
        {
            moddedScores.Clear();
            byte thisVersion = reader.ReadByte();

            List<string> compStrings = new List<string>();

            int stringCount = reader.ReadInt32();
            for (int i = 0; i < stringCount; i++)
            {
                compStrings.Add(reader.ReadString());
            }

            int scoreCount = reader.ReadInt32();
            for (int i = 0; i < scoreCount; i++)
            {
                ModdedEndlessScore endScore = new ModdedEndlessScore();
                endScore.name = reader.ReadString();
                endScore.levelId = compStrings[reader.ReadInt32()];
                endScore.seed = reader.ReadInt32();
                endScore.score = reader.ReadInt32();

                int guidTagsCount = reader.ReadInt32();
                for (int j = 0; j < guidTagsCount; j++)
                {
                    string guid = compStrings[reader.ReadInt32()];
                    endScore.guidAndTags.Add(guid, new string[reader.ReadInt32()]);
                    for (int k = 0; k < endScore.guidAndTags[guid].Length; k++)
                    {
                        endScore.guidAndTags[guid][k] = compStrings[reader.ReadInt32()];
                    }
                }
                moddedScores.Add(endScore);
            }

            int[] tripScores1d = new int[reader.ReadInt32()];
            for (int i = 0; i < tripScores1d.Length; i++)
            {
                tripScores1d[i] = reader.ReadInt32();
            }

            string[] tripNames1d = new string[reader.ReadInt32()];
            for (int i = 0; i < tripNames1d.Length; i++)
            {
                bool isNull = reader.ReadBoolean();
                if (isNull) continue;
                tripNames1d[i] = reader.ReadString();
            }

            tripScores = tripScores1d.ConvertTo2d(16,5);
            tripNames = tripNames1d.ConvertTo2d(16,5);
        }

        private static void Write(BinaryWriter writer)
        {
            writer.Write(version);

            // compress our strings to avoid writing guids tons of times thus bloating the filesize
            // (i mean honestly most will barely even play endless mode with mods so i doubt this file will ever surpass more than like. a few megabytes)
            // (but its better to keep track)
            List<string> compStrings = new List<string>();
            foreach (ModdedEndlessScore score in moddedScores)
            {
                foreach (KeyValuePair<string, string[]> kvp in score.guidAndTags)
                {
                    if (!compStrings.Contains(kvp.Key))
                    {
                        compStrings.Add(kvp.Key);
                    }
                    for (int i = 0; i < kvp.Value.Length; i++)
                    {
                        if (!compStrings.Contains(kvp.Value[i]))
                        {
                            compStrings.Add(kvp.Value[i]);
                        }
                    }
                }
                if (!compStrings.Contains(score.levelId))
                {
                    compStrings.Add(score.levelId);
                }
            }

            writer.Write(compStrings.Count);
            for (int i = 0; i < compStrings.Count; i++)
            {
                writer.Write(compStrings[i]);
            }

            writer.Write(moddedScores.Count);
            for (int i = 0; i < moddedScores.Count; i++)
            {
                ModdedEndlessScore toWrite = moddedScores[i];
                writer.Write(toWrite.name);
                writer.Write(compStrings.IndexOf(toWrite.levelId));
                writer.Write(toWrite.seed);
                writer.Write(toWrite.score);
                // now write guids and stuff
                writer.Write(toWrite.guidAndTags.Count);
                foreach (KeyValuePair<string, string[]> guidAndTag in toWrite.guidAndTags)
                {
                    writer.Write(compStrings.IndexOf(guidAndTag.Key)); // write the key/guid
                    writer.Write(guidAndTag.Value.Length);
                    for (int j = 0; j < guidAndTag.Value.Length; j++)
                    {
                        writer.Write(compStrings.IndexOf(guidAndTag.Value[j])); // write the tags
                    }
                }
            }

            // write the field trip stuff (placeholder)
            int[] tripScores1d = tripScores.ConvertTo1d(16, 5);
            writer.Write(tripScores1d.Length);
            for (int i = 0; i < tripScores1d.Length; i++)
            {
                writer.Write(tripScores1d[i]);
            }

            string[] tripNames1d = tripNames.ConvertTo1d(16, 5);
            writer.Write(tripNames1d.Length);
            for (int i = 0; i < tripNames1d.Length; i++)
            {
                writer.Write(tripNames1d[i] == null);
                if (tripNames1d[i] != null)
                {
                    writer.Write(tripNames1d[i]);
                }
            }
        }

        // placeholder
        public static int[,] tripScores = new int[16, 5];
        public static string[,] tripNames = new string[16, 5];
    }

    public static class EndlessModeManagement
    {
        internal static List<ModdedEndlessLevel> medList = new List<ModdedEndlessLevel>();
        internal static Dictionary<PluginInfo, Action<List<EndlessLevelTypeContainer>>> endActions = new Dictionary<PluginInfo, Action<List<EndlessLevelTypeContainer>>>();

        /// <summary>
        /// Adds an entirely new level to endless mode.
        /// This automatically adds your mod to the ModdedHighscoreManager.
        /// </summary>
        /// <param name="data"></param>
        public static void AddEndlessLevel(ModdedEndlessLevel data)
        {
            if (medList.Contains(data)) return;
            medList.Add(data);
            if (!ModdedHighscoreManager.tagList.ContainsKey(data.pluginInfo))
            {
                ModdedHighscoreManager.tagList.Add(data.pluginInfo, new string[0]);
            }
        }

        /// <summary>
        /// Adds an action to be called when the EndlessLevelTypeContainer array is being finalized.
        /// Use this to add sizes to existing levels or re-order things.
        /// This automatically adds your mod to the ModdedHighscoreManager.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="act"></param>
        public static void AddEndlessAction(PluginInfo key, Action<List<EndlessLevelTypeContainer>> act)
        {
            if (!ModdedHighscoreManager.tagList.ContainsKey(key))
            {
                ModdedHighscoreManager.tagList.Add(key, new string[0]);
            }
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
