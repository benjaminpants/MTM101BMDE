using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace MTM101BaldAPI.Patches
{
    [ConditionalPatchConfig("mtm101.rulerp.bbplus.baldidevapi", "Generator", "Fix Vanilla Crashes")]
    [HarmonyPatch(typeof(LevelBuilder))]
    [HarmonyPatch("PlaceItemInRandomRoom")]
    static class PlaceItemInRandomRoomPatch
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is ArgumentOutOfRangeException) return null;
            return __exception;
        }
    }
}
