using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Pickup))]
    [HarmonyPatch("AssignItem")]
    class MinorItemPatches
    {
        static void Postfix(Pickup __instance, ItemObject item)
        {
            __instance.transform.name = "Item_" + item.itemType.ToStringExtended();
        }
    }
}
