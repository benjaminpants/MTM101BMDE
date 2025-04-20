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

        public static readonly Type[] defaultIgnoreTypes = new Type[]
        {
            typeof(object),
            typeof(UnityEngine.Object),
            typeof(MonoBehaviour),
            typeof(Behaviour),
            typeof(ScriptableObject)
        };

        protected Dictionary<Type, Dictionary<string, object>> data = new Dictionary<Type, Dictionary<string, object>>();

        /// <summary>
        /// Get the amount of unique elements in the AssetManager.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Create an AssetManager with a custom set of types to be ignored. It is suggested you also add the types in AssetManager.defaultIgnoreTypes
        /// </summary>
        /// <param name="ignoreTypes"></param>
        public AssetManager(Type[] ignoreTypes)
        {
            this.ignoreTypes = ignoreTypes;
        }

        /// <summary>
        /// Create an AssetManager with the default types ignored.
        /// </summary>
        public AssetManager()
        {
            ignoreTypes = defaultIgnoreTypes;
        }

        public void Add<T>(string key, T value)
        {
            AddInternal(key, value, value.GetType());
        }

        /// <summary>
        /// Remove all objects of type T from the AssetManager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// Add a range of elements to the AssetManager, using the keyFunc to determine the keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="keyFunc"></param>
        public void AddRange<T>(T[] range, Func<T, string> keyFunc)
        {
            for (int i = 0; i < range.Length; i++)
            {
                Add(keyFunc.Invoke(range[i]), range[i]);
            }
        }

        /// <summary>
        /// Add all resources of the specified type to the AssetManager, using the name as the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddFromResources<T>() where T : UnityEngine.Object
        {
            AddRange<T>(Resources.FindObjectsOfTypeAll<T>());
        }

        /// <summary>
        /// Add all resources of the specified type to the AssetManager that are not created by mods/weren't created during runtime, using the name as the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddFromResourcesNoClones<T>() where T : UnityEngine.Object
        {
            AddRange<T>(Resources.FindObjectsOfTypeAll<T>().Where(x => x.GetInstanceID() >= 0).ToArray());
        }

        /// <summary>
        /// Check if the asset manager contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            foreach (KeyValuePair<Type, Dictionary<string,object>> kvp in data)
            {
                if (kvp.Value.ContainsKey(key))
                {
                    return true;
                }    
            }
            return false;
        }

        /// <summary>
        /// Add a range of values to the AssetManager, using the .name of the object as the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        public void AddRange<T>(T[] range) where T : UnityEngine.Object
        {
            AddRange(range, (obj) =>
            {
                return obj.name;
            });
        }

        /// <summary>
        /// Add a range of values to the AssetManager, using the .name of the object as the key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        public void AddRange<T>(List<T> range) where T : UnityEngine.Object
        {
            AddRange(range.ToArray());
        }

        /// <summary>
        /// Add a range of values to the AssetManager using a dictionary of object key pairs, with an optional prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="prefix"></param>
        public void AddRange<T>(Dictionary<T, string> range, string prefix = "")
        {
            AddRange<T>(range.Keys.ToArray(), (obj) =>
            {
                return prefix + range[obj];
            });
        }

        /// <summary>
        /// Add a range of values to the AssetManager using an object array and a key array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="keys"></param>
        public void AddRange<T>(T[] range, string[] keys)
        {
            for (int i = 0; i < range.Length; i++)
            {
                Add(keys[i], range[i]);
            }
        }

        /// <summary>
        /// Add a range of values to the AssetManager using a dictionary of object key pairs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
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
