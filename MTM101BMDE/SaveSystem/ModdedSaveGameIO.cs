using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.SaveSystem
{
    // The main ModdedSaveGameIO class, these are to be inherited.
    // Useful for people who want their savedata to be small. Or people who just like using BinaryReader/BinaryWriter
    public abstract class ModdedSaveGameIOBinary
    {
        public abstract PluginInfo pluginInfo { get; }

        public abstract void Save(BinaryWriter writer);

        public abstract void Load(BinaryReader reader);

        public abstract void Reset();
    }

    // A version of ModdedSaveGameIOBinary, but for saving/loading plaintext.
    // Useful for people who want to do something like splitting a comma seeperated string for their data.
    public abstract class ModdedSaveGameIOText : ModdedSaveGameIOBinary
    {
        public abstract string SaveText();

        public abstract void LoadText(string toLoad);

        public override void Save(BinaryWriter writer)
        {
            writer.Write(SaveText());
        }

        public override void Load(BinaryReader reader)
        {
            LoadText(reader.ReadString());
        }
    }

    // A version of ModdedSaveGameIOText but it saves Json data.
    // Useful for people who just want to serialize/deserialize an object and need not much else.
    public abstract class ModdedSaveGameIOJson<T> : ModdedSaveGameIOText
    {
        public abstract T GetObjectToSave();

        public abstract void OnObjectLoaded(T loadedObject);

        public override string SaveText()
        {
            return JsonUtility.ToJson(GetObjectToSave());
        }

        public override void LoadText(string toLoad)
        {
            OnObjectLoaded(JsonUtility.FromJson<T>(toLoad));
        }
    }

    // A dummy ModdedSaveGameIO used for changing the save type to Modded.
    // Writes a 0 so if they want to add data in the future all they have to do is check if the first byte is zero to see if they already have data
    internal class ModdedSaveGameIODummy : ModdedSaveGameIOBinary
    {
        private PluginInfo _info;
        public override PluginInfo pluginInfo => _info;

        internal ModdedSaveGameIODummy(PluginInfo info)
        {
            _info = info;
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write((byte)0);
        }

        public override void Load(BinaryReader reader)
        {
            reader.ReadByte();
        }

        public override void Reset()
        {

        }
    }
}
