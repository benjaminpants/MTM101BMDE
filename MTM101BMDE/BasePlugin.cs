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
using MTM101BaldAPI.OptionsAPI;

//this code is reused from BaldiMP and BB+ twitch
namespace MTM101BaldAPI
{


    [BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "BB+ Dev API", VersionNumber)]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {

        public const string VersionNumber = "1.3.0.0";

        public static bool IsClassicRemastered
        {
            get
            {
                return Application.temporaryCachePath.Contains("Basically Games/Baldi's Basics Classic Remastered"); //thanks to fasguy
            }
        }

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

        public void CloseGame(IPageButton but, IPage p)
        {
            Application.Quit();
        }

        static Page rootPage = new Page("root");

        public static Folder optionsPage = new Folder("options", new Page("optionsroot"),new List<IPage>());

#if DEBUG
        void OnMen(OptionsMenu __instance)
        {
            GameObject ob = CustomOptionsCore.CreateNewCategory(__instance, "Test Menu");
            MenuToggle ch = CustomOptionsCore.CreateToggleButton(__instance, new Vector2(0f, 0f), "Checkbox", false, "Defaults to \"false\"");
            ch.transform.SetParent(ob.transform, false);

            TextLocalizer lol = CustomOptionsCore.CreateText(__instance, new Vector2(-70f, 70f), "Test Text");
            lol.transform.SetParent(ob.transform, false);

            StandardMenuButton lolagain = CustomOptionsCore.CreateTextButton(__instance, new Vector2(-70f, 40f), "HEY!", "This is a button that prints \"HEY!!\" in the console.", () =>
            {
                UnityEngine.Debug.Log("HEY!!");
            });
            lolagain.transform.SetParent(ob.transform, false);

            AdjustmentBars bar = CustomOptionsCore.CreateAdjustmentBar(__instance, new Vector2(-96f, -40f), "barTiny", 2, "Tiny Bar", 0, () =>
            {
                UnityEngine.Debug.Log("1");
            });
            bar.transform.SetParent(ob.transform, false);
            bar = CustomOptionsCore.CreateAdjustmentBar(__instance, new Vector2(-96f, -70f), "barSmall", 6, "Small Bar", 1, () =>
            {
                UnityEngine.Debug.Log("2");
            });
            bar.transform.SetParent(ob.transform, false);
            bar = CustomOptionsCore.CreateAdjustmentBar(__instance, new Vector2(-96f, -100f), "barNorm", 10, "Normal Bar", 5, () =>
            {
                UnityEngine.Debug.Log("3");
            });
            bar.transform.SetParent(ob.transform, false);
            bar = CustomOptionsCore.CreateAdjustmentBar(__instance, new Vector2(-96f, -130f), "barBig", 15, "Big Bar", 1, () =>
            {
                UnityEngine.Debug.Log("4");
            });
            bar.transform.SetParent(ob.transform, false);
            bar = CustomOptionsCore.CreateAdjustmentBar(__instance, new Vector2(-96f, -160f), "barHuge", 22, "Huge Bar", 11, () =>
            {
                UnityEngine.Debug.Log("5");
            });
            bar.transform.SetParent(ob.transform, false);

            StandardMenuButton b = CustomOptionsCore.CreateApplyButton(__instance, "Apply button. Prints \"APPLY!\" in the console.", () =>
            {
                UnityEngine.Debug.Log("APPLY!");
            });

            b.transform.SetParent(ob.transform, false);

            //CustomOptionsCore.CreateNewCategory(__instance, "Empty Menu");

            //CustomOptionsCore.CreateNewCategory(__instance, "Still Empty");
        }
#endif

#if DEBUG
        void GoToTester(IPageButton but, IPage page)
        {
            NameMenuManager.SwitchToPage("tester");
        }
        void DebugFolder()
        {
            List<IPage> pl = new List<IPage>();
            Page pB = new Page("TEST");
            Folder f = new Folder("tester", pB, pl);
            for (int i = 0; i < 4; i++)
            {
                Page p = new Page("TEST");
                p.buttons.Add(new StringInput("debug", "d:%v", null, null));
                p.buttons.Add(new Button("lol", "Return", f.ReturnToDefaultPage));
                f.AddPage(p);
            }
            void goToRandom(IPageButton but, IPage page)
            {
                f.GoToPage(UnityEngine.Random.Range(0,3));
            }
            pB.buttons.Add(new Button("random","Random", goToRandom));
            NameMenuManager.AddPage(f);
            rootPage.buttons.Add(new Button("testBut", "Tester", GoToTester));
        }
#endif

        void Awake()
        {
            /*NameMenuManager.AddPage(rootPage);
            optionsPage.rootPage = rootPage;
            optionsPage.showReturn = true;
            NameMenuManager.AddPage(optionsPage);
            //Button testBut = new StringInput("testBut", "Value: %v", null, null);
            rootPage.buttons.Add(new Button("welcomeTitle", "Welcome!", null));
            rootPage.buttons.Add(new Button("startBut", "Start", GoToStart));
            rootPage.buttons.Add(new Button("optionsBut","Options",GoToOptions));*/
            NameMenuManager.SwitchToPage("save_select");
#if DEBUG
            DebugFolder();
            CustomOptionsCore.OnMenuInitialize += OnMen;
#endif
            //rootPage.buttons.Add(new Button("exitBut", "Exit", CloseGame));
            //rootPage.buttons.Add(testBut);

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");
			BaseUnityPlugin namemenu = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.Name == "BB+ Name Menu API");
			if (namemenu != null)
			{
				Application.Quit();
			}
            

			harmony.PatchAll();

        }
    }


    //Handle patching appropiate functions to allow for the version number to be patched
    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class InjectAPINameName
    {
        static void Postfix(NameManager __instance)
        {
            Transform t = __instance.transform.parent.Find("Version Number");
            TMPro.TMP_Text text = t.gameObject.GetComponent<TMPro.TMP_Text>();
            text.text += "API " + MTM101BaldiDevAPI.VersionNumber;
            t.localPosition += new Vector3(0f, 28f);
        }
    }

    [HarmonyPatch(typeof(MainMenu))]
    [HarmonyPatch("Start")]
    class InjectAPIMainName
    {
        static void Postfix(MainMenu __instance)
        {
            Transform t = __instance.transform.Find("Reminder");
            TMPro.TMP_Text text = t.gameObject.GetComponent<TMPro.TMP_Text>();
            text.text = "Modding API " + MTM101BaldiDevAPI.VersionNumber;
        }
    }
}
