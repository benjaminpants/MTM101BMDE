using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{

    public static class CustomLevelObjectExtensions
    {
        public static CustomLevelObject CustomLevelObject(this SceneObject sceneObj)
        {
            if (sceneObj.levelObject == null) return null;
            return (CustomLevelObject)sceneObj.levelObject;
        }
    }


    /// <summary>
    /// A custom version of the LevelObject class, currently doesn't contain much else but it serves as a good base to make extending level generator functionality easy in the future.
    /// </summary>
    public class CustomLevelObject : LevelObject
    {

        private Dictionary<string, Dictionary<string, object>> customModDatas = new Dictionary<string, Dictionary<string, object>>();

        public object GetCustomModValue(string modUUID, string key)
        {
            if (!customModDatas.ContainsKey(modUUID)) return null;
            if (!customModDatas[modUUID].ContainsKey(key)) return false;
            return customModDatas[modUUID][key];
        }

        public object GetCustomModValue(PluginInfo pluginInfo, string key)
        {
            return GetCustomModValue(pluginInfo.Metadata.GUID, key);
        }

        /// <summary>
        /// Makes a clone of this CustomLevelObject, preserving the custom mod data.
        /// </summary>
        /// <returns></returns>
        public CustomLevelObject MakeClone()
        {
            CustomLevelObject obj = CustomLevelObject.Instantiate(this);
            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in customModDatas)
            {
                foreach (KeyValuePair<string, object> internalKvp in kvp.Value)
                {
                    obj.SetCustomModValue(kvp.Key, internalKvp.Key, internalKvp.Value);
                }
            }
            return obj;
        }

        /// <summary>
        /// Adds the specified key/value to the CustomLevelObject, allowing for storing extra, mod specific generator settings.
        /// </summary>
        /// <param name="pluginInfo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCustomModValue(PluginInfo pluginInfo, string key, object value)
        {
            string modUUID = pluginInfo.Metadata.GUID;
            if (!customModDatas.ContainsKey(modUUID))
            {
                customModDatas.Add(modUUID, new Dictionary<string, object>());
            }
            if (customModDatas[modUUID].ContainsKey(key))
            {
                customModDatas[modUUID][key] = value;
                return;
            }
            customModDatas[modUUID].Add(key, value);
        }

        public void SetCustomModValue(string modUUID, string key, object value)
        {
            if (!customModDatas.ContainsKey(modUUID))
            {
                customModDatas.Add(modUUID, new Dictionary<string, object>());
            }
            if (customModDatas[modUUID].ContainsKey(key))
            {
                customModDatas[modUUID][key] = value;
                return;
            }
            customModDatas[modUUID].Add(key, value);
        }

        [Obsolete("BB+ no longer uses .items, use .forcedItems or .potentialItems instead!", true)]
        public new WeightedItemObject[] items; //hacky way of adding the Obsolete tag, but it works?

        [Obsolete("BB+ no longer uses .totalShopItems, change totalShopItems in the SceneObject instead!", true)]
        public new int totalShopItems;

        [Obsolete("BB+ no longer uses .shopItems, change shopItems in the SceneObject instead!", true)]
        public new WeightedItemObject[] shopItems;

        [Obsolete("BB+ no longer uses .mapPrice, change mapPrice in the SceneObject instead!", true)]
        public new int mapPrice;

        [Obsolete("BB+ no longer uses .potentialNPCs, change potentialNPCs in the SceneObject instead!", true)]
        public new List<WeightedNPC> potentialNPCs;
    }
}
