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

        static void Postfix(CullingManager __instance, ref EnvironmentController ___ec, bool ___active, bool ___manualMode, int ___currentChunkId)
        {
            if (CullAffector.allAffectors.Count == 0) return; //dont bother if we have no CullAffectors.
            List<int> culledChunks = new List<int>() { ___currentChunkId };
            for (int i = 0; i < CullAffector.allAffectors.Count; i++)
            {
                CullAffector affector = CullAffector.allAffectors[i];
                Cell currentCell = ___ec.CellFromPosition(affector.transform.position);
                if (!currentCell.HasChunk) continue;
                __instance.CalculateOcclusionCullingForChunk(currentCell.Chunk.Id);
                culledChunks.Add(currentCell.Chunk.Id);
            }
            if (___active && !___manualMode)
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
