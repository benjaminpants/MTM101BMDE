using BepInEx;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class StickerBuilder<T> where T : ExtendedStickerData, new()
    {
        PluginInfo info;
        string stickerEnumName = "";
        Sticker stickerEnum = Sticker.Nothing;
        Sprite sprite = null;
        bool affectsGenerator = false;
        float duplicateOdds = 1f;
        int cap = int.MaxValue;
        bool isBonus = false;
        string[] tags = new string[0];

        public StickerBuilder(PluginInfo info)
        {
            this.info = info;
        }

        /// <summary>
        /// Set the Items enum to use for the sticker.
        /// </summary>
        /// <param name="sticker"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetEnum(Sticker sticker)
        {
            stickerEnum = sticker;
            stickerEnumName = "";
            return this;
        }

        /// <summary>
        /// Creates an Stickers enum using EnumExtensions with the specified name.
        /// </summary>
        /// <param name="enumToRegister"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetEnum(string enumToRegister)
        {
            stickerEnum = Sticker.Nothing;
            stickerEnumName = enumToRegister;
            return this;
        }

        /// <summary>
        /// Sets the sprite for the sticker to use
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetSprite(Sprite sprite)
        {
            this.sprite = sprite;
            return this;
        }


        /// <summary>
        /// Sets the maximum return value for StickerManager.StickerValue for this sticker.
        /// Please only set this if your sticker would start throwing exceptions if it surpasses this value, otherwise, leave it uncapped for fun.
        /// </summary>
        /// <param name="cap"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetValueCap(int cap)
        {
            this.cap = cap;
            return this;
        }

        /// <summary>
        /// Sets the multiplier for duplicate odds for this sticker, ranging from 0-1, where the value is raised to the power of how many stickers of that type the player already has.
        /// </summary>
        /// <param name="odds"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetDuplicateOddsMultiplier(float odds)
        {
            duplicateOdds = odds;
            return this;
        }

        /// <summary>
        /// Marks this sticker as affecting the generator
        /// </summary>
        /// <returns></returns>
        public StickerBuilder<T> SetAsAffectingGenerator()
        {
            affectsGenerator = true;
            return this;
        }

        /// <summary>
        /// Marks this sticker as affecting the generator
        /// </summary>
        /// <returns></returns>
        public StickerBuilder<T> SetAsBonusSticker()
        {
            isBonus = true;
            return this;
        }

        /// <summary>
        /// Sets the metadata tags for this sticker
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetTagsArray(string[] tags)
        {
            this.tags = tags;
            return this;
        }

        /// <summary>
        /// Sets the metadata tags for this sticker
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public StickerBuilder<T> SetTags(params string[] tag)
        {
            return SetTagsArray(tag);
        }

        public T Build()
        {
            T stickerData = new T();
            stickerData.sprite = sprite;
            if (stickerEnumName == "")
            {
                if (stickerEnum == Sticker.Nothing) throw new Exception("You must assign an enum to the sticker!");
                stickerData.sticker = stickerEnum;
            }
            else
            {
                stickerData.sticker = EnumExtensions.ExtendEnum<Sticker>(stickerEnumName);
            }
            stickerData.stickerValueCap = cap;
            stickerData.affectsLevelGeneration = affectsGenerator;
            stickerData.duplicateOddsMultiplier = duplicateOdds;
            StickerMetaData tagsData = StickerMetaStorage.Instance.AddSticker(info, stickerData);
            tagsData.flags |= (isBonus ? StickerFlags.IsBonus : StickerFlags.None);
            tagsData.tags.UnionWith(tags);
            return stickerData;
        }
    }
}
