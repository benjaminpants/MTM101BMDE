using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(PitstopGameManager))]
    [HarmonyPatch("Initialize")]
    [HarmonyPriority(Priority.First)]
    class PitstopPosterPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructionsE)
        {
            CodeInstruction[] instructions = instructionsE.ToArray();
            for (int i = 38; i < instructions.Length; i++)
            {
                yield return instructions[i];
            }
            yield break;
        }

        static void Prefix(EnvironmentController ___ec, DirectedIntVector2 ___nextLevelPosterPlacement)
        {
            if (Singleton<CoreGameManager>.Instance.nextLevel == null) return;
            LevelObject nextLevelObject = (Singleton<CoreGameManager>.Instance.nextLevel.randomizedLevelObject.Length != 0) ? GameInitializer.GetControlledRandomLevelData(Singleton<CoreGameManager>.Instance.nextLevel) : Singleton<CoreGameManager>.Instance.nextLevel.levelObject;
            if (nextLevelObject == null) return;
            LevelTypeMeta type = nextLevelObject.type.GetMeta();
            if (type == null) return;
            ___ec.BuildPoster(type.poster, ___ec.CellFromPosition(___nextLevelPosterPlacement.position), ___nextLevelPosterPlacement.direction);
        }
    }
}
