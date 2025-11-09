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
        public Sticker sticker;
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
            return CreateStateData(Singleton<BaseGameManager>.Instance.CurrentLevel, true, inventoryState.sticky);
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
        /// <param name="sticky"></param>
        /// <returns></returns>
        public virtual StickerStateData CreateStateData(int activeLevel, bool opened, bool sticky)
        {
            return new ExtendedStickerStateData(sticker, activeLevel, opened, sticky);
        }

        /// <summary>
        /// This is called when the sticker is actually going to be applied.
        /// This just calls CreateOrGetAppliedStateData and sets the slot to the return value.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="inventoryState"></param>
        /// <param name="slot"></param>
        public virtual void ApplySticker(StickerManager manager, StickerStateData inventoryState, int slot)
        {
            manager.activeStickerData[slot] = CreateStateData(Singleton<BaseGameManager>.Instance.CurrentLevel, true, inventoryState.sticky);
        }
    }

    public class ExtendedGluestickData : VanillaCompatibleExtendedStickerData
    {
        public override void ApplySticker(StickerManager manager, StickerStateData inventoryState, int slot)
        {
            manager.UpgradeSlot(slot);
        }
    }

    /// <summary>
    /// A version of ExtendedStickerData, but it returns a regular StickerStateData instead.
    /// </summary>
    public class VanillaCompatibleExtendedStickerData : ExtendedStickerData
    {
        public override StickerStateData CreateStateData(int activeLevel, bool opened, bool sticky)
        {
            return new StickerStateData(sticker, activeLevel, opened, sticky);
        }
    }

    [Serializable]
    public class ExtendedStickerStateData : StickerStateData
    {
        public ExtendedStickerStateData(Sticker sticker, int activeLevel, bool opened, bool sticky) : base(sticker, activeLevel, opened, sticky)
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
            writer.Write((byte)1);
            writer.Write(data.activeLevel);
            writer.Write(data.opened);
            writer.Write(data.sticky);
        }

        public static void ReadDefault(BinaryReader reader, StickerStateData data)
        {
            byte version = reader.ReadByte(); // version
            data.activeLevel = reader.ReadInt32();
            data.opened = reader.ReadBoolean();
            if (version == 0) return;
            data.sticky = reader.ReadBoolean();
        }
    }

    public static class StickerManagerExtensions
    {
        public static StickerStateData AddSticker(this StickerManager me, Sticker sticker, bool opened, bool sticky, bool animation)
        {
            return AddExistingSticker(me, StickerMetaStorage.Instance.Get(sticker).value.CreateStateData(0, opened, sticky), opened, animation);
        }

        public static StickerStateData AddRandomSticker(this StickerManager me, WeightedSticker[] potentialStickers, bool opened, bool sticky, bool animation)
        {
            return AddSticker(me, WeightedSticker.RandomSelection(potentialStickers), opened, sticky, animation);
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
                ExtendedStickerStateData newData = (ExtendedStickerStateData)StickerMetaStorage.Instance.Get(me.sticker).value.CreateStateData(me.activeLevel, me.opened, me.sticky);
                BinaryReader reader = new BinaryReader(memoryStream);
                newData.ReadInto(reader);
                return newData;
            }
            return new StickerStateData(me.sticker, me.activeLevel, me.opened, me.sticky);
        }
    }
}
