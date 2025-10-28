using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class ItemBuilder
    {
        private string localizedText = "Unnamed";
        private string localizedDescription = "Unnamed\nNo description.";
        private Sprite smallSprite;
        private Sprite largeSprite;
        private Items itemEnum = Items.None;
        private string itemEnumName = "";
        private int price = 100;
        private int generatorCost = 10;
        private Type itemObjectType = null;
        private string[] tags = new string[0];
        private ItemFlags flags = ItemFlags.None;
        private PluginInfo info;
        private ItemMetaData metaDataToAddTo;
        private bool instantUse = false;
        private Item objectReference;
        private SoundObject pickupSoundOverride;
        private bool overrideDisabled = false;


        public ItemBuilder(PluginInfo info)
        {
            this.info = info;
        }

        /// <summary>
        /// Builds the ItemObject and creates the Item prefab if none is specified.
        /// </summary>
        /// <returns></returns>
        public ItemObject Build()
        {
            ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
            item.nameKey = localizedText;
            item.descKey = localizedDescription;
            item.name = "ItmOb_" + localizedText;
            item.itemType = itemEnum;
            if (itemEnumName != "")
            {
                // stop the warning from occuring
                if (EnumExtensions.EnumWithExtendedNameExists<Items>(itemEnumName))
                {
                    item.itemType = EnumExtensions.GetFromExtendedName<Items>(itemEnumName);
                }
                else
                {
                    item.itemType = EnumExtensions.ExtendEnum<Items>(itemEnumName);
                }
            }
            item.itemSpriteSmall = smallSprite;
            item.itemSpriteLarge = largeSprite;
            item.price = price;
            item.value = generatorCost;
            item.overrideDisabled = overrideDisabled;
            if (itemObjectType != null)
            {
                GameObject obj = new GameObject();
                obj.SetActive(false);
                Item comp = (Item)obj.AddComponent(itemObjectType);
                comp.name = "Obj" + item.name;
                item.item = comp;
                obj.ConvertToPrefab(true);
            }
            if (objectReference != null)
            {
                item.item = objectReference;
            }
            if (instantUse)
            {
                flags |= ItemFlags.InstantUse;
                item.addToInventory = false;
            }
            item.audPickupOverride = pickupSoundOverride;
            if (metaDataToAddTo != null)
            {
                metaDataToAddTo.itemObjects = metaDataToAddTo.itemObjects.AddToArray(item);
                item.AddMeta(metaDataToAddTo);
                return item;
            }
            ItemMetaData itemMeta = new ItemMetaData(info, item);
            itemMeta.tags.UnionWith(tags);
            itemMeta.flags = flags;
            item.AddMeta(itemMeta);
            return item;
        }

        /// <summary>
        /// Sets the metadata of the item.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public ItemBuilder SetMeta(ItemFlags flags, string[] tags)
        {
            this.flags = flags;
            this.tags = tags;
            return this;
        }

        /// <summary>
        /// Sets the pickup sound of the item to be something different from the default
        /// </summary>
        /// <param name="sound">The sound that will be played when this item is picked up.</param>
        /// <returns></returns>
        public ItemBuilder SetPickupSound(SoundObject sound)
        {
            pickupSoundOverride = sound;
            return this;
        }

        /// <summary>
        /// Sets the metadata of the item to an already existing metadata which this object will be appened to.
        /// </summary>
        /// <param name="existingMeta"></param>
        /// <returns></returns>
        public ItemBuilder SetMeta(ItemMetaData existingMeta)
        {
            metaDataToAddTo = existingMeta;
            return this;
        }

        /// <summary>
        /// Makes this object able to be used, even when the item manager is otherwise disabled. (Such as Johnny's Shop)
        /// </summary>
        /// <returns></returns>
        public ItemBuilder SetAsNotOverridable()
        {
            overrideDisabled = true;
            return this;
        }

        /// <summary>
        /// Sets the type of the item component to add to the created Item prefab.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ItemBuilder SetItemComponent<T>() where T : Item
        {
            itemObjectType = typeof(T);
            objectReference = null;
            return this;
        }

        /// <summary>
        /// Sets the item component to an already existing gameObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public ItemBuilder SetItemComponent<T>(T gameObject) where T : Item
        {
            itemObjectType = null;
            objectReference = gameObject;
            return this;
        }

        /// <summary>
        /// Sets the name of the item and its description in the shop.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ItemBuilder SetNameAndDescription(string name, string description)
        {
            localizedText = name;
            localizedDescription = description;
            return this;
        }

        /// <summary>
        /// Makes the item be used instantly when collected through a pickup.
        /// </summary>
        /// <returns></returns>
        public ItemBuilder SetAsInstantUse()
        {
            instantUse = true;
            return this;
        }

        /// <summary>
        /// Sets the big and large sprites respectively.
        /// </summary>
        /// <param name="small">The small sprite, usually with a pixelsPerUnit of 25.</param>
        /// <param name="large">The large sprite, usually with a pixelsPerUnit of 50.</param>
        /// <returns></returns>
        public ItemBuilder SetSprites(Sprite small, Sprite large) 
        {
            smallSprite = small;
            largeSprite = large;
            return this;
        }

        /// <summary>
        /// Set the Items enum to use for the item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ItemBuilder SetEnum(Items item)
        {
            itemEnum = item;
            itemEnumName = "";
            return this;
        }
        
        /// <summary>
        /// Creates an Items enum using EnumExtensions with the specified name.
        /// </summary>
        /// <param name="enumToRegister"></param>
        /// <returns></returns>
        public ItemBuilder SetEnum(string enumToRegister)
        {
            itemEnum = Items.None;
            itemEnumName = enumToRegister;
            return this;
        }

        /// <summary>
        /// Sets the item's price in the shop.
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public ItemBuilder SetShopPrice(int price)
        {
            this.price = price;
            return this;
        }

        /// <summary>
        /// Sets the cost of the item for the generator.
        /// Each level has a budget it can spend on items.
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public ItemBuilder SetGeneratorCost(int cost)
        {
            generatorCost = cost;
            return this;
        }

    }
}
