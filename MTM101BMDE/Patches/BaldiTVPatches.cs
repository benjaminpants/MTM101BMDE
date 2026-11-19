using HarmonyLib;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(BaldiTV))]
    class BaldiTVPatches
    {
        static FieldInfo _BaldiSpeaks_sound = AccessTools.Field(AccessTools.Method(typeof(BaldiTV), "BaldiSpeaks").GetCustomAttribute<StateMachineAttribute>().StateMachineType, "sound"); // thank you AlexBW145!

        [HarmonyPatch("QueueEnumerator")]
        [HarmonyPrefix]
        static void QueueEnumeratorPrefix(BaldiTV __instance, ref IEnumerator enumerator)
        {
            if (enumerator.GetType().Name.StartsWith("<BaldiSpeaks>d__24"))
            {
                SoundObject sound = (SoundObject)_BaldiSpeaks_sound.GetValue(enumerator);
                string characterToSayLine = BaldiTVExtensionHandler.GetCharacterForSoundObject(sound);
                if (characterToSayLine == "baldi") return; // no need to change the enumerator
                enumerator = BaldiTVExtensionHandler.GetCharacterEnumerator(characterToSayLine, __instance, sound);
            }
        }

        [HarmonyPatch("ReInit")]
        [HarmonyPostfix]
        static void ReInitPatch(BaldiTV __instance)
        {
            if (__instance.TryGetComponent(out BaldiTVExtensionContainer container))
            {
                container.tvCharacter.CleanupSpeaker(container.image, __instance);
                GameObject.Destroy(container);
            }
        }
    }
}