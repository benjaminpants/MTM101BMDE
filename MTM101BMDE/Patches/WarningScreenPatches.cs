using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MTM101BaldAPI.Patches
{



    static internal class WarningScreenContainer
    {
        static internal (string, bool)[] screens => nonCriticalScreens.Select(x => (x, false)).ToArray().AddRangeToArray(criticalScreens.Select(x => (x, true)).ToArray()).ToArray();

        static internal List<string> nonCriticalScreens = new List<string>();

        static internal List<string> criticalScreens = new List<string>();

        static internal int currentPage = 0; //since we will never be returning to the warning screen again, just have this as a static variable.

        static internal string pressAny = "";
    }

    [HarmonyPatch(typeof(WarningScreen))]
    [HarmonyPatch("Start")]
    [HarmonyPriority(800)]
    static class WarningScreenStartPatch
    {
        static bool Prefix(WarningScreen __instance)
        {
            string text = "";
            if (Singleton<InputManager>.Instance.SteamInputActive)
            {
                text = string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Men_Warning"), Singleton<InputManager>.Instance.GetInputButtonName("MouseSubmit"));
                WarningScreenContainer.pressAny = string.Format("PRESS {0} TO CONTINUE", Singleton<InputManager>.Instance.GetInputButtonName("MouseSubmit"));
            }
            else
            {
                text = string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Men_Warning"), "ANY BUTTON");
                WarningScreenContainer.pressAny = string.Format("PRESS {0} TO CONTINUE", "ANY BUTTON");
            }
            WarningScreenContainer.nonCriticalScreens.Insert(0, text);
            __instance.textBox.text = text;
            return false;
        }
    }
    [HarmonyPatch(typeof(WarningScreen))]
    [HarmonyPatch("Advance")]
    [HarmonyPriority(800)]
    static class WarningScreenAdvancePatch
    {
        static bool Prefix(WarningScreen __instance)
        {
            if (WarningScreenContainer.currentPage >= WarningScreenContainer.screens.Length)
            {
                return true;
            }
            if ((WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item2) && ((WarningScreenContainer.currentPage + 1) >= WarningScreenContainer.screens.Length))
            {
                return false; //dont allow passing a critical screen
            }
            WarningScreenContainer.currentPage++;
            if (WarningScreenContainer.currentPage >= WarningScreenContainer.screens.Length)
            {
                return true;
            }
            Singleton<GlobalCam>.Instance.Transition(UiTransition.Dither, 0.01666667f);
            if (!WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item2)
            {
                __instance.textBox.text = "<color=yellow>WARNING!</color>\n" + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1 + "\n\n" + WarningScreenContainer.pressAny;
            }
            else
            {
                if (((WarningScreenContainer.currentPage + 1) < WarningScreenContainer.screens.Length))
                {
                    __instance.textBox.text = "<color=red>ERROR!</color>\n" + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1 + "\n\n" + WarningScreenContainer.pressAny;
                }
                else
                {
                    __instance.textBox.text = "<color=red>ERROR!</color>\n" + WarningScreenContainer.screens[WarningScreenContainer.currentPage].Item1 + "\n\nPRESS ALT+F4 TO EXIT";
                }
            }
            return false;
        }
    }
}
