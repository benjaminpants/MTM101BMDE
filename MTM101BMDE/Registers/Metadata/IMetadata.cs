using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// is all this abstraction even remotely necessary? i don't feel like reprogamming the whole thing though so fuck.
namespace MTM101BaldAPI.Registers
{
    public interface IMetadata<T>
    {
        T value { get; }

        List<string> tags { get; }

        PluginInfo info { get; }
    }

    public interface IMetadataStorage<TMeta, TKey, TClass> where TMeta : IMetadata<TClass>
    {
        TMeta Get(TKey key);

        TMeta[] FindAllWithTags(bool matchAll, params string[] tags);

        TMeta Find(Predicate<TMeta> predicate);

        TMeta[] FindAll(Predicate<TMeta> predicate);

        void Add(TMeta toAdd);

        TMeta[] All();
    }

    public class BasicMetaStorage<T1, T2> : IMetadataStorage<T1, T2, T2> where T1 : IMetadata<T2>
    {
        private Dictionary<T2, T1> metas = new Dictionary<T2, T1>();

        public virtual void Add(T1 toAdd)
        {
            Add(toAdd.value, toAdd);
        }

        public virtual void Add(T2 itm, T1 toAdd)
        {
            metas.Add(itm, toAdd);
        }

        public T1[] All()
        {
            return metas.Values.ToArray();
        }

        public T1 Find(Predicate<T1> predicate)
        {
            return FindAll(predicate).FirstOrDefault();
        }

        public T1[] FindAll(Predicate<T1> predicate)
        {
            return metas.Values.ToList().FindAll(predicate).Distinct().ToArray();
        }

        public T1[] FindAllWithTags(bool matchAll, params string[] tags)
        {
            return FindAll(x =>
            {
                foreach (string toSearchFor in tags)
                {
                    if (x.tags.Contains(toSearchFor))
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

    public abstract class MetaStorage<TEnum, TMeta, TType> : IMetadataStorage<TMeta, TEnum, TType> 
        where TMeta : IMetadata<TType>
        where TEnum: Enum
    {

        protected Dictionary<TEnum, TMeta> metas = new Dictionary<TEnum, TMeta>();

        public abstract void Add(TMeta toAdd);

        public abstract TMeta Get(TType value);

        public TMeta[] All()
        {
            return metas.Values.ToArray();
        }

        public TMeta Find(Predicate<TMeta> predicate)
        {
            return metas.Values.ToList().Find(predicate);
        }

        public TMeta[] FindAll(Predicate<TMeta> predicate)
        {
            return metas.Values.ToList().FindAll(predicate).ToArray();
        }

        public TMeta[] FindAllWithTags(bool matchAll, params string[] tags)
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

        public TMeta Get(TEnum key)
        {
            return metas.GetValueSafe(key);
        }
    }
}
