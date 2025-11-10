using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{

    class StickerInitFixRan : MonoBehaviour
    {

    }

    [HarmonyPatch]
    class StickerPatches
    {

        static MethodInfo _DestroyStickers = AccessTools.Method(typeof(StickerScreenController), "DestroyStickers");
        static MethodInfo _InitializeStickers = AccessTools.Method(typeof(StickerScreenController), "InitializeStickers");
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerScreenController))]
        [HarmonyPatch("ApplyHeldSticker")]
        [HarmonyPriority(Priority.Last)]
        static bool ApplyHeldStickerPrefix(StickerScreenController __instance, int slot, ref bool ___holdingSticker, int ___heldStickerId, GameObject ___dropStickerButton)
        {
            if (___holdingSticker && Singleton<StickerManager>.Instance.StickerCanBeCovered(slot))
            {
                ___holdingSticker = false;
                Singleton<StickerManager>.Instance.ApplyExistingSticker(StickerMetaStorage.Instance.Get(Singleton<StickerManager>.Instance.stickerInventory[___heldStickerId].sticker).value.CreateOrGetAppliedStateData(Singleton<StickerManager>.Instance.stickerInventory[___heldStickerId]), slot);
                Singleton<StickerManager>.Instance.RemoveStickerFromInventory(___heldStickerId);
                _DestroyStickers.Invoke(__instance, null);
                _InitializeStickers.Invoke(__instance, null);
                ___dropStickerButton.SetActive(false);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Last)]
        static void StickerUpdatePrefix(StickerManager __instance)
        {
            if (!__instance.TryGetComponent<StickerInitFixRan>(out _))
            {
                for (int i = 0; i < __instance.activeStickerData.Length; i++)
                {
                    if (__instance.activeStickerData[i].GetType() == typeof(StickerStateData))
                    {
                        Debug.Log("Correcting " + i + "!");
                        __instance.activeStickerData[i] = StickerMetaStorage.Instance.Get(__instance.activeStickerData[i].sticker).value.CreateStateData(__instance.activeStickerData[i].activeLevel, __instance.activeStickerData[i].opened, __instance.activeStickerData[i].sticky);
                    }
                }
                __instance.gameObject.AddComponent<StickerInitFixRan>();
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("ApplySticker")]
        [HarmonyPriority(Priority.Last)]
        static bool ApplyStickerPrefix(StickerStateData sticker, int slot, StickerManager __instance, StickerManager.StickerAppliedDelegate ___OnStickerApplied)
        {
            StickerMetaStorage.Instance.Get(sticker.sticker).value.ApplySticker(__instance, sticker, slot);
            ___OnStickerApplied.Invoke();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerCanBeCovered")]
        [HarmonyPriority(Priority.Last)]
        static bool StickerCanBeCoveredPrefix(int slot, StickerManager __instance, ref bool __result)
        {
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[slot].sticker).value.CanBeCovered(__instance.activeStickerData[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerCanBeApplied")]
        [HarmonyPriority(Priority.Last)]
        static bool StickerCanBeAppliedPrefix(Sticker sticker, StickerManager __instance, ref bool __result)
        {
            __result = StickerMetaStorage.Instance.Get(sticker).value.CanBeApplied();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerValue")]
        [HarmonyPriority(Priority.Last)]
        static void StickerValuePostfix(Sticker sticker, ref int __result)
        {
            __result = Mathf.Min(__result, StickerMetaStorage.Instance.Get(sticker).value.stickerValueCap);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetAppliedStickerSprite")]
        [HarmonyPriority(Priority.Last)]
        static bool GetAppliedStickerSpritePrefix(StickerManager __instance, int inventoryId, ref Sprite __result)
        {
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[inventoryId].sticker).value.GetAppliedSprite(__instance.activeStickerData[inventoryId]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetInventoryStickerSprite")]
        [HarmonyPriority(Priority.Last)]
        static bool GetInventoryStickerSpritePrefix(StickerManager __instance, int inventoryId, ref Sprite __result)
        {
            if (!__instance.stickerInventory[inventoryId].opened) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.stickerInventory[inventoryId].sticker).value.sprite;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetLocalizedAppliedStickerDescription")]
        [HarmonyPriority(Priority.Last)]
        static bool GetLocalizedAppliedStickerDescriptionPrefix(StickerManager __instance, int slot, ref string __result)
        {
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[slot].sticker).value.GetLocalizedAppliedStickerDescription(__instance.activeStickerData[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetLocalizedInventoryStickerDescription")]
        [HarmonyPriority(Priority.Last)]
        static bool GetLocalizedInventoryStickerDescriptionPrefix(StickerManager __instance, int slot, ref string __result)
        {
            if (!__instance.stickerInventory[slot].opened) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.stickerInventory[slot].sticker).value.GetLocalizedInventoryStickerDescription(__instance.stickerInventory[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("OpenUnopenedStickerPackets")]
        [HarmonyPriority(Priority.Last)]
        static bool OpenUnopenedStickerPacketsPrefix(StickerManager __instance, bool animation)
        {
            for (int i = 0; i < __instance.stickerInventory.Count; i++)
            {
                if (__instance.stickerInventory[i].opened) continue;
                __instance.stickerInventory[i].opened = true;
                if (animation)
                {
                    Singleton<CoreGameManager>.Instance.GetHud(0).ShowCollectedSticker(__instance.GetInventoryStickerSprite(i));
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetStickerOddsMultiplier")]
        [HarmonyPriority(Priority.Last)]
        static bool GetStickerOddsMultiplierPrefix(StickerManager __instance, Sticker sticker, ref float __result)
        {
            __result = StickerMetaStorage.Instance.Get(sticker).value.CalculateDuplicateOddsMultiplier(__instance);
            return false;
        }

        static FieldInfo _stickerInventory = AccessTools.Field(typeof(StickerManager), "stickerInventory");
        static MethodInfo _AddSticker = AccessTools.Method(typeof(StickerPatches), "AddSticker");
        static MethodInfo _AddRandomSticker = AccessTools.Method(typeof(StickerPatches), "AddRandomSticker");

        static void AddSticker(StickerManager manager, Sticker sticker)
        {
            manager.AddSticker(sticker, true, false, false);
        }

        static void AddRandomSticker(StickerManager manager, Sticker sticker, bool openNow, bool sticky)
        {
            manager.AddSticker(sticker, openNow, sticky, false);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveIdenticalRandomStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveIdenticalStickersTranspiler(IEnumerable<CodeInstruction> instructionsE)
        {
            CodeInstruction[] instructions = instructionsE.ToArray();
            bool patched = false;
            for (int i = 0; i < instructions.Length; i++)
            {
                if (((i + 9) >= instructions.Length) || patched)
                {
                    yield return instructions[i];
                    continue;
                }
                if ((instructions[i].opcode == OpCodes.Ldarg_0)
                    &&
                    ((instructions[i + 1].opcode == OpCodes.Ldfld) && ((FieldInfo)instructions[i + 1].operand == _stickerInventory))
                    &&
                    (instructions[i + 2].opcode == OpCodes.Ldloc_0)
                    &&
                    (instructions[i + 3].opcode == OpCodes.Ldc_I4_0)
                    &&
                    (instructions[i + 4].opcode == OpCodes.Ldarg_3)
                    &&
                    (instructions[i + 5].opcode == OpCodes.Ldc_I4_0)
                    &&
                    (instructions[i + 6].opcode == OpCodes.Newobj)
                    &&
                    (instructions[i + 7].opcode == OpCodes.Callvirt))
                {
                    patched = true;
                    yield return instructions[i];
                    yield return instructions[i + 2];
                    yield return new CodeInstruction(OpCodes.Call, _AddSticker);
                    i += 7;
                    continue;
                }
                yield return instructions[i];
            }
            if (!patched) throw new NotImplementedException("Unable to patch StickerManager.GiveIdenticalRandomStickers!");
            yield break;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveNormalRandomStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveNormalRandomStickersTranspiler(IEnumerable<CodeInstruction> instructionsE)
        {
            return GenericGiveRandomStickersTranspiler(instructionsE, "GiveNormalRandomStickers");
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveRandomBonusStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveRandomBonusStickersTranspiler(IEnumerable<CodeInstruction> instructionsE)
        {
            return GenericGiveRandomStickersTranspiler(instructionsE, "GiveRandomBonusStickers");
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveNewRandomStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveNewRandomStickersTranspiler(IEnumerable<CodeInstruction> instructionsE)
        {
            return GenericGiveRandomStickersTranspiler(instructionsE, "GiveNewRandomStickers");
        }

        static IEnumerable<CodeInstruction> GenericGiveRandomStickersTranspiler(IEnumerable<CodeInstruction> instructionsE, string message)
        {
            CodeInstruction[] instructions = instructionsE.ToArray();
            bool patched = false;
            for (int i = 0; i < instructions.Length; i++)
            {
                if ((i + 9) >= instructions.Length)
                {
                    yield return instructions[i];
                    continue;
                }
                if ((instructions[i].opcode == OpCodes.Ldarg_0)
                    &&
                    ((instructions[i + 1].opcode == OpCodes.Ldfld) && ((FieldInfo)instructions[i + 1].operand == _stickerInventory))
                    &&
                    (instructions[i + 2].opcode == OpCodes.Ldarg_0)
                    &&
                    (instructions[i + 3].opcode == OpCodes.Ldfld)
                    &&
                    (instructions[i + 4].opcode == OpCodes.Call)
                    &&
                    (instructions[i + 5].opcode == OpCodes.Ldc_I4_0)
                    &&
                    (instructions[i + 6].opcode == OpCodes.Ldarg_3)
                    &&
                    ((instructions[i + 7].opcode == OpCodes.Ldarg_S) || (instructions[i + 7].opcode == OpCodes.Ldc_I4_0))
                    &&
                    (instructions[i + 8].opcode == OpCodes.Newobj)
                    &&
                    (instructions[i + 9].opcode == OpCodes.Callvirt))
                {
                    Debug.Log("Patching at: " + i);
                    patched = true;
                    // two this' as the first one gets popped for _potentialStickersToAdd
                    yield return instructions[i]; // this
                    yield return instructions[i + 2]; // this
                    yield return instructions[i + 3]; // _potentialStickersToAdd
                    yield return instructions[i + 4]; // call RandomSelection
                    yield return instructions[i + 6]; // openNow
                    yield return instructions[i + 7]; // depending on what we are patching either 0 or sticky parameter
                    yield return new CodeInstruction(OpCodes.Call, _AddRandomSticker);
                    i += 9;
                    continue;
                }
                yield return instructions[i];
            }
            if (!patched) throw new NotImplementedException("Unable to patch " + message + "!");
            yield break;
        }
    }
}
