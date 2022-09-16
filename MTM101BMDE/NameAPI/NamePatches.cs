using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
using System.IO;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Linq;
//more stuff
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection.Emit;

namespace MTM101BaldAPI.NameMenu
{


        /*[HarmonyReversePatch]
        [HarmonyPatch(typeof(HijackLoadDelay), "MyLoadDelay")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++) //Goes through every instruction
            {
                if (codes[i].opcode == OpCodes.Ldstr) //If its a string
                {
                    var scene = codes[i].operand as string; //Get the value as a string
                    if (scene == "MainMenu") 
                    {
                        codes[i].operand = "NameEntry"
                    }
                }
            }
            return codes.AsEnumerable();
        }*/



    [HarmonyPatch(typeof(NameButton))]
    [HarmonyPatch("Highlight")]
    class HijackHighlight
    {
        static bool Prefix(ref int ___fileNo)
        {
            if (NameMenuManager.Current_Page == "save_select" || ___fileNo == 7) return true;
            List<MenuObject> currentelements = NameMenuManager.Folders.Find(x => x.pagename == NameMenuManager.Current_Page).GetElements();
            if (currentelements == null) return true;
            if (currentelements.Count > ___fileNo)
            {
                return !(currentelements[___fileNo].GetType() == typeof(MenuTitle));
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Load")]
    class HijackDefaultLoad
    {

        const float Fontsize = 24f;
        static bool Prefix(NameManager __instance, ref string[] ___nameList, ref NameButton[] ___buttons)
        {
            for (int i = 0; i <= 6; i++)
            {
                ___buttons[i].text.fontSize = Fontsize;
            }
            if (NameMenuManager.Current_Page == "save_select") return true;
            if (NameMenuManager.Current_Page != "root")
            {
                ___nameList[7] = NameMenuManager.Pending_Start ? (NameMenuManager.NeedsManditoryAction ? "" : "Continue") : "Return";
            }
            else
            {
                ___nameList[7] = "";
            }
            for (int i = 0; i <= 6; i++)
            {
                ___nameList[i] = "";
            }
            List<MenuObject> currentelements = NameMenuManager.Folders.Find(x => x.pagename == NameMenuManager.Current_Page).GetElements();
            for (int i = 0; i < currentelements.Count; i++)
            {
                ___nameList[i] = currentelements[i].GetName();
				if (___buttons[i] == null) continue;
                if (___nameList[i].Length > 11)
                {
                    ___buttons[i].text.fontSize = Fontsize * (11f / (float)(___nameList[i].Length));
                }
                else
                {
                    ___buttons[i].text.fontSize = Fontsize;
                }
            }
            __instance.UpdateState();
            return false;
        }

    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Update")]
    class NoNameUpdate
    {
        static bool Prefix()
        {
            return NameMenuManager.Current_Page == "save_select";
        }
    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("NameClicked")]
    class ModifyNameClick
    {
        static bool Prefix(NameManager __instance, int fileNo, ref string[] ___nameList)
        {
            if (NameMenuManager.Current_Page == "save_select")
            {
                NameMenuManager.CallNameClicked(___nameList[fileNo]);
                return true;
            }
            if (fileNo != 7)
            {
                List<MenuObject> currentelements = NameMenuManager.Folders.Find(x => x.pagename == NameMenuManager.Current_Page).GetElements();
                if (currentelements.Count > fileNo)
                {
                    currentelements[fileNo].Press();
                }
            }
            else
            {
                if (!NameMenuManager.Pending_Start)
                {
                    if (NameMenuManager.Current_Page != "root")
                    {
                        NameMenuManager.Current_Page = NameMenuManager.Folders.Find(x => x.pagename == NameMenuManager.Current_Page).prevpage;
                    }
                }
                else
                {
                    if (!NameMenuManager.NeedsManditoryAction)
                    {
                        NameMenuManager.Current_Page = "save_select";
                    }
                }
            }
			if (NameMenuManager.Current_Page == "save_select")
			{
				if (NameMenuManager.PendingPages.Count != 0)
				{
					NameMenuManager.Pending_Start = true;
					NameMenuManager.Current_Page = NameMenuManager.PendingPages[0];
					NameMenuManager.NeedsManditoryAction = NameMenuManager.Folders.Find(x => x.pagename == NameMenuManager.Current_Page).manditory;
					NameMenuManager.PendingPages.RemoveAt(0);
				}
			}
			___nameList = new string[8];

			__instance.InvokeMethod("Load");
            __instance.UpdateState();

            return false;
        }
    }



    
}
