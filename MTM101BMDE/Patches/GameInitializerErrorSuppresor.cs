using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace MTM101BaldAPI.Patches
{

    [HarmonyPatch(typeof(GameInitializer))]
    [HarmonyPatch("Initialize")]
    class GameInitializerErrorSuppresor
    {
        static bool Prefix()
        {
            return Singleton<CoreGameManager>.Instance != null;
        }
    }
}
