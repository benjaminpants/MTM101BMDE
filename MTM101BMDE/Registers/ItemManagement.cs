using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    // flags for basic item behaviors so mods know how to handle them
    [Flags]
    public enum ItemFlags
    {
        None = 0, // This item has no necessary flags.
        MultipleUse = 1, // This item has multiple uses like the grappling hook.
        NoInventory = 2, // This item should not appear in the players inventory. This is useful for stuff like presents or non-items that use the Pickup system.
        NoUses = 4, // This item doesn't do anything when used, regardless of circumstance. This is for items like the Apple, but not the quarter as it can be used in machines.
        Persists = 8, // This item's behavior doesn't instantly destroy itself when used. This is applicable for the BSODA or the Big Ol' Boots.
        Physical = 16 // This item creates a physical object in the world, this is applicable for the BSODA but not the Big Ol' Boots.
    }

    public class ItemMetaData : IMetadata<ItemObject>
    {
        public ItemObject[] itemObjects; // for things like the grappling hook, the highest use count should be stored first.

        public ItemObject value => itemObjects[0];

        public PluginInfo info => _info;
        private PluginInfo _info;

        public int generatorCost => value.cost;
        public string nameKey => value.nameKey;

        public ItemFlags flags;
        public Items id => value.itemType;

        public List<string> tags => _tags;
        List<string> _tags = new List<string>();

        public ItemMetaData(BaseUnityPlugin plug, ItemObject itmObj)
        {
            itemObjects = new ItemObject[1] { itmObj };
            _info = plug.Info;
        }

        public ItemMetaData(BaseUnityPlugin plug, ItemObject[] itmObjs)
        {
            itemObjects = itmObjs;
            _info = plug.Info;
        }
    }

    public class ItemMetaStorage : MetaStorage<ItemMetaData, ItemObject>
    {
        public ItemMetaData FindByEnum(Items itm)
        {
            return Find(x =>
            {
                return x.id == itm;
            });
        }

        public ItemMetaData[] GetAllWithFlags(ItemFlags flag)
        {
            return FindAll(x =>
            {
                return x.flags.HasFlag(flag);
            });
        }
    }
}
