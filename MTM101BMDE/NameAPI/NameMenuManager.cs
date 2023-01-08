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

        static MethodInfo loadInfo = AccessTools.Method(typeof(NameManager), "Load");
        public static void Refresh()
        {
            NameManager inst = NameManager.nm;
            loadInfo.Invoke(inst,null);
            inst.UpdateState();
        }

        public static void AddPage(IPage page)
        {
            Pages.Add(page.Name, page);
        }

        public static void SwitchToPage(IPage page)
        {
            CurrentPageName = page.Name;
        }

        public static void SwitchToPage(string page)
        {
            CurrentPageName = page;
        }

        public static void ReturnFromPage(IPage page)
        {
            if (page.rootPage == null) return;
            Type parenttype = page.rootPage.GetType();
            if (typeof(IPageFolder).IsAssignableFrom(parenttype))
            {
                ((IPageFolder)page.rootPage).GoToPage(-1); //parent is a "folder", let that handle switching back
            }
            else //parent is not a folder, assume its stored in the pages dictionary.
            {
                CurrentPageName = page.rootPage.Name; //send us to that page.
            }
            Refresh(); //reload?
        }

        public static string CurrentPageName = "root";
        public static IPage CurrentPage {
            get
            {
                return Pages[NameMenuManager.CurrentPageName];
            }
        }
        public static Dictionary<string, IPage> Pages = new Dictionary<string, IPage>();

        public static event Action<string> OnNameClicked;

        /// <summary>
        /// Don't call this ever. This is for internal use. Its public because im lazy.
        /// </summary>
        public static void CallNameClicked(string name)
        {
            if (OnNameClicked != null)
            {
                OnNameClicked(name);
            }
        }
    }


}
