using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MTM101BaldAPI.OptionsAPI
{
    public static class CustomOptionsCore
    {
        /// <summary>
        /// This gets called when the Option's Menu gets created. This function may be called multiple times.
        /// </summary>
        public static event Action<OptionsMenu> OnMenuInitialize;

        /// <summary>
        /// This gets called whenever the options menu gets closed, so you can save your stuff.
        /// </summary>
        public static event Action<OptionsMenu> OnMenuClose;

        public static void CallOnMenuInitialize(OptionsMenu m)
        {
            if (OnMenuInitialize != null)
            {
                OnMenuInitialize(m);
            }
        }

        public static void CallOnMenuClose(OptionsMenu m)
        {
            if (OnMenuClose != null)
            {
                OnMenuClose(m);
            }
        }
    }

    public class CustomOptionsHandler : MonoBehaviour // WOW! this exists!
    {

    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Awake")]
    class OnOptionAwake
    {
        static void Postfix(OptionsMenu __instance)
        {
            if (__instance.gameObject.GetComponent<CustomOptionsHandler>()) return;
            __instance.gameObject.AddComponent<CustomOptionsHandler>();
            CustomOptionsCore.CallOnMenuInitialize(__instance);
        }
    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Close")]
    class OnOptionClose
    {
        static void Postfix(OptionsMenu __instance)
        {
            CustomOptionsCore.CallOnMenuClose(__instance);
        }
    }
}
