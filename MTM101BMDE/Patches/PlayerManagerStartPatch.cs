using HarmonyLib;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(PlayerManager))]
    [HarmonyPatch("Start")]
    internal class PlayerManagerStartPatch
    {
        private static void Postfix(PlayerManager __instance)
        {
            __instance.GetMovementStatModifier();
            __instance.gameObject.AddComponent<PlayerDestroyTracker>().itm = __instance.itm;
        }
    }
}
