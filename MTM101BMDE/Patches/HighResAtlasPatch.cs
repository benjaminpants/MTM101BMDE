using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{

    [HarmonyPatch(typeof(RoomController))]
    [HarmonyPatch("GenerateTextureAtlas")]
    [HarmonyPatch(new Type[0])]
    static class HighResAtlasPatch
    {
        static bool Prefix(RoomController __instance)
        {
            int maxSize = Mathf.Max(Mathf.Max(__instance.florTex.width, __instance.florTex.height), Mathf.Max(__instance.wallTex.width, __instance.wallTex.height), Mathf.Max(__instance.ceilTex.width, __instance.ceilTex.height));
            if (maxSize > 256)
            {
                __instance.GenerateTextureAtlas(maxSize);
                return false;
            }
            return true;
        }
    }
}
