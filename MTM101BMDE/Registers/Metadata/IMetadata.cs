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

        HashSet<string> tags { get; }

        PluginInfo info { get; }
    }

    public class BasicMetaStorage<MetaType, T> where MetaType : IMetadata<T>
    {
        private Dictionary<T, MetaType> metas = new Dictionary<T, MetaType>();

        public virtual void Add(MetaType toAdd)
        {
            Add(toAdd.value, toAdd);
        }

        public virtual void Add(T itm, MetaType toAdd)
        {
            metas.Add(itm, toAdd);
        }

        public MetaType[] All()
        {
            return metas.Values.ToArray();
        }

        public MetaType Find(Predicate<MetaType> predicate)
        {
            return FindAll(predicate).FirstOrDefault();
        }

        public MetaType[] FindAll(Predicate<MetaType> predicate)
        {
            return metas.Values.ToList().FindAll(predicate).Distinct().ToArray();
        }

        public MetaType[] FindAllWithTags(bool matchAll, params string[] tags)
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

        public MetaType Get(T key)
        {
            return metas.GetValueSafe(key);
        }

        public virtual bool Remove(T toRemove)
        {
            return metas.Remove(toRemove);
        }
    }

    public abstract class MetaStorage<TEnum, TMeta, TType> 
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

        public virtual bool Remove(TEnum toRemove)
        {
            return metas.Remove(toRemove);
        }
    }
}
