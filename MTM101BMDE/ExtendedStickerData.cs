using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public class ExtendedStickerData : StickerData
    {
        public int stickerValueCap = int.MaxValue;

        /// <summary>
        /// Returns true if this sticker can be applied.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBeApplied()
        {
            if (!affectsLevelGeneration) return true;
            return (Singleton<BaseGameManager>.Instance.InPitstop() && Singleton<BaseGameManager>.Instance.CurrentStickerLevel() != Singleton<CoreGameManager>.Instance.lastLevelNumber);
        }

        /// <summary>
        /// Returns the sprite this sticker uses when applied.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Sprite GetAppliedSprite(StickerStateData data)
        {
            return sprite;
        }

        /// <summary>
        /// Returns true if this sticker can be covered/replaced.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool CanBeCovered(StickerStateData data)
        {
            if (!affectsLevelGeneration) return true;
            return data.activeLevel != Singleton<BaseGameManager>.Instance.CurrentStickerLevel();
        }

        /// <summary>
        /// The state that gets put into the active stickers array.
        /// The default behavior is to use CreateStickerData to make a copy of inventoryState,
        /// but you may want to override this method.
        /// </summary>
        /// <param name="inventoryState"></param>
        /// <returns></returns>
        public virtual StickerStateData CreateOrGetAppliedStateData(StickerStateData inventoryState)
        {
            return CreateStateData(Singleton<BaseGameManager>.Instance.CurrentLevel, true);
        }

        /// <summary>
        /// Returns the localized string used when this sticker is applied.
        /// </summary>
        /// <returns></returns>
        public virtual string GetLocalizedAppliedStickerDescription(StickerStateData data)
        {
            return string.Format("{0}<br><br>{1}", Singleton<LocalizationManager>.Instance.GetLocalizedText(string.Format("StickerTitle_{0}", EnumExtensions.GetExtendedName<Sticker>((int)sticker))), Singleton<LocalizationManager>.Instance.GetLocalizedText(string.Format("StickerDescription_{0}", EnumExtensions.GetExtendedName<Sticker>((int)sticker))));
        }

        /// <summary>
        /// Returns the localized string used when this sticker is in the sticker inventory.
        /// </summary>
        /// <returns></returns>
        public virtual string GetLocalizedInventoryStickerDescription(StickerStateData data)
        {
            return string.Format("{0}<br><br>{1}", Singleton<LocalizationManager>.Instance.GetLocalizedText(string.Format("StickerTitle_{0}", EnumExtensions.GetExtendedName<Sticker>((int)sticker))), Singleton<LocalizationManager>.Instance.GetLocalizedText(string.Format("StickerDescription_{0}", EnumExtensions.GetExtendedName<Sticker>((int)sticker))));
        }

        /// <summary>
        /// Creates the data for the specified sticker.
        /// </summary>
        /// <param name="activeLevel"></param>
        /// <param name="opened"></param>
        /// <returns></returns>
        public virtual StickerStateData CreateStateData(int activeLevel, bool opened)
        {
            return new ExtendedStickerStateData(sticker, activeLevel, opened);
        }
    }

    /// <summary>
    /// A version of ExtendedStickerData, but it returns a regular StickerStateData instead.
    /// </summary>
    public class VanillaCompatibleExtendedStickerData : ExtendedStickerData
    {
        public override StickerStateData CreateStateData(int activeLevel, bool opened)
        {
            return new StickerStateData(sticker, activeLevel, opened);
        }
    }

    [Serializable]
    public class ExtendedStickerStateData : StickerStateData
    {
        public ExtendedStickerStateData(Sticker sticker, int activeLevel, bool opened) : base(sticker, activeLevel, opened)
        {
        }

        public virtual void Write(BinaryWriter writer)
        {
            WriteDefault(writer, this);
        }

        public virtual void ReadInto(BinaryReader reader)
        {
            ReadDefault(reader, this);
        }

        public static void WriteDefault(BinaryWriter writer, StickerStateData data)
        {
            writer.Write((byte)0);
            writer.Write(data.activeLevel);
            writer.Write(data.opened);
        }

        public static void ReadDefault(BinaryReader reader, StickerStateData data)
        {
            reader.ReadByte(); // version
            data.activeLevel = reader.ReadInt32();
            data.opened = reader.ReadBoolean();
        }
    }

    public static class StickerManagerExtensions
    {
        public static StickerStateData AddSticker(this StickerManager me, Sticker sticker, bool opened, bool animation)
        {
            return AddExistingSticker(me, StickerMetaStorage.Instance.Get(sticker).value.CreateStateData(0, opened), opened, animation);
        }

        public static StickerStateData AddExistingSticker(this StickerManager me, StickerStateData data, bool opened, bool animation)
        {
            me.stickerInventory.Add(data);
            if (animation && opened)
            {
                Singleton<CoreGameManager>.Instance.GetHud(0).ShowCollectedSticker(me.GetInventoryStickerSprite(me.stickerInventory.Count - 1));
            }
            return data;
        }

        static FieldInfo _OnStickerApplied = AccessTools.Field(typeof(StickerManager), "OnStickerApplied");

        public static StickerStateData ApplyExistingSticker(this StickerManager me, StickerStateData data, int slot)
        {
            me.activeStickerData[slot] = data;
            ((StickerManager.StickerAppliedDelegate)_OnStickerApplied.GetValue(me)).Invoke();
            return data;
        }

        public static StickerStateData MakeCopy(this StickerStateData me)
        {
            if (me is ExtendedStickerStateData)
            {
                MemoryStream memoryStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.Default, true);
                ((ExtendedStickerStateData)me).Write(writer);
                writer.Dispose();
                memoryStream.Position = 0;
                ExtendedStickerStateData newData = (ExtendedStickerStateData)StickerMetaStorage.Instance.Get(me.sticker).value.CreateStateData(me.activeLevel, me.opened);
                BinaryReader reader = new BinaryReader(memoryStream);
                newData.ReadInto(reader);
                return newData;
            }
            return new StickerStateData(me.sticker, me.activeLevel, me.opened);
        }
    }
}
