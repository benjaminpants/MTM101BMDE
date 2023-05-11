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
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using System.IO;

//this code is reused from BaldiMP and BB+ twitch
namespace MTM101BaldAPI
{


    [BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "BB+ Dev API", VersionNumber)]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {

        public const string VersionNumber = "2.0.0.0";

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
                allowsaves = value;
            }
        }

        private static bool allowsaves = true;

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

        void Awake()
        {
#if DEBUG
            CustomOptionsCore.OnMenuInitialize += OnMen;
#endif

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");
			BaseUnityPlugin namemenu = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.Name == "BB+ Name Menu API");
			if (namemenu != null)
			{
				Application.Quit();
			}

			harmony.PatchAll();

            ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string myPath) =>
            {
                if (isSave)
                {
                    File.WriteAllText(Path.Combine(myPath, "testData.txt"), "This data doesn't actually store anything!!");
                }
            });

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
