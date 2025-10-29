using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.IO;
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
        public virtual StickerStateData CreateStickerData(int activeLevel, bool opened)
        {
            return new ExtendedStickerStateData(sticker, activeLevel, opened);
        }
    }

    /// <summary>
    /// A version of ExtendedStickerData, but it returns a regular StickerStateData instead.
    /// </summary>
    public class VanillaCompatibleExtendedStickerData : ExtendedStickerData
    {
        public override StickerStateData CreateStickerData(int activeLevel, bool opened)
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

        public virtual void WriteState(BinaryWriter writer)
        {
            writer.Write((byte)0);
            writer.Write(activeLevel);
            writer.Write(opened);
        }

        public virtual void ReadState(BinaryReader reader)
        {
            reader.ReadByte(); // version
            activeLevel = reader.ReadInt32();
            opened = reader.ReadBoolean();
        }
    }

    public static class StickerManagerExtensions
    {
        public static StickerStateData AddSticker(this StickerManager me, Sticker sticker, bool opened, bool animation)
        {
            StickerStateData data = StickerMetaStorage.Instance.Get(sticker).value.CreateStickerData(0, opened);
            me.stickerInventory.Add(data);
            if (animation && opened)
            {
                Singleton<CoreGameManager>.Instance.GetHud(0).ShowCollectedSticker(me.GetInventoryStickerSprite(me.stickerInventory.Count - 1));
            }
            return data;
        }
    }
}
