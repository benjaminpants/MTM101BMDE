using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// TODO: rewrite this whole thing(possibly for BB+ 0.5..?) to better encourage the following structure:
// a static variable with a class
// a monobehavior attached to CoreGameManager that stores the actual data while you are playing
// the save function transferring data to the static class
// TODO: figure out how to do this
namespace MTM101BaldAPI.SaveSystem
{
    /// <summary>
    /// The main ModdedSaveGameIO class, these are to be inherited.
    /// Useful for people who want their savedata to be small. Or people who just like using BinaryReader/BinaryWriter
    /// </summary>
    public abstract class ModdedSaveGameIOBinary
    {
        public abstract PluginInfo pluginInfo { get; }

        /// <summary>
        /// When the game is saved, this function will be called to write the data.
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Save(BinaryWriter writer);

        /// <summary>
        /// When the game is loaded, this function will be called to read the data.
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Load(BinaryReader reader);

        /// <summary>
        /// When the CoreGameManager is created, this function will be called. Use this to attach a monobehavior that stores the data while play is in session.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="isFromSavedGame"></param>
        public virtual void OnCGMCreated(CoreGameManager instance, bool isFromSavedGame)
        {
            // do nothing
        }

        /// <summary>
        /// When a file is deleted, this function will be called. Use it to restore settings to default.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Generates the current set of tags. 
        /// Used to seperate save files with different settings that effect generation, such as a toggle to make a character spawn always, for example.
        /// </summary>
        /// <returns></returns>
        public virtual string[] GenerateTags()
        {
            return new string[0];
        }

        /// <summary>
        /// Displays the tags in a way that is more readable for players looking at their modded saves to figure out why they are different.
        /// Generally, you should keep tags readable even if DisplayTagsDefault is used like it will be if this mod isn't actively installed.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public virtual string DisplayTags(string[] tags)
        {
            return DisplayTagsDefault(tags);
        }

        /// <summary>
        /// The default function for displaying tags. Will be used if the mod is uninstalled or DisplayTags isn't overwritten.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string DisplayTagsDefault(string[] tags)
        {
            if (tags.Length == 0)
            {
                return "No tags.";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>Tags:</b>");
            for (int i = 0; i < tags.Length; i++)
            {
                sb.AppendLine(tags[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Determines if tags are ready to be generated.
        /// Return false if tags are not ready to be generated yet, making the API load the first save with matching mods, ignoring tags for this mod.
        /// It is suggested to manually call the RegenerateTags function once tags are ready to be generated.
        /// <b>This does not stop GenerateTags from being called!</b>
        /// </summary>
        /// <returns></returns>
        public virtual bool TagsReady()
        {
            return true;
        }
    }

    /// <summary>
    /// A version of ModdedSaveGameIOBinary, but for saving/loading plaintext.
    /// Useful for people who want to do something like splitting a comma seeperated string for their data.
    /// </summary>
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

    /// <summary>
    /// A version of ModdedSaveGameIOText but it saves Json data.
    /// Useful for people who just want to serialize/deserialize an object and need not much else.
    /// </summary>
    /// <typeparam name="T">The type of object you want to serialize.</typeparam>
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

    /// <summary>
    /// A dummy ModdedSaveGameIO used for changing the save type to Modded for custom items.
    /// Simply writes a zero. Switch to ModdedSaveGameIOBinary and check for that zero to see if there is data to load if in the future your mod needs data.
    /// </summary>
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
            // do nothing
        }
    }
}
