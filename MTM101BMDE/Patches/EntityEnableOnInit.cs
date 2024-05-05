using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [ConditionalPatchNever]
    [HarmonyPatch(typeof(Entity))]
    [HarmonyPatch("Initialize")]
    internal class EntityPatch
    {
        private static void Prefix(Entity __instance, ref Transform ___transform, bool ___active)
        {
            ___transform = __instance.transform;
            if (___active) //so if someone creates an object with a disabled entity we dont fuck shit up
            {
                __instance.SetActive(true);
            }
        }
    }
}
