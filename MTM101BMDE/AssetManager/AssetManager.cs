using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

        protected Type[] ignoreTypes;

        protected static Type[] defaultIgnoreTypes = new Type[]
        {
            typeof(object),
            typeof(UnityEngine.Object),
            typeof(MonoBehaviour),
            typeof(Behaviour),
            typeof(ScriptableObject)
        };

        protected Dictionary<Type, Dictionary<string, object>> data = new Dictionary<Type, Dictionary<string, object>>();

        public int GetUniqueCount()
        {
            List<object> found = new List<object>();
            int count = 0;
            foreach (Dictionary<string, object> dict in data.Values)
            {
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    if (!found.Contains(kvp.Value))
                    {
                        found.Add(kvp.Value);
                        count++;
                    }
                }
            }
            return count;
        }

        public AssetManager(Type[] ignoreTypes)
        {
            this.ignoreTypes = ignoreTypes;
        }

        public AssetManager()
        {
            ignoreTypes = defaultIgnoreTypes;
        }

        public void Add<T>(string key, T value)
        {
            AddInternal(key, value, value.GetType());
        }

        public void ClearAll<T>()
        {
            if (!data.ContainsKey(typeof(T))) return;
            List<string> keysToRemove = new List<string>();
            foreach (string key in data[typeof(T)].Keys)
            {
                keysToRemove.Add(key);
            }
            keysToRemove.Do(x =>
            {
                Remove<T>(x);
            });
        }

        public void AddRange<T>(T[] range, Func<T, string> keyFunc)
        {
            for (int i = 0; i < range.Length; i++)
            {
                Add(keyFunc.Invoke(range[i]), range[i]);
            }
        }

        public void AddFromResources<T>() where T : UnityEngine.Object
        {
            AddRange<T>(Resources.FindObjectsOfTypeAll<T>());
        }

        public bool ContainsKey(string t)
        {
            foreach (KeyValuePair<Type, Dictionary<string,object>> kvp in data)
            {
                if (kvp.Value.ContainsKey(t))
                {
                    return true;
                }    
            }
            return false;
        }

        public void AddRange<T>(T[] range) where T : UnityEngine.Object
        {
            AddRange(range, (obj) =>
            {
                return obj.name;
            });
        }

        public void AddRange<T>(List<T> range) where T : UnityEngine.Object
        {
            AddRange(range.ToArray());
        }

        public void AddRange<T>(Dictionary<T, string> range, string prefix = "")
        {
            AddRange<T>(range.Keys.ToArray(), (obj) =>
            {
                return prefix + range[obj];
            });
        }

        public void AddRange<T>(T[] range, string[] keys)
        {
            for (int i = 0; i < range.Length; i++)
            {
                Add(keys[i], range[i]);
            }
        }

        public void AddRange<T>(Dictionary<string, T> range)
        {
            foreach (KeyValuePair<string, T> kvp in range)
            {
                Add(kvp.Key, kvp.Value);
            }
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
            if (ignoreTypes.Contains(type)) return;
            if (!data.ContainsKey(type))
            {
                data[type] = new Dictionary<string, object>();
            }
            data[type][key] = value;

            AddInternal(key, value, type.BaseType);
        }

        // todo: consider removing this?
        public bool RemoveCheap<T>(string key)
        {
            return RemoveInternal(typeof(T), key);
        }

        public bool Remove<T>(string key)
        {
            Type actType = Get<T>(key).GetType();
            if (actType != typeof(T))
            {
                return RemoveInternal(actType, key);
            }
            return RemoveInternal(typeof(T), key);
        }

        protected bool RemoveInternal(Type type, string key, bool found = false)
        {
            if (ignoreTypes.Contains(type)) return found;
            if (!data.ContainsKey(type)) return false;
            found = data[type].Remove(key) || found;
            if (data[type].Count == 0)
            {
                data.Remove(type);
            }
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
                AddInternal(key, value, value.GetType());
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
