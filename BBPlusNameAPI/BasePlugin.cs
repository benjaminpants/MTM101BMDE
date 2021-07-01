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
//this code is reused from BaldiMP and BB+ twitch
namespace BBPlusNameAPI
{
    [BepInPlugin("mtm101.rulerp.bbplus.baldinamemenu", "BB+ Name Menu API", "1.2.0.0")]



    public class BaldiNameAPI : BaseUnityPlugin
    {
        bool funnyvariable;
        string currentmod = "mtm101.rulerp.bbplus.baldinamemenu";
        public object ChangeFunnyVariable()
        {
            funnyvariable = !funnyvariable;
            return funnyvariable;
        }

        public void CloseGame(Name_MenuObject obj)
        {
            Application.Quit();
        }

        public void CrashTheGameBecauseFuckYou(Name_MenuObject obj)
        {
            Environment.Exit(0);
        }

        public void SetCurrentMod(Name_MenuObject obj)
        {
            currentmod = obj.Name; //we set this to the GUID
            NameMenuManager.Current_Page = "moddata";
        }

        public List<Name_MenuObject> ReturnObjs()
        {
            List<Name_MenuObject> objs = new List<Name_MenuObject>();
            List<BaseUnityPlugin> plugins = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList();
            foreach (BaseUnityPlugin plugin in plugins)
            {
                objs.Add(new Name_MenuGeneric(plugin.Info.Metadata.GUID, plugin.Info.Metadata.Name, SetCurrentMod));
            }

            return objs;
        }

        public List<Name_MenuObject> ReturnData()
        {
            List<Name_MenuObject> objs = new List<Name_MenuObject>();
            BaseUnityPlugin plugin = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.GUID == currentmod);
            objs.Add(new Name_MenuTitle("GUID", "GUID:" + plugin.Info.Metadata.GUID));
            objs.Add(new Name_MenuTitle("VER", "Version:" + plugin.Info.Metadata.Version.Major + "." + plugin.Info.Metadata.Version.Minor));
            objs.Add(new Name_MenuTitle("BUILD", "Build:" + plugin.Info.Metadata.Version.Build));
            objs.Add(new Name_MenuTitle("REV", "Revision:" + plugin.Info.Metadata.Version.Revision));
            objs.Add(new Name_MenuTitle("LOC", "DLL Name:" + System.IO.Path.GetFileName(plugin.Info.Location)));

            return objs;
        }


        void Awake()
        {
            
            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldinamemenu");
            List<Name_MenuObject> RootMenu = new List<Name_MenuObject>();
            RootMenu.Add(new Name_MenuTitle("generic_title","Welcome!"));
            RootMenu.Add(new Name_MenuFolder("goto_start", "Start", "save_select"));
            RootMenu.Add(new Name_MenuFolder("options", "Options", "options"));
            RootMenu.Add(new Name_MenuFolder("modspage", "Mods", "modslist"));
            RootMenu.Add(new Name_MenuGeneric("exit", "Exit", CloseGame));
            NameMenuManager.AddPage("root", "root");
            NameMenuManager.AddPage("options", "root");
            NameMenuManager.AddPage("bbnmoptions", "options");
            NameMenuManager.AddPage("modslist", "root", ReturnObjs);
            NameMenuManager.AddPage("moddata", "modslist", ReturnData);
            NameMenuManager.AddToPage("options",new Name_MenuFolder("bbnmoptions", "BB+ Name Menu", "bbnmoptions"));
            NameMenuManager.AddToPage("bbnmoptions", new Name_MenuOption("change_test", "Test Option", funnyvariable, ChangeFunnyVariable));
            NameMenuManager.AddToPage("bbnmoptions", new Name_MenuGeneric("crash", "Crash The Game Lol", CrashTheGameBecauseFuckYou));
            NameMenuManager.AddToPageBulk("root",RootMenu);


            harmony.PatchAll();

        }
    }
}
