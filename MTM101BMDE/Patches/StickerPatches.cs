using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{

    class StickerInitFixRan : MonoBehaviour
    {

    }

    // TODO: consider just. making the methods that take in an ExtendedStickerStateData take in a StickerStateData instead.
    // TODO: implement saving
    [HarmonyPatch]
    class StickerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Last)]
        static void StickerUpdatePrefix(StickerManager __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return;
            if (!__instance.TryGetComponent<StickerInitFixRan>(out _))
            {
                for (int i = 0; i < __instance.activeStickerData.Length; i++)
                {
                    if (__instance.activeStickerData[i].GetType() == typeof(StickerStateData))
                    {
                        Debug.Log("Correcting " + i + "!");
                        __instance.activeStickerData[i] = StickerMetaStorage.Instance.Get(__instance.activeStickerData[i].sticker).value.CreateStickerData(__instance.activeStickerData[i].activeLevel, __instance.activeStickerData[i].opened);
                    }
                }
                __instance.gameObject.AddComponent<StickerInitFixRan>();
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("ApplySticker")]
        [HarmonyPriority(Priority.Last)]
        static bool ApplyStickerPrefix(Sticker sticker, int slot, StickerManager __instance, StickerManager.StickerAppliedDelegate ___OnStickerApplied)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            __instance.activeStickerData[slot] = StickerMetaStorage.Instance.Get(sticker).value.CreateStickerData(Singleton<BaseGameManager>.Instance.CurrentLevel, true);
            ___OnStickerApplied.Invoke();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerCanBeCovered")]
        [HarmonyPriority(Priority.Last)]
        static bool StickerCanBeCoveredPrefix(int slot, StickerManager __instance, ref bool __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[slot].sticker).value.CanBeCovered((ExtendedStickerStateData)__instance.activeStickerData[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerCanBeApplied")]
        [HarmonyPriority(Priority.Last)]
        static bool StickerCanBeAppliedPrefix(Sticker sticker, StickerManager __instance, ref bool __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            __result = StickerMetaStorage.Instance.Get(sticker).value.CanBeApplied();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("StickerValue")]
        [HarmonyPriority(Priority.Last)]
        static void StickerValuePostfix(Sticker sticker, ref int __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return;
            __result = Mathf.Min(__result, StickerMetaStorage.Instance.Get(sticker).value.stickerValueCap);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetAppliedStickerSprite")]
        [HarmonyPriority(Priority.Last)]
        static bool GetAppliedStickerSpritePrefix(StickerManager __instance, int inventoryId, ref Sprite __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[inventoryId].sticker).value.GetAppliedSprite((ExtendedStickerStateData)__instance.activeStickerData[inventoryId]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetInventoryStickerSprite")]
        [HarmonyPriority(Priority.Last)]
        static bool GetInventoryStickerSpritePrefix(StickerManager __instance, int inventoryId, ref Sprite __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
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
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.activeStickerData[slot].sticker).value.GetLocalizedAppliedStickerDescription((ExtendedStickerStateData)__instance.activeStickerData[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("GetLocalizedInventoryStickerDescription")]
        [HarmonyPriority(Priority.Last)]
        static bool GetLocalizedInventoryStickerDescriptionPrefix(StickerManager __instance, int slot, ref string __result)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            if (!__instance.stickerInventory[slot].opened) return true;
            __result = StickerMetaStorage.Instance.Get(__instance.stickerInventory[slot].sticker).value.GetLocalizedInventoryStickerDescription((ExtendedStickerStateData)__instance.stickerInventory[slot]);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StickerManager))]
        [HarmonyPatch("OpenUnopenedStickerPackets")]
        [HarmonyPriority(Priority.Last)]
        static bool OpenUnopenedStickerPacketsPrefix(StickerManager __instance, bool animation)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
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
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            Sticker chosenSticker = WeightedSelection<Sticker>.RandomSelection(Singleton<CoreGameManager>.Instance.sceneObject.potentialStickers);
            Singleton<StickerManager>.Instance.AddSticker(chosenSticker, false, false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PitstopGameManager))]
        [HarmonyPatch("GiveRandomSticker")]
        [HarmonyPriority(Priority.Last)]
        static bool PitstopGiveRandomStickerPrefix()
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Vanilla) return true;
            Sticker chosenSticker = WeightedSelection<Sticker>.RandomSelection(Singleton<CoreGameManager>.Instance.nextLevel.potentialStickers);
            Singleton<StickerManager>.Instance.AddSticker(chosenSticker, true, true);
            return false;
        }
    }
}
