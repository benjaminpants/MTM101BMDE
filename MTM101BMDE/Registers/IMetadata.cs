using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public interface IMetadata<T>
    {
        T value { get; }

        List<string> tags { get; }

        PluginInfo info { get; }
    }

    public interface IMetadataStorage<T, T2> where T : IMetadata<T2>
    {
        T Get(T2 key);

        T[] FindAllWithTags(string[] tags, bool matchAll);

        T Find(Predicate<T> predicate);

        T[] FindAll(Predicate<T> predicate);

        void Add(T toAdd);

        T[] All();
    }

    public class MetaStorage<T1, T2> : IMetadataStorage<T1, T2> where T1 : IMetadata<T2>
    {
        private Dictionary<T2, T1> metas = new Dictionary<T2, T1>();

        public void Add(T1 toAdd)
        {
            Add(toAdd.value, toAdd);
        }

        public void Add(T2 itm, T1 toAdd)
        {
            metas.Add(itm, toAdd);
        }

        public T1[] All()
        {
            return metas.Values.ToArray();
        }

        public T1 Find(Predicate<T1> predicate)
        {
            return FindAll(predicate).First();
        }

        public T1[] FindAll(Predicate<T1> predicate)
        {
            return metas.Values.ToList().FindAll(predicate).ToArray();
        }

        public T1[] FindAllWithTags(string[] tags, bool matchAll)
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

        public T1 Get(T2 key)
        {
            return metas.GetValueSafe(key);
        }
    }
}
