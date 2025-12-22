using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using UnityEngine;
using UnityEngine.UI;

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
        static bool ApplyHeldStickerPrefix(StickerScreenController __instance, int slot, ref bool ___holdingSticker, int ___heldStickerInventoryId, GameObject ___dropStickerButton, SoundObject ___audApply, Sprite ___cursorOpenSprite)
        {
            if (!___holdingSticker) return false;
            StickerStateData heldData = Singleton<StickerManager>.Instance.stickerInventory[___heldStickerInventoryId];
            if (!StickerMetaStorage.Instance.Get(heldData.sticker).value.CouldCoverSticker(Singleton<StickerManager>.Instance, heldData, Singleton<StickerManager>.Instance.activeStickerData[slot], ___heldStickerInventoryId, slot)) return false;
            if (Singleton<StickerManager>.Instance.StickerCanBeCovered(slot))
            {
                ___holdingSticker = false;
                Singleton<StickerManager>.Instance.ApplySticker(Singleton<StickerManager>.Instance.stickerInventory[___heldStickerInventoryId], slot);
                Singleton<StickerManager>.Instance.RemoveStickerFromInventory(___heldStickerInventoryId);
                _DestroyStickers.Invoke(__instance, null);
                _InitializeStickers.Invoke(__instance, null);
                ___dropStickerButton.SetActive(false);
                Singleton<MusicManager>.Instance.PlaySoundEffect(___audApply);
                CursorController.Instance.SetSprite(___cursorOpenSprite);
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
                        MTM101BaldiDevAPI.Log.LogDebug("Correcting sticker state data of: " + i + "!");
                        __instance.activeStickerData[i] = StickerMetaStorage.Instance.Get(__instance.activeStickerData[i].sticker).value.CreateStateData(__instance.activeStickerData[i].activeLevel, __instance.activeStickerData[i].opened, __instance.activeStickerData[i].sticky);
                    }
                }
                __instance.gameObject.AddComponent<StickerInitFixRan>();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("AwakeFunction")]
        [HarmonyPriority(Priority.First)]
        static void StickerAwakeFunctionPrefix(ref List<Sticker> ___bonusStickers)
        {
            ___bonusStickers = StickerMetaStorage.Instance.FindAll(x => x.flags.HasFlag(StickerFlags.IsBonus)).Select(x => x.type).Distinct().ToList();
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
            if (StickerMetaStorage.Instance.Get(sticker) == null)
            {
                MTM101BaldiDevAPI.Log.LogWarning(sticker.ToStringExtended() + " has no meta! Unused?");
                return;
            }
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
            __result = StickerMetaStorage.Instance.Get(__instance.stickerInventory[inventoryId].sticker).value.GetInventorySprite(__instance.stickerInventory[inventoryId]);
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

        // set sprites
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventorySticker))]
        [HarmonyPatch("SetInventoryId")]
        [HarmonyPriority(Priority.First)]
        static void SetInventoryIdPostfix(InventorySticker __instance, Image ___image, int ___inventoryId)
        {
            ___image.sprite = Singleton<StickerManager>.Instance.GetInventoryStickerSprite(___inventoryId);
        }

        // set sprites
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StickerScreenController))]
        [HarmonyPatch("UpdateStickerInventoryPositions")]
        [HarmonyPriority(Priority.First)]
        static void UpdateStickerInventoryPositionsPostfix(StickerScreenController __instance, List<InventorySticker> ___inventoryStickers)
        {
            for (int i = 0; i < ___inventoryStickers.Count; i++)
            {
                ___inventoryStickers[i].SetValue(Singleton<StickerManager>.Instance.GetStickerStackSize(Singleton<StickerManager>.Instance.stickerInventory[(int)_inventoryId.GetValue(___inventoryStickers[i])]));
            }
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

        static MethodInfo _CompareStickers = AccessTools.Method(typeof(StickerPatches), "CompareStickers");

        static FieldInfo _inventoryId = AccessTools.Field(typeof(InventorySticker), "inventoryId");

        static bool CompareStickers(InventorySticker sticker, StickerStateData otherSticker)
        {
            int inventoryId = (int)_inventoryId.GetValue(sticker);
            StickerStateData invSticker = Singleton<StickerManager>.Instance.stickerInventory[inventoryId];
            return invSticker.CanStackWith(otherSticker);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerScreenController))]
        [HarmonyPatch("InitializeStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> InitializeStickersTranspiler(IEnumerable<CodeInstruction> instructionsE)
        {
            CodeInstruction[] instructions = instructionsE.ToArray();
            bool patched = false;
            for (int i = 0; i < instructions.Length; i++)
            {
                if (((i + 8) >= instructions.Length) || patched)
                {
                    yield return instructions[i];
                    continue;
                }
                if ((instructions[i].opcode == OpCodes.Ldloc_S)
                    &&
                    ((instructions[i + 1].opcode == OpCodes.Callvirt))
                    &&
                    (instructions[i + 2].opcode == OpCodes.Call)
                    &&
                    (instructions[i + 3].opcode == OpCodes.Ldfld)
                    &&
                    (instructions[i + 4].opcode == OpCodes.Ldloc_1)
                    &&
                    (instructions[i + 5].opcode == OpCodes.Callvirt)
                    &&
                    (instructions[i + 6].opcode == OpCodes.Ldfld)
                    &&
                    (instructions[i + 7].opcode == OpCodes.Bne_Un))
                {
                    patched = true;
                    yield return instructions[i]; // pass in inventory sticker
                    //yield return new CodeInstruction(OpCodes.Nop);
                    yield return instructions[i + 2]; // StickerManager GetInstance
                    yield return instructions[i + 3]; // stickerInventory
                    yield return instructions[i + 4]; // load i
                    yield return instructions[i + 5]; // get
                    yield return new CodeInstruction(OpCodes.Call, _CompareStickers);
                    instructions[i + 7].opcode = OpCodes.Brfalse;
                    yield return instructions[i + 7];
                    i += 7;
                    continue;
                }
                yield return instructions[i];
            }
            if (!patched) throw new NotImplementedException("Unable to patch StickerScreenController.InitializeStickers!");
            yield break;
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
        static IEnumerable<CodeInstruction> GiveNormalRandomStickersTranspiler(IEnumerable<CodeInstruction> instructionsE) => GenericGiveRandomStickersTranspiler(instructionsE, "GiveNormalRandomStickers");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveRandomBonusStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveRandomBonusStickersTranspiler(IEnumerable<CodeInstruction> instructionsE) => GenericGiveRandomStickersTranspiler(instructionsE, "GiveRandomBonusStickers");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveNewRandomStickers")]
        [HarmonyPriority(Priority.Last)]
        static bool GiveNewRandomStickers(StickerManager __instance, WeightedSticker[] potentialStickers, int amount, bool openNow, List<WeightedSticker> ____potentialStickersToAdd)
        {
            // TODO: remember to update when mystman12 fixes the bugs with this
            Dictionary<Sticker, int> totalInPossesion = new Dictionary<Sticker, int>();
            for (int i = 0; i < amount; i++)
            {
                ____potentialStickersToAdd.Clear();
                totalInPossesion.Clear();
                StickerMetaData[] allStickers = StickerMetaStorage.Instance.All();
                for (int j = 0; j < allStickers.Length; j++)
                {
                    totalInPossesion.Add(allStickers[j].value.sticker, __instance.TotalInPosession(allStickers[j].value.sticker));
                }
                int minValue = int.MaxValue;
                foreach (var kvp in totalInPossesion)
                {
                    minValue = Mathf.Min(minValue, kvp.Value);
                }
                foreach (WeightedSticker weightedSticker in potentialStickers)
                {
                    if (__instance.TotalInPosession(weightedSticker.selection) <= minValue)
                    {
                        ____potentialStickersToAdd.Add(new WeightedSticker(weightedSticker.selection, Mathf.RoundToInt((float)weightedSticker.weight * __instance.GetStickerOddsMultiplier(weightedSticker.selection))));
                    }
                }
                if (____potentialStickersToAdd.Count == 0)
                {
                    MTM101BaldiDevAPI.Log.LogWarning("Game would've exceptioned here, out of fresh stickers, increasing odds...");
                    while (____potentialStickersToAdd.Count == 0)
                    {
                        minValue++;
                        foreach (WeightedSticker weightedSticker in potentialStickers)
                        {
                            if (__instance.TotalInPosession(weightedSticker.selection) <= minValue)
                            {
                                ____potentialStickersToAdd.Add(new WeightedSticker(weightedSticker.selection, Mathf.RoundToInt((float)weightedSticker.weight * __instance.GetStickerOddsMultiplier(weightedSticker.selection))));
                            }
                        }
                    }
                }
                __instance.AddSticker(____potentialStickersToAdd.RandomSelection(), openNow, false, openNow);
                //AddRandomSticker(__instance, ____potentialStickersToAdd.RandomSelection(), openNow, false);
            }
            return false;
        }

        /*
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GiveNewRandomStickers")]
        [HarmonyPriority(Priority.First)]
        static IEnumerable<CodeInstruction> GiveNewRandomStickersTranspiler(IEnumerable<CodeInstruction> instructionsE) => GenericGiveRandomStickersTranspiler(instructionsE, "GiveNewRandomStickers");*/

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
                    MTM101BaldiDevAPI.Log.LogDebug(message + " patched at instruction: " + i);
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
