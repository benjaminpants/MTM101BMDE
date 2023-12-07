using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public class ObjectBuilderMeta : IMetadata<ObjectBuilder>
    {
        public Obstacle obstacle => builder.obstacle;

        private ObjectBuilder builder;
        public ObjectBuilder value => builder;

        private List<string> _tags = new List<string>();
        public List<string> tags => _tags;

        private PluginInfo _info;
        public PluginInfo info => _info;

        public ObjectBuilderMeta(PluginInfo info, ObjectBuilder builder)
        {
            _info = info;
            this.builder = builder;
        }
    }

    public class ObjectBuilderMetaStorage : IMetadataStorage<ObjectBuilderMeta, ObjectBuilder, ObjectBuilder>
    {
        public static ObjectBuilderMetaStorage Instance => MTM101BaldiDevAPI.objBuilderMeta;

        private Dictionary<Obstacle, Dictionary<string, ObjectBuilderMeta>> metas = new Dictionary<Obstacle, Dictionary<string, ObjectBuilderMeta>>();
        private Dictionary<Obstacle, string> defaultKeys = new Dictionary<Obstacle, string>();

        public void Add(ObjectBuilderMeta toAdd)
        {
            Add(toAdd.obstacle, toAdd);
        }

        public void Add(Obstacle obst, ObjectBuilderMeta toAdd)
        {
            Add(obst, toAdd, toAdd.value.name);
        }

        private void Add(Obstacle obst, ObjectBuilderMeta toAdd, string key)
        {
            Dictionary<string, ObjectBuilderMeta> dictToAdd;
            if (!metas.ContainsKey(obst))
            {
                dictToAdd = new Dictionary<string, ObjectBuilderMeta>();
                defaultKeys.Add(obst, key);
                metas.Add(obst, dictToAdd);
            }
            else
            {
                dictToAdd = metas[obst];
            }
            dictToAdd.Add(key, toAdd);
        }

        public void Add(ObjectBuilderMeta toAdd, string key)
        {
            Add(toAdd.obstacle, toAdd, key);
        }

        public ObjectBuilderMeta[] All()
        {
            List<ObjectBuilderMeta> metalist = new List<ObjectBuilderMeta>();
            metas.Values.Do(x =>
            {
                metalist.AddRange(x.Values);
            });
            return metalist.ToArray();
        }

        public ObjectBuilderMeta Find(Predicate<ObjectBuilderMeta> predicate)
        {
            return All().ToList().Find(predicate);
        }

        public ObjectBuilderMeta[] FindAll(Predicate<ObjectBuilderMeta> predicate)
        {
            return All().ToList().FindAll(predicate).ToArray();
        }

        public ObjectBuilderMeta[] FindAllWithTags(bool matchAll, params string[] tags)
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

        public ObjectBuilderMeta Get(ObjectBuilder key)
        {
            return Find(x => x.value == key);
        }

        public ObjectBuilderMeta Get(Obstacle obst)
        {
            if (!metas.ContainsKey(obst)) return null;
            return metas[obst][defaultKeys[obst]];
        }

        public ObjectBuilderMeta Get(Obstacle obst, string key)
        {
            if (!metas.ContainsKey(obst)) return null;
            if (!metas[obst].ContainsKey(key)) return null;
            return metas[obst][key];
        }
    }
}
