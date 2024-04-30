using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;

namespace MTM101BaldAPI.Patches
{

    [HarmonyPatch(typeof(GameInitializer))]
    [HarmonyPatch("Initialize")]
    class BaseGameMangerSetActive
    {
        static void SetBaseActive(BaseGameManager bg)
        {
            bg.gameObject.SetActive(true);
        }
        static MethodInfo fo = AccessTools.Method(typeof(BaseGameMangerSetActive), "SetBaseActive");
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool didPatch = false;
            CodeInstruction[] codeInstructions = instructions.ToArray();
            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                yield return instruction;
                if (didPatch) continue;
                if (i + 7 > codeInstructions.Length - 1) continue;
                if (
                    (codeInstructions[i + 0].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 1].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 2].opcode == OpCodes.Ldloc_1) &&
                    (codeInstructions[i + 3].opcode == OpCodes.Ldloc_2) &&
                    (codeInstructions[i + 4].opcode == OpCodes.Call) &&
                    (codeInstructions[i + 5].opcode == OpCodes.Stfld) &&
                    (codeInstructions[i + 6].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 7].opcode == OpCodes.Ldarg_0)
                    )
                {
                    didPatch = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_2); //baseGameManager (local variable)
                    yield return new CodeInstruction(OpCodes.Call, fo); //BaseGameMangerSetActive.SetActive
                }
            }
            if (!didPatch)
            {
                throw new Exception("Unable to patch GameInitializer.Initialize!");
            }
            yield break;
        }
    }
}
