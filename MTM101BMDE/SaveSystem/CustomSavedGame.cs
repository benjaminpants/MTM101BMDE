﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MTM101BaldAPI.Registers;
using System.Linq;

namespace MTM101BaldAPI.SaveSystem
{
    // stores the name of a few parts of an item to make sure it can be found again if, for one reason or another it gets changed.
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
        MissingItems
    }

    // The root class for modded save games, storing the base game data, but restructured in a way that is more mod friendly.
    public class ModdedSaveGame
    {
        public List<ModdedItemIdentifier> items = new List<ModdedItemIdentifier>(); // use a list instead of a standard
        public int levelId = 0;
        public int ytps = 0;
        public int lives = 2;
        public int seed = 0;
        public int version = 0;
        public bool saveAvailable = false;
        public bool fieldTripPlayed = false;
        public bool[] foundMapTiles = new bool[0];
        public int mapSizeX = 0;
        public int mapSizeZ = 0;
        internal static Dictionary<string, ModdedSaveGameIOBinary> ModdedSaveGameHandlers = new Dictionary<string, ModdedSaveGameIOBinary>();

        public static void AddSaveHandler(ModdedSaveGameIOBinary handler)
        {
            if (handler.pluginInfo == null) throw new ArgumentNullException("You need to create a class that inherits from the ModdedSaveGameIOBinary class!");
            MTM101BaldiDevAPI.saveHandler = SavedGameDataHandler.Modded;
            ModdedSaveGameHandlers.Add(handler.pluginInfo.Metadata.GUID, handler);
        }

        public void Save(BinaryWriter writer, bool writeWarning = true)
        {
            writer.Write(writeWarning ? "Hey, YOU! Yeah, YOU! If you want to edit this file, use a hex editor, as this is not a text file!" : MTM101BaldiDevAPI.VersionNumber);
            writer.Write(saveAvailable);
            if (!saveAvailable) return;
            writer.Write(version);
            writer.Write(ModdedSaveGameHandlers.Count);
            foreach (KeyValuePair<string, ModdedSaveGameIOBinary> kvp in ModdedSaveGameHandlers)
            {
                writer.Write(kvp.Key);
            }
            writer.Write(levelId);
            writer.Write(seed);
            writer.Write(ytps);
            writer.Write(lives);
            writer.Write(fieldTripPlayed);
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
            foreach (KeyValuePair<string, ModdedSaveGameIOBinary> kvp in ModdedSaveGameHandlers)
            {
                kvp.Value.Save(writer);
            }
        }

        public ModdedSaveLoadStatus Load(BinaryReader reader)
        {
            reader.ReadString();
            saveAvailable = reader.ReadBoolean();
            if (!saveAvailable) return ModdedSaveLoadStatus.NoSave;
            version = reader.ReadInt32();
            int modCount = reader.ReadInt32();
            List<string> modHandlers = new List<string>();
            for (int i = 0; i < modCount; i++)
            {
                modHandlers.Add(reader.ReadString());
            }
            levelId = reader.ReadInt32();
            seed = reader.ReadInt32();
            ytps = reader.ReadInt32();
            lives = reader.ReadInt32();
            fieldTripPlayed = reader.ReadBoolean();
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
            // seperate the verification from the actual reading part so we dont partially load mod stuff.
            // we can gurantee that we can reset this class but nothing about the others.
            for (int i = 0; i < modHandlers.Count; i++)
            {
                if (!ModdedSaveGameHandlers.ContainsKey(modHandlers[i])) return ModdedSaveLoadStatus.MissingHandlers;
            }
            for (int i = 0; i < modHandlers.Count; i++)
            {
                ModdedSaveGameHandlers[modHandlers[i]].Load(reader);
            }
            return ModdedSaveLoadStatus.Success;
        }
    }
}