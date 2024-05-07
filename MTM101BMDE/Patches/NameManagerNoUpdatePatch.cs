using HarmonyLib;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Update")]
    internal class NameManagerNoUpdatePatch
    {
        private static bool Prefix()
        {
            return ModLoadingScreenManager.doneLoading;
        }
    }
}
