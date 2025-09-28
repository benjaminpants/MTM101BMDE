using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{

    [HarmonyPatch(typeof(Cell))]
    [HarmonyPatch("PrepareForPoster")]
    [HarmonyPatch(new Type[0])]
    static class PostFixPatch
    {
        static bool Prefix(Cell __instance)
        {
            if (__instance.Tile.MeshRenderer.sharedMaterial.shader.name.StartsWith("Shader Graphs/TileStandardWPoster")) return false;
            return true;
        }
    }
}
