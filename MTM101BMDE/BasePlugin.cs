using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib; //god im hoping i got the right version of harmony
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;
using MTM101BaldAPI.NameMenu;

//this code is reused from BaldiMP and BB+ twitch
namespace MTM101BaldAPI
{
	[BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "BB+ Dev API", "1.1.0.0")]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {
        string currentmod = "mtm101.rulerp.bbplus.baldidevapi";






		public static bool SavesEnabled
		{
			get
			{
				return allowsaves;
			}
			set
			{
				if (value == false)
				{
					allowsaves = false;
				}
				else
				{
					UnityEngine.Debug.LogWarning("You can't re-enable saves once a mod has disabled them!");
				}
			}
		}

		private static bool allowsaves = true;

        public void CloseGame(MenuObject obj)
        {
            Application.Quit();
        }

        public void SetCurrentMod(MenuObject obj)
        {
            currentmod = obj.Name; //we set this to the GUID
            NameMenuManager.Current_Page = "moddata";
        }

        public List<MenuObject> ReturnObjs()
        {
            List<MenuObject> objs = new List<MenuObject>();
            List<BaseUnityPlugin> plugins = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList();
            foreach (BaseUnityPlugin plugin in plugins)
            {
                objs.Add(new MenuGeneric(plugin.Info.Metadata.GUID, plugin.Info.Metadata.Name, SetCurrentMod));
            }

            return objs;
        }

        public List<MenuObject> ReturnData()
        {
            List<MenuObject> objs = new List<MenuObject>();
            BaseUnityPlugin plugin = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.GUID == currentmod);
            objs.Add(new MenuTitle("GUID", "GUID:" + plugin.Info.Metadata.GUID));
            objs.Add(new MenuTitle("VER", "Version:" + plugin.Info.Metadata.Version.Major + "." + plugin.Info.Metadata.Version.Minor));
            objs.Add(new MenuTitle("BUILD", "Build:" + plugin.Info.Metadata.Version.Build));
            objs.Add(new MenuTitle("REV", "Revision:" + plugin.Info.Metadata.Version.Revision));
            objs.Add(new MenuTitle("LOC", "DLL Name:" + System.IO.Path.GetFileName(plugin.Info.Location)));

            return objs;
        }


        void Awake()
        {
            
            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");
            List<MenuObject> RootMenu = new List<MenuObject>();
            RootMenu.Add(new MenuTitle("generic_title","Welcome!"));
            RootMenu.Add(new MenuFolder("goto_start", "Start", "save_select"));
            RootMenu.Add(new MenuFolder("options", "Options", "options"));
            RootMenu.Add(new MenuFolder("modspage", "Mods", "modslist"));
            RootMenu.Add(new MenuGeneric("exit", "Exit", CloseGame));
            NameMenuManager.AddPage("root", "root");
            NameMenuManager.AddPage("options", "root");
            NameMenuManager.AddPage("bbnmoptions", "options");
            NameMenuManager.AddPage("modslist", "root", ReturnObjs);
            NameMenuManager.AddPage("moddata", "modslist", ReturnData);
            NameMenuManager.AddToPageBulk("root",RootMenu);
			BaseUnityPlugin namemenu = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.Name == "BB+ Name Menu API");
			if (namemenu != null)
			{
				Application.Quit();
			}


			harmony.PatchAll();

        }
    }
}
