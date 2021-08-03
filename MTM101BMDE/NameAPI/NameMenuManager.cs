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


namespace MTM101BaldAPI.NameMenu
{
    public static class NameMenuManager
    {

        public static string Current_Page = "root";
        public static List<Page> Folders = new List<Page>();
        public static List<string> PendingPages = new List<string>();
        public static string Prev_Page = "root";
        public static bool Pending_Start;
        public static bool NeedsManditoryAction;

        public static void AddPage(string pagename, string returnto, Func<List<MenuObject>> func = null)
        {
            Folders.Add(new Page(pagename, returnto, false, func != null, func));
        }

        public static void AddPreStartPage(string pagename, bool requiresmanditoryaction, Func<List<MenuObject>> func = null)
        {
            Folders.Add(new Page(pagename, "", requiresmanditoryaction, func != null, func));
            PendingPages.Add(pagename);
        }

        public static void AddToPage(string pagename, MenuObject obj)
        {
            Folders.Find(x => x.pagename == pagename).Elements.Add(obj);
        }

        public static void AddToPageBulk(string pagename, List<MenuObject> objs)
        {
            Folders.Find(x => x.pagename == pagename).Elements.AddRange(objs);
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
                    NameMenuManager.NeedsManditoryAction = Folders.Find(x => x.pagename == Current_Page).manditory;
                    NameMenuManager.PendingPages.RemoveAt(0);
                }
            }
            NameManager.nm.InvokeMethod("Load");
            NameManager.nm.UpdateState();
        }

		//this function is stupid? i think i know a method that I believe is faster so I'll be using that in the future
        public static object InvokeMethod<T>(this T obj, string methodName, params object[] args) //thank you owen james: https://stackoverflow.com/users/2736798/owen-james
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return method.Invoke(obj, args);
        }


    }


}
