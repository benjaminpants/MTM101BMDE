using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    // flags for various important things about a room
    [Flags]
    public enum RoomFlags
    {
        None = 0, // This room has no necessary flags.
        NonRectangle = 1, // This room is not a rectangle.
    }

    public class RoomAssetMetaStorage : IMetadataStorage<RoomAssetMeta, RoomCategory, RoomAsset>
    {

        private Dictionary<RoomCategory, Dictionary<string, RoomAssetMeta>> metas = new Dictionary<RoomCategory, Dictionary<string, RoomAssetMeta>>();

        public static RoomAssetMetaStorage Instance => MTM101BaldiDevAPI.roomAssetMeta;

        public void Add(RoomAssetMeta toAdd)
        {
            if (!metas.ContainsKey(toAdd.category))
            {
                metas.Add(toAdd.category, new Dictionary<string, RoomAssetMeta>());
            }
            /*if (metas[toAdd.category].ContainsKey(toAdd.name))
            {
                MTM101BaldiDevAPI.Log.LogWarning("Duplicate name (" + toAdd.name + ") found, defaulting to ScriptableObjectName...");
                metas[toAdd.category].Add(((UnityEngine.ScriptableObject)toAdd.value).name, toAdd);
                return;
            }*/
            metas[toAdd.category].Add(toAdd.name, toAdd);
        }

        public RoomAssetMeta[] All()
        {
            List<RoomAssetMeta> metalist = new List<RoomAssetMeta>();
            metas.Values.Do(x =>
            {
                metalist.AddRange(x.Values);
            });
            return metalist.ToArray();
        }

        public RoomAssetMeta[] AllOfCategory(RoomCategory cat)
        {
            if (!metas.ContainsKey(cat)) return new RoomAssetMeta[0];
            return metas[cat].Values.ToArray();
        }

        public RoomAssetMeta Find(Predicate<RoomAssetMeta> predicate)
        {
            return All().ToList().Find(predicate);
        }

        public RoomAssetMeta[] FindAll(Predicate<RoomAssetMeta> predicate)
        {
            return All().ToList().FindAll(predicate).ToArray();
        }

        public RoomAssetMeta[] FindAllWithTags(bool matchAll, params string[] tags)
        {
            return FindAll(x =>
            {
                foreach (string tag in x.tags)
                {
                    // if it contains the tag and we don't need to match all, return true, otherwise continue past the return false
                    if (tags.Contains(tag))
                    {
                        if (!matchAll)
                        {
                            return true;
                        }
                        continue;
                    }
                    return false;
                }
                return true;
            });
        }

        public RoomAssetMeta Get(RoomCategory key)
        {
            return metas[key].First().Value;
        }

        public RoomAssetMeta Get(string name)
        {
            return Find(x => x.name == name);
        }

        public RoomAssetMeta Get(RoomCategory cat, string name)
        {
            if (!metas.ContainsKey(cat)) return null;
            return metas[cat][name];
        }
    }

    public class RoomAssetMeta : IMetadata<RoomAsset>
    {

        public RoomCategory category => rAsset.category;

        public string name => ((UnityEngine.ScriptableObject)rAsset).name; //get the actual object name

        public string plusName => rAsset.name; // todo: figure out the purpose of RoomAsset.name

        public IntVector2 size { private set; get; }

        public RoomFlags flags { private set; get; }

        private RoomAsset rAsset;
        public RoomAsset value => rAsset;

        private List<string> _tags = new List<string>();
        public List<string> tags => _tags;

        public PluginInfo info => _info;
        private PluginInfo _info;

        public RoomAssetMeta(PluginInfo plugfo, RoomAsset roomAsset)
        {
            _info = plugfo;
            rAsset = roomAsset;
            UpdateFlags();
        }

        private RoomFlags UpdateFlags()
        {
            RoomFlags flags = RoomFlags.None;
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int i = 0; i < rAsset.cells.Count; i++)
            {
                minX = Math.Min(minX, rAsset.cells[i].pos.x);
                minY = Math.Min(minY, rAsset.cells[i].pos.z);
                maxX = Math.Max(minX, rAsset.cells[i].pos.x);
                maxY = Math.Max(minY, rAsset.cells[i].pos.z);
            }
            size = new IntVector2(maxX - minX, maxY - minY);
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (rAsset.GetCellIndexAt(x,y) == -1)
                    {
                        flags |= RoomFlags.NonRectangle;
                        goto RectangleCalculationDone;
                    }
                }
            }
        RectangleCalculationDone:
            return flags;
        }
    }
}
