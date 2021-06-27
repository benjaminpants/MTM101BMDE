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
    [BepInPlugin("mtm101.rulerp.bbplus.baldinamemenu", "BB+ Name Menu API", "1.1.0.0")]



    public class BaldiNameAPI : BaseUnityPlugin
    {
        bool funnyvariable;
        public object ChangeFunnyVariable()
        {
            UnityEngine.Debug.Log(funnyvariable);
            funnyvariable = !funnyvariable;
            return funnyvariable;
        }

        public void CloseGame()
        {
            Application.Quit();
        }

        public void CrashTheGameBecauseFuckYou()
        {
            Environment.Exit(0);
        }

        void Awake()
        {
            
            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldinamemenu");
            List<Name_MenuObject> RootMenu = new List<Name_MenuObject>();
            RootMenu.Add(new Name_MenuTitle("generic_title","Welcome!"));
            RootMenu.Add(new Name_MenuFolder("goto_start", "Start", "save_select"));
            RootMenu.Add(new Name_MenuFolder("options", "Options", "options"));
            RootMenu.Add(new Name_MenuGeneric("exit", "Exit", CloseGame));
            NameMenuManager.AddPage("root", "root");
            NameMenuManager.AddPage("options", "root");
            NameMenuManager.AddPage("bbnmoptions", "options");
            NameMenuManager.AddToPage("options",new Name_MenuFolder("bbnmoptions", "BB+ Name Menu", "bbnmoptions"));
            NameMenuManager.AddToPage("bbnmoptions", new Name_MenuOption("change_test", "Test Option", funnyvariable, ChangeFunnyVariable));
            NameMenuManager.AddToPage("bbnmoptions", new Name_MenuGeneric("crash", "Crash The Game Lol", CrashTheGameBecauseFuckYou));
            NameMenuManager.AddToPageBulk("root",RootMenu);

            harmony.PatchAll();

        }
    }
}
