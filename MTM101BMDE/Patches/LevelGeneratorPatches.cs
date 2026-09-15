using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace MTM101BaldAPI.Patches
{
    class LevelGeneratorPatches
    {
        [HarmonyPatch(typeof(LevelGenerator))]
        [HarmonyPatch("Generate", MethodType.Enumerator)]
        static Exception Finalizer(Exception __exception)
        {
            // no exception if it is null.
            if (__exception == null) return __exception;
            UnityEngine.Debug.Log("Caught error, printing in console so it doesn't just silently crash!");
            UnityEngine.Debug.LogException(__exception);
            return __exception;
        }

        // this code is absolutely ugly but whatever
        [HarmonyPatch(typeof(LevelGenerator), nameof(LevelGenerator.Generate), MethodType.Enumerator)]
        class MoreExitsPatch
        {

            static FieldInfo _LevelGenerator_potentailExitDirections = AccessTools.Field(AccessTools.Method(typeof(LevelGenerator), "Generate").GetCustomAttribute<StateMachineAttribute>().StateMachineType, "<potentailExitDirections>5__36");

            static MethodInfo _AddMoreExitsToList = AccessTools.Method(typeof(MoreExitsPatch), "AddMoreExitsToList");

            static void AddMoreExitsToList(LevelGenerator gen, ref List<Direction> directions)
            {
                if (gen.ld.exitCount <= 4) return;
                if (directions.Count == 0)
                {
                    Directions.FillWithAll(directions);
                }
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions)
                    .Start()
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, _LevelGenerator_potentailExitDirections),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(List<Direction>), nameof(List<Direction>.RemoveAt))))
                    .ThrowIfInvalid("FAIL").Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _LevelGenerator_potentailExitDirections),
                        Transpilers.EmitDelegate<Action<LevelGenerator, List<Direction>>>((gen, direction) =>
                        {
                            AddMoreExitsToList(gen, ref direction);
                        })
                        )
                    .InstructionEnumeration();
                return matcher;
            }
        }
    }
}
