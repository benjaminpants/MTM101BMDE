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
        [HarmonyPatch(typeof(BaseGameManager))]
        [HarmonyPatch("GiveRandomSticker")]
        [HarmonyPriority(Priority.Last)]
        static bool GiveRandomStickerPrefix()
        {
            Sticker chosenSticker = WeightedSelection<Sticker>.RandomSelection(Singleton<CoreGameManager>.Instance.sceneObject.potentialStickers);
            Singleton<StickerManager>.Instance.AddSticker(chosenSticker, false, false, false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PitstopGameManager))]
        [HarmonyPatch("GiveRandomSticker")]
        [HarmonyPriority(Priority.Last)]
        static bool PitstopGiveRandomStickerPrefix()
        {
            Sticker chosenSticker = WeightedSelection<Sticker>.RandomSelection(Singleton<CoreGameManager>.Instance.nextLevel.potentialStickers);
            Singleton<StickerManager>.Instance.AddSticker(chosenSticker, true, false, true);
            return false;
        }

        static FieldInfo _stickerInventory = AccessTools.Field(typeof(StickerManager), "stickerInventory");
        static MethodInfo _AddSticker = AccessTools.Method(typeof(StickerPatches), "AddSticker");

        static void AddSticker(StickerManager manager, Sticker sticker)
        {
            manager.AddSticker(sticker, true, false, false);
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
                Debug.Log(instructions[i].opcode);
                Debug.Log(instructions[i].operand);
                if ((i + 7) >= instructions.Length)
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
                    instructions[i + 0] = new CodeInstruction(OpCodes.Ldarg_0); // this
                    instructions[i + 1] = new CodeInstruction(OpCodes.Ldloc_0); // sticker
                    instructions[i + 2] = new CodeInstruction(OpCodes.Call, _AddSticker);
                    instructions[i + 3] = new CodeInstruction(OpCodes.Nop);
                    instructions[i + 4] = new CodeInstruction(OpCodes.Nop);
                    instructions[i + 5] = new CodeInstruction(OpCodes.Nop);
                    instructions[i + 6] = new CodeInstruction(OpCodes.Nop);
                    instructions[i + 7] = new CodeInstruction(OpCodes.Nop);
                }
                yield return instructions[i];
            }
            if (!patched) throw new NotImplementedException("Unable to patch StickerManager.GiveIdenticalRandomStickers!");
            yield break;
        }
    }
}
