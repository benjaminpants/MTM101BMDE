using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI
{
    [Serializable]
    public class CustomLevelGenerationParameters : LevelGenerationParameters
    {
        private Dictionary<string, Dictionary<string, object>> customModDatas = new Dictionary<string, Dictionary<string, object>>();

        public WeightedItemObject[] additionalItems = new WeightedItemObject[0];
        public List<ItemObject> additionalForcedItems = new List<ItemObject>();

        public object GetCustomModValue(string modUUID, string key)
        {
            if (!customModDatas.ContainsKey(modUUID)) return null;
            if (!customModDatas[modUUID].ContainsKey(key)) return null;
            return customModDatas[modUUID][key];
        }

        public object GetCustomModValue(PluginInfo pluginInfo, string key)
        {
            return GetCustomModValue(pluginInfo.Metadata.GUID, key);
        }
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

        public void AssignExtraData(CustomLevelObject obj)
        {
            foreach (KeyValuePair<string, Dictionary<string, object>> kvp in obj.customModDatas)
            {
                foreach (KeyValuePair<string, object> internalKvp in kvp.Value)
                {
                    SetCustomModValue(kvp.Key, internalKvp.Key, internalKvp.Value);
                }
            }
            potentialItems = potentialItems.AddRangeToArray(additionalItems);
            forcedItems.AddRange(additionalForcedItems);
        }
    }

    // the required patch

    [HarmonyPatch(typeof(LevelGenerationParameters))]
    [HarmonyPatch("AssignData")]
    internal class LevelGenParamAssignDataPatch
    {
        static void Postfix(LevelGenerationParameters __instance, LevelObject baseObject)
        {
            if (__instance is CustomLevelGenerationParameters)
            {
                if (baseObject is CustomLevelObject)
                {
                    ((CustomLevelGenerationParameters)__instance).AssignExtraData((CustomLevelObject)baseObject);
                }
            }
        }
    }
}
