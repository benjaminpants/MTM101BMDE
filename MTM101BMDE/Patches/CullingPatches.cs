using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.Components;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(CullingManager))]
    [HarmonyPatch("LateUpdate")]
    class CullingPatch
    {


        static FieldInfo _ec = AccessTools.Field(typeof(CullingManager), "ec");
        static FieldInfo _active = AccessTools.Field(typeof(CullingManager), "active");
        static FieldInfo _manualMode = AccessTools.Field(typeof(CullingManager), "manualMode");
        static FieldInfo _currentChunkId = AccessTools.Field(typeof(CullingManager), "currentChunkId");
        static void Postfix(CullingManager __instance)
        {
            if (CullAffector.allAffectors.Count == 0) return; //dont bother if we have no CullAffectors.
            List<int> culledChunks = new List<int>() { (int)_currentChunkId.GetValue(__instance) };
            for (int i = 0; i < CullAffector.allAffectors.Count; i++)
            {
                CullAffector affector = CullAffector.allAffectors[i];
                Cell currentCell = ((EnvironmentController)_ec.GetValue(__instance)).CellFromPosition(affector.transform.position);
                if (!currentCell.HasChunk) continue;
                __instance.CalculateOcclusionCullingForChunk(currentCell.Chunk.Id);
                culledChunks.Add(currentCell.Chunk.Id);
            }
            if ((bool)_active.GetValue(__instance) && !(bool)_manualMode.GetValue(__instance))
            {
                for (int i = 0; i < __instance.allChunks.Count; i++)
                {
                    bool chunkVisible = false;
                    for (int j = 0; j < culledChunks.Count; j++)
                    {
                        if (__instance.allChunks[culledChunks[j]].visibleChunks[i])
                        {
                            chunkVisible = true;
                            break;
                        }
                    }
                    __instance.allChunks[i].Render(chunkVisible);
                }
            }
        }
    }
}
