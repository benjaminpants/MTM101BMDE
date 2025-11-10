using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    // flags for basic item behaviors so mods know how to handle them
    [Flags]
    public enum ItemFlags
    {
        /// <summary>
        /// This item has no necessary flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// // This item has multiple uses like the grappling hook.
        /// </summary>
        MultipleUse = 1,
        /// <summary>
        /// This item should not appear in the players inventory and is used instantly upon pickup.
        /// </summary>
        InstantUse = 2,
        /// <summary>
        /// This item doesn't do anything when used, regardless of circumstance. This is for items like the Apple, but not the quarter as it can be used in machines.
        /// </summary>
        NoUses = 4,
        /// <summary>
        /// This item's behavior doesn't instantly destroy itself when used. This is applicable for the BSODA or the Techno Boots.
        /// </summary>
        Persists = 8,
        /// <summary>
        /// This item creates a physical entity in the world, this is applicable for the BSODA but not the Techno Boots.
        /// </summary>
        CreatesEntity = 16,
        /// <summary>
        /// This item has a variant in the tutorial that must be accounted for when handling MultipleUse.
        /// </summary>
        HasTutorialVariant = 32,
        /// <summary>
        /// This item is unobtainable and should not be given to the player (or have its .item instantiated) in any circumstance.
        /// </summary>
        Unobtainable = 64,
        // unimplemented
        /*
        /// <summary>
        /// This item is a runtime item, generated during the middle of a run.
        /// </summary>
        RuntimeItem = 128*/
    }

    public class ItemMetaData : IMetadata<ItemObject>
    {

        public ItemObject[] itemObjects; // for things like the grappling hook, the highest use count should be stored first.

        public ItemObject value => itemObjects.Last();

        public PluginInfo info => _info;
        private PluginInfo _info;

        public int generatorCost => value.value;
        public string nameKey => value.nameKey;

        public ItemFlags flags;
        public Items id => value.itemType;

        public HashSet<string> tags => _tags;
        HashSet<string> _tags = new HashSet<string>();

        public ItemMetaData(PluginInfo info, ItemObject itmObj)
        {
            itemObjects = new ItemObject[1] { itmObj };
            _info = info;
        }

        public ItemMetaData(PluginInfo info, ItemObject[] itmObjs)
        {
            itemObjects = itmObjs;
            _info = info;
        }
    }

    public class ItemMetaStorage : BasicMetaStorage<ItemMetaData, ItemObject>
    {
        public static ItemMetaStorage Instance => MTM101BaldiDevAPI.itemMetadata;

        static FieldInfo _value = AccessTools.Field(typeof(ITM_YTPs), "value");

        /// <summary>
        /// Get the ItemObject for the points item with the specified(or closest) value.
        /// </summary>
        /// <param name="points">The amount of points to try to search for</param>
        /// <param name="mustBeExact">If true, the point count must be exact, otherwise it will return null.</param>
        /// <returns>The found ItemObject, or the closest match if mustBeExact is false.</returns>
        public ItemObject GetPointsObject(int points, bool mustBeExact)
        {
            ItemObject[] pointItems = FindByEnum(Items.Points).itemObjects.Where(x => x.item is ITM_YTPs).ToArray(); //incase someone does something weird
            int[] pointValues = pointItems.Select(x => (int)_value.GetValue(x.item)).ToArray();
            ItemObject closestMatch = null;
            int closestMatchDiff = int.MaxValue;
            for (int i = 0; i < pointValues.Length; i++)
            {
                int diff = Math.Abs(pointValues[i] - points);
                if (diff < closestMatchDiff)
                {
                    closestMatch = pointItems[i];
                    closestMatchDiff = diff;
                }
            }
            if (mustBeExact && (closestMatchDiff != 0)) return null;
            return closestMatch;
        }

        public ItemMetaData FindByEnum(Items itm)
        {
            return Find(x =>
            {
                return x.id == itm;
            });
        }

        public ItemMetaData FindByEnumFromMod(Items itm, PluginInfo specificMod)
        {
            return Find(x =>
            {
                return (x.id == itm) && (x.info == specificMod);
            });
        }

        public ItemMetaData[] GetAllWithFlags(ItemFlags flag)
        {
            return FindAll(x =>
            {
                return x.flags.HasFlag(flag);
            }).Distinct().ToArray();
        }

        public ItemMetaData[] GetAllFromMod(PluginInfo mod)
        {
            return FindAll(x =>
            {
                return x.info == mod;
            }).Distinct().ToArray();
        }

        public ItemMetaData[] GetAllWithoutFlags(ItemFlags flag)
        {
            return FindAll(x =>
            {
                return !x.flags.HasFlag(flag);
            }).Distinct().ToArray();
        }

        public override bool Remove(ItemObject toRemove)
        {
            ItemMetaData meta = Get(toRemove);
            if (meta == null) return false;
            meta.itemObjects = meta.itemObjects.Where(x => x != toRemove).ToArray();
            if (meta.itemObjects.Length == 0)
            {
                return base.Remove(toRemove);
            }
            return true;
        }
    }
}
