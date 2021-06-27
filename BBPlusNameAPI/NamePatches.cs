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
//more stuff
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace BBPlusNameAPI
{
    public static class NameMenuManager
    {
        public static string Current_Page = "root";
        public static Dictionary<string, List<Name_MenuObject>> Folders = new Dictionary<string, List<Name_MenuObject>>();
        public static Dictionary<string, string> PrevPages = new Dictionary<string, string>();
        public static List<string> PendingPages = new List<string>();
        public static List<bool> RequiresManditoryAction = new List<bool>();
        public static string Prev_Page = "root";
        public static bool Pending_Start;
        public static bool NeedsManditoryAction;


        public static void AddPage(string pagename, string returnto)
        {
            Folders.Add(pagename,new List<Name_MenuObject>());
            PrevPages.Add(pagename,returnto);
        }

        public static void AddPreStartPage(string pagename, bool requiresmanditoryaction)
        {
            Folders.Add(pagename, new List<Name_MenuObject>());
            PrevPages.Add(pagename, pagename);
            PendingPages.Add(pagename);
            RequiresManditoryAction.Add(requiresmanditoryaction);
        }

        public static void AddToPage(string pagename, Name_MenuObject obj)
        {
            Folders[pagename].Add(obj);
        }

        public static void AddToPageBulk(string pagename, List<Name_MenuObject> objs)
        {
            Folders[pagename].AddRange(objs);
        }

        public static void AllowContinue(bool instant)
        {

            if (!instant)
            {
                NeedsManditoryAction = false;
            }
            else
            {
                NameMenuManager.Current_Page = "save_select";
                if (NameMenuManager.PendingPages.Count != 0)
                {
                    NameMenuManager.Pending_Start = true;
                    NameMenuManager.Current_Page = NameMenuManager.PendingPages[0];
                    NameMenuManager.NeedsManditoryAction = NameMenuManager.RequiresManditoryAction[0];
                    NameMenuManager.PendingPages.RemoveAt(0);
                    NameMenuManager.RequiresManditoryAction.RemoveAt(0);
                }
            }
            NameManager.nm.InvokeMethod("Load");
            NameManager.nm.UpdateState();
        }


        public static object InvokeMethod<T>(this T obj, string methodName, params object[] args) //thank you owen james: https://stackoverflow.com/users/2736798/owen-james
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return method.Invoke(obj, args);
        }

    }


    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("ToggelDeleteMode")]
    class DisableDelete
    {
        static bool Prefix()
        {
            return (NameMenuManager.Current_Page == "save_select");
        }
    }



    [HarmonyPatch(typeof(NameButton))]
    [HarmonyPatch("Highlight")]
    class HijackHighlight
    {
        static bool Prefix(ref int ___fileNo)
        {
            if (NameMenuManager.Current_Page == "save_select" || ___fileNo == 7) return true;
            try //fix this later im too tired for this shit
            {
                if (NameMenuManager.Folders[NameMenuManager.Current_Page][___fileNo] != null)
                {
                    return !(NameMenuManager.Folders[NameMenuManager.Current_Page][___fileNo].GetType() == typeof(Name_MenuTitle));
                }
            }
            catch
            {

            }
            return false;
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
            for (int i = 0; i < NameMenuManager.Folders[NameMenuManager.Current_Page].Count; i++)
            {
                ___nameList[i] = NameMenuManager.Folders[NameMenuManager.Current_Page][i].GetName();
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
        static bool Prefix(NameManager __instance, int fileNo)
        {
            if (NameMenuManager.Current_Page == "save_select") return true;
            if (fileNo != 7)
            {
                if (NameMenuManager.Folders[NameMenuManager.Current_Page][fileNo] != null)
                {
                    NameMenuManager.Folders[NameMenuManager.Current_Page][fileNo].Press();
                }
            }
            else
            {
                if (!NameMenuManager.Pending_Start)
                {
                    if (NameMenuManager.Current_Page != "root")
                    {
                        NameMenuManager.Current_Page = NameMenuManager.PrevPages[NameMenuManager.Current_Page];
                    }
                }
                else
                {
                    NameMenuManager.Current_Page = "save_select";
                }
            }
            if (NameMenuManager.Current_Page == "save_select")
            {
                if (NameMenuManager.PendingPages.Count != 0)
                {
                    NameMenuManager.Pending_Start = true;
                    NameMenuManager.Current_Page = NameMenuManager.PendingPages[0];
                    NameMenuManager.NeedsManditoryAction = NameMenuManager.RequiresManditoryAction[0];
                    NameMenuManager.PendingPages.RemoveAt(0);
                    NameMenuManager.RequiresManditoryAction.RemoveAt(0);
                }
            }
            __instance.InvokeMethod("Load");
            __instance.UpdateState();

            return false;
        }
    }



    
}
