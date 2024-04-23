using HarmonyLib;
using UnityEngine.SceneManagement;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(WarningScreen), "Start")]
    internal class WarningScreenCustomText
    {
        internal static bool preventAdvance = false;
        private static string forceText = null;

        internal static bool Prefix(WarningScreen __instance)
        {
            if (forceText != null)
            {
                __instance.textBox.SetText(forceText);
            }
            if (preventAdvance)
            {
                // Returning false will prevent Baldi to format the text to "Click button to continue"
                return false;
            }
            return true;
        }
        internal static void ShowWarningScreen(string text)
        {
            forceText = text;
            preventAdvance = true;
            SceneManager.LoadScene("Warnings");
        }
    }
    [HarmonyPatch(typeof(WarningScreen), "Advance")]
    internal class WarningScreenPreventAdvance
    {
        internal static bool Prefix() => !WarningScreenCustomText.preventAdvance;
    }
}
