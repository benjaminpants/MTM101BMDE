using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.AssetTools
{
    /// <summary>
    /// This class provides an (optional) easy and convient way to store mass amounts of asset references.
    /// This is useful for storing textures, SoundObjects, NPC references, and more.
    /// This class is not meant to be the end all be all of storing and managing assets.
    /// However, this is a good starting point for most mods.
    /// </summary>
    public class AssetManager
    {

        protected Dictionary<Type, Dictionary<string, object>> data = new Dictionary<Type, Dictionary<string, object>>();

        public int Count
        {
            get
            {
                int count = 0;
                foreach (Dictionary<string, object> item in data.Values)
                {
                    count += item.Count;
                }
                return count;
            }
        }

        public void Add<T>(string key, T value)
        {
            AddInternal(key, value, typeof(T));
        }

        public void AddRange<T>(T[] range, Func<T, string> keyFunc)
        {
            for (int i = 0; i < range.Length; i++)
            {
                Add(keyFunc.Invoke(range[i]), range[i]);
            }
        }

        public void AddRange<T>(T[] range) where T : UnityEngine.Object
        {
            AddRange(range, (obj) =>
            {
                return obj.name;
            });
        }

        public T[] GetAll<T>()
        {
            if (!data.ContainsKey(typeof(T))) return new T[0];
            List<T> values = new List<T>();
            data[typeof(T)].Values.Do(x =>
            {
                values.Add((T)x);
            });
            return values.ToArray();
        }

        protected void AddInternal(string key, object value, Type type)
        {
            if (type == typeof(object)) return;
            if (type == typeof(UnityEngine.Object)) return;
            if (!data.ContainsKey(type))
            {
                data[type] = new Dictionary<string, object>();
            }
            data[type][key] = value;

            AddInternal(key, value, type.BaseType);
        }

        public bool Remove<T>(string key)
        {
            return RemoveInternal(typeof(T), key);
        }

        internal bool RemoveInternal(Type type, string key, bool found = false)
        {
            if (type == typeof(object)) return found;
            if (type == typeof(UnityEngine.Object)) return found;
            if (!data.ContainsKey(type)) return false;
            found = found || data[type].Remove(key);
            return RemoveInternal(type.BaseType, key, found);
        }

        public object this[Type type, string key]
        {
            get
            {
                return GetInternal(type, key);
            }
            set
            {
                AddInternal(key, value, type);
            }
        }

        public object this[string key]
        {
            // It is suggested to avoid using the getter here, because its slow.
            get
            {
                foreach (Type item in data.Keys)
                {
                    object returnV = GetInternal(item, key);
                    if (returnV != null)
                    {
                        return returnV;
                    }
                }
                return null;
            }
            set
            {
                AddInternal(key, value, value.GetType());
            }
        }

        public T Get<T>(string key)
        {
            object value = GetInternal(typeof(T), key);
            return value == null ? default : (T)value;
        }

        protected object GetInternal(Type type, string key)
        {
            if (!data.ContainsKey(type))
            {
                throw new KeyNotFoundException();
            }
            if (data[type].TryGetValue(key, out object value))
            {
                return value;
            }
            return null;
        }
    }
}
