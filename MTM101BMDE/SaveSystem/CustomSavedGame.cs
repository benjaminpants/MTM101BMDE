using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MTM101BaldAPI.Registers;
using System.Linq;

namespace MTM101BaldAPI.SaveSystem
{
    public class PartialModdedSavedGame
    {
        public int seed;
        public string[] mods;
        public bool hasFile;
        public bool canBeMoved;
        public Dictionary<string, string[]> tags;

        public PartialModdedSavedGame(int seed, string[] mods, Dictionary<string, string[]> tags)
        {
            this.seed = seed;
            this.mods = mods;
            hasFile = true;
            canBeMoved = false;
            this.tags = tags;
        }
        
        public PartialModdedSavedGame(ModdedSaveGame saveGame)
        {
            seed = saveGame.seed;
            mods = ModdedSaveGame.ModdedSaveGameHandlers.Keys.ToArray();
            tags = new Dictionary<string, string[]>();
            foreach (KeyValuePair<string, string[]> kvp in saveGame.modTags)
            {
                tags.Add(kvp.Key, kvp.Value);
            }
            hasFile = saveGame.saveAvailable;
            canBeMoved = false;
        }

        public PartialModdedSavedGame(string[] mods, Dictionary<string, string[]> tags)
        {
            seed = 0;
            this.mods = mods;
            this.tags = tags;
            hasFile = false;
            canBeMoved = false;
        }

        public PartialModdedSavedGame()
        {
            this.seed = 0;
            this.mods = new string[0];
            this.hasFile = false;
            canBeMoved = false;
            tags = new Dictionary<string, string[]>();
        }
    }


    /// <summary>
    /// Stores the mod GUID and name of the ItemObject to allow it to be referenced later.
    /// </summary>
    [Serializable]
    public struct ModdedItemIdentifier
    {
        public byte version;
        public string itemPluginGUID;
        public string itemObjectName; // the name of the ItemObject

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(itemPluginGUID);
            writer.Write(itemObjectName);
        }

        public ItemObject LocateObject()
        {
            ModdedItemIdentifier thiz = this;
            ItemMetaData[] datas = MTM101BaldiDevAPI.itemMetadata.FindAll(x => x.info.Metadata.GUID == thiz.itemPluginGUID);
            for (int i = 0; i < datas.Length; i++)
            {
                ItemObject[] objectsMatchingName = datas[i].itemObjects.Where(x => x.name == thiz.itemObjectName).ToArray();
                if (objectsMatchingName.Length != 0) return objectsMatchingName.Last();
            }
            return null;
        }

        public ModdedItemIdentifier(ItemObject objct)
        {
            ItemMetaData meta = objct.GetMeta();
            if (meta == null) throw new NullReferenceException("Object: " + objct.name + " doesn't have meta! Can't create ModdedItemIdentifier!");
            version = 0;
            itemPluginGUID = meta.info.Metadata.GUID;
            itemObjectName = objct.name;
        }

