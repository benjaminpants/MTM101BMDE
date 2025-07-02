using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine;

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

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool didPatch = false;
            CodeInstruction[] codeInstructions = instructions.ToArray();
            for (int i = 0; i < codeInstructions.Length; i++)
            {
                if (didPatch)
                {
                    yield return codeInstructions[i];
                    continue;
                }
                CodeInstruction instruction = codeInstructions[i];
                if (instruction.opcode == OpCodes.Newobj)
                {
                    instruction.operand = AccessTools.Constructor(typeof(CustomLevelGenerationParameters));
                    didPatch = true;
                }
                yield return codeInstructions[i];
            }
            yield break;
        }
    }
}
