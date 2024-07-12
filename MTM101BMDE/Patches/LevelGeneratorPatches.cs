using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Assertions;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("Generate", MethodType.Enumerator)]
    class LevelGeneratorPatches
    {
        static Exception Finalizer(Exception __exception)
        {
            // no exception if it is null.
            if (__exception == null) return __exception;
            UnityEngine.Debug.Log("Caught error, printing in console so it doesn't just silently crash!");
            UnityEngine.Debug.LogException(__exception);
            return __exception;
        }
    }
}
