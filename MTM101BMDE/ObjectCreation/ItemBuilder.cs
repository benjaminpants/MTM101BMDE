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
        private bool deactiveObject;
        private string[] tags = new string[0];
        private ItemFlags flags = ItemFlags.None;
        private PluginInfo info;
        private ItemMetaData metaDataToAddTo;


        public ItemBuilder(PluginInfo info)
        {
            this.info = info;
        }

        public ItemObject Build()
        {
            ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
            item.nameKey = localizedText;
            item.descKey = localizedDescription;
            item.name = "ItmOb_" + localizedText;
            item.itemType = itemEnum;
            if (itemEnumName != "")
            {
                item.itemType = EnumExtensions.ExtendEnum<Items>(itemEnumName);
            }
            item.itemSpriteSmall = smallSprite;
            item.itemSpriteLarge = largeSprite;
            item.price = price;
            item.cost = generatorCost;
            if (itemObjectType != null)
            {
                GameObject obj = new GameObject();
                if (deactiveObject)
                {
                    obj.SetActive(false);
                }
                Item comp = (Item)obj.AddComponent(itemObjectType);
                comp.name = "Obj" + item.name;
                item.item = comp;
                GameObject.DontDestroyOnLoad(obj);
            }
            if (metaDataToAddTo != null)
            {
                metaDataToAddTo.itemObjects = metaDataToAddTo.itemObjects.AddToArray(item);
                return item;
            }
            ItemMetaData itemMeta = new ItemMetaData(info, item);
            itemMeta.tags.AddRange(tags);
            itemMeta.flags = flags;
            return item;
        }

        public ItemBuilder SetMeta(ItemFlags flags, string[] tags)
        {
            this.flags = flags;
            this.tags = tags;
            return this;
        }

        public ItemBuilder SetMeta(ItemMetaData existingMeta)
        {
            metaDataToAddTo = existingMeta;
            return this;
        }

        public ItemBuilder SetItemComponent<T>(bool startActive = true) where T : Item
        {
            itemObjectType = typeof(T);
            deactiveObject = !startActive;
            return this;
        }

        public ItemBuilder SetNameAndDescription(string name, string description)
        {
            localizedText = name;
            localizedDescription = description;
            return this;
        }

        public ItemBuilder SetSprites(Sprite small, Sprite large) 
        {
            smallSprite = small;
            largeSprite = large;
            return this;
        }

        public ItemBuilder SetEnum(Items item)
        {
            itemEnum = item;
            itemEnumName = "";
            return this;
        }

        public ItemBuilder SetEnum(string enumToRegister)
        {
            itemEnum = Items.None;
            itemEnumName = enumToRegister;
            return this;
        }

        public ItemBuilder SetShopPrice(int price)
        {
            this.price = price;
            return this;
        }

        public ItemBuilder SetGeneratorCost(int cost)
        {
            generatorCost = cost;
            return this;
        }

    }
}