        public static ModdedItemIdentifier Read(BinaryReader reader)
        {
            ModdedItemIdentifier dent = new ModdedItemIdentifier();
            dent.version = reader.ReadByte();
            dent.itemPluginGUID = reader.ReadString();
            dent.itemObjectName = reader.ReadString();
            return dent;
        }
    }

    public enum ModdedSaveLoadStatus
    {
        Success,
        NoSave,
        MissingHandlers,
        MissingItems,
        MismatchedTags
    }

    /// <summary>
    /// The root class for modded save games, storing the base game data, but restructured in a way that is more mod friendly.
    /// </summary>
    public class ModdedSaveGame
    {
        public List<ModdedItemIdentifier> items = new List<ModdedItemIdentifier>();
        public List<ModdedItemIdentifier> lockerItems = new List<ModdedItemIdentifier>();
        public Dictionary<string, string[]> modTags = new Dictionary<string, string[]>();
        public int levelId = 0;
        public int ytps = 0;
        public int lives = 2;
        public int seed = 0;
        public const int version = 3;
        public bool saveAvailable = false;
        public bool fieldTripPlayed = false;
        public bool johnnyHelped = false;
        public bool mapPurchased;
        public bool mapAvailable;
        public bool[] foundMapTiles = new bool[0];
        public int mapSizeX = 0;
        public int mapSizeZ = 0;
        internal static Dictionary<string, ModdedSaveGameIOBinary> ModdedSaveGameHandlers = new Dictionary<string, ModdedSaveGameIOBinary>();


        public void FillBlankModTags()
        {
            ModdedSaveGameHandlers.Do(x =>
            {
                if (!modTags.ContainsKey(x.Key))
                {
                    modTags.Add(x.Key, x.Value.GenerateTags());
                }
            });
        }

        public static void AddSaveHandler(ModdedSaveGameIOBinary handler)
        {
            if (handler.pluginInfo == null) throw new ArgumentNullException("You need to create a class that inherits from the ModdedSaveGameIOBinary class!");
            MTM101BaldiDevAPI.saveHandler = SavedGameDataHandler.Modded;
            ModdedSaveGameHandlers.Add(handler.pluginInfo.Metadata.GUID, handler);
        }

        public static void AddSaveHandler(PluginInfo info)
        {
            AddSaveHandler(new ModdedSaveGameIODummy(info));
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(MTM101BaldiDevAPI.VersionNumber);
            writer.Write(saveAvailable);
            writer.Write(version);
            writer.Write(ModdedSaveGameHandlers.Count);
            foreach (KeyValuePair<string, ModdedSaveGameIOBinary> kvp in ModdedSaveGameHandlers)
            {
                writer.Write(kvp.Key);
                string[] tags = kvp.Value.GenerateTags();
                writer.Write(tags.Length);
                for (int i = 0; i < tags.Length; i++)
                {
                    writer.Write(tags[i]);
                }
            }
            if (!saveAvailable) return;
            writer.Write(levelId);
            writer.Write(seed);
            writer.Write(ytps);
            writer.Write(lives);
            writer.Write(fieldTripPlayed);
            writer.Write(mapAvailable);
            writer.Write(mapPurchased);
            writer.Write(johnnyHelped);
            writer.Write(mapSizeX);
            writer.Write(mapSizeZ);
            writer.Write(foundMapTiles.Length);
            for (int i = 0; i < foundMapTiles.Length; i++)
            {
                writer.Write(foundMapTiles[i]);
            }
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Write(writer);
            }
            writer.Write(lockerItems.Count);
            for (int i = 0; i < lockerItems.Count; i++)
            {
                lockerItems[i].Write(writer);
            }
            foreach (KeyValuePair<string, ModdedSaveGameIOBinary> kvp in ModdedSaveGameHandlers)
            {
                kvp.Value.Save(writer);
            }
        }

        public static PartialModdedSavedGame PartialLoad(BinaryReader reader)
        {
            reader.ReadString();
            bool saveAvailable = reader.ReadBoolean();
            if (!saveAvailable)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length) //we must be in an older version, return early.
                {
                    return new PartialModdedSavedGame();
                }
            }
            int version = reader.ReadInt32();
            int modCount = reader.ReadInt32();
            List<string> modHandlers = new List<string>();
            Dictionary<string, string[]> modTags = new Dictionary<string, string[]>();
            for (int i = 0; i < modCount; i++)
            {
                string modHandler = reader.ReadString();
                modHandlers.Add(modHandler);
                if (version >= 3)
                {
                    int tagsToRead = reader.ReadInt32();
                    List<string> tags = new List<string>();
                    for (int j = 0; j < tagsToRead; j++)
                    {
                        tags.Add(reader.ReadString());
                    }
                    modTags.Add(modHandler, tags.ToArray());
                }
                else
                {
                    modTags.Add(modHandler, new string[0]);
                }
            }
            if (!saveAvailable)
            {
                return new PartialModdedSavedGame(modHandlers.ToArray(), modTags);
            }
            reader.ReadInt32();
            int seed = reader.ReadInt32();
            return new PartialModdedSavedGame(seed, modHandlers.ToArray(), modTags);
        }

        public ModdedSaveLoadStatus Load(BinaryReader reader, bool addMissingTags)
        {
            modTags.Clear();
            bool tagsMatch = true;
            reader.ReadString();
            saveAvailable = reader.ReadBoolean();
            if (!saveAvailable)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length) //we must be in an older version
                {
                    FillBlankModTags();
                    return ModdedSaveLoadStatus.NoSave;
                }
            }
            int version = reader.ReadInt32();
            int modCount = reader.ReadInt32();
            List<string> modHandlers = new List<string>();
            for (int i = 0; i < modCount; i++)
            {
                string modHandler = reader.ReadString();
                modHandlers.Add(modHandler);
                if (version >= 3)
                {
                    int tagsToRead = reader.ReadInt32();
                    List<string> tags = new List<string>();
                    for (int j = 0; j < tagsToRead; j++)
                    {
                        tags.Add(reader.ReadString());
                    }
                    modTags.Add(modHandler, tags.ToArray());
                    if (ModdedSaveGameHandlers.ContainsKey(modHandler))
                    {
                        if (ModdedSaveGameHandlers[modHandler].TagsReady())
                        {
                            string[] generatedTags = ModdedSaveGameHandlers[modHandler].GenerateTags();
                            // if the lengths dont match obviously the rest of the tags won't
                            if (generatedTags.Length != tagsToRead)
                            {
                                tagsMatch = false;
                                continue;
                            }
                            for (int j = 0; j < generatedTags.Length; j++)
                            {
                                if (!tags.Contains(generatedTags[j]))
                                {
                                    tagsMatch = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        tagsMatch = false;
                    }
                }
                else
                {
                    modTags.Add(modHandler, new string[0]);
                }
            }
            if (!saveAvailable) return ModdedSaveLoadStatus.NoSave;
            levelId = reader.ReadInt32();
            seed = reader.ReadInt32();
            ytps = reader.ReadInt32();
            lives = reader.ReadInt32();
            fieldTripPlayed = reader.ReadBoolean();
            // time to load all these new booleans!
            if (version >= 2)
            {
                mapAvailable = reader.ReadBoolean();
                mapPurchased = reader.ReadBoolean();
                johnnyHelped = reader.ReadBoolean();
            }
            mapSizeX = reader.ReadInt32();
            mapSizeZ = reader.ReadInt32();
            int foundTilesLength = reader.ReadInt32();
            foundMapTiles = new bool[foundTilesLength];
            for (int i = 0; i < foundTilesLength; i++)
            {
                foundMapTiles[i] = reader.ReadBoolean();
            }
            items.Clear();
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                ModdedItemIdentifier ident = ModdedItemIdentifier.Read(reader);
                if (ident.LocateObject() == null) return ModdedSaveLoadStatus.MissingItems;
                items.Add(ident);
            }
            // load lockers if we are in a save version that supports them
            if (version >= 2)
            {
                int lockerItemCount = reader.ReadInt32();
                for (int i = 0; i < lockerItemCount; i++)
                {
                    ModdedItemIdentifier ident = ModdedItemIdentifier.Read(reader);
                    if (ident.LocateObject() == null) return ModdedSaveLoadStatus.MissingItems;
                    lockerItems.Add(ident);
                }
            }
            // seperate the verification from the actual reading part so we dont partially load mod stuff.
            // we can gurantee that we can reset this class but nothing about the others.
            if (!tagsMatch) return ModdedSaveLoadStatus.MismatchedTags;
            for (int i = 0; i < modHandlers.Count; i++)
            {
                if (!ModdedSaveGameHandlers.ContainsKey(modHandlers[i])) return ModdedSaveLoadStatus.MissingHandlers;
            }
            for (int i = 0; i < modHandlers.Count; i++)
            {
                ModdedSaveGameHandlers[modHandlers[i]].Load(reader);
            }
            if (addMissingTags)
            {
                foreach (KeyValuePair<string, ModdedSaveGameIOBinary> kvp in ModdedSaveGameHandlers)
                {
                    if (modTags.ContainsKey(kvp.Key)) continue;
                    modTags.Add(kvp.Key, kvp.Value.GenerateTags());
                }
            }
            return ModdedSaveLoadStatus.Success;
        }
    }
}
