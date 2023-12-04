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
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.AssetTools;
using UnityCipher;
using MTM101BaldAPI.Reflection;

namespace MTM101BaldAPI
{


    [BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "BB+ Dev API", VersionNumber)]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public const string VersionNumber = "3.0.0.0";

        internal static bool CalledInitialize = false;

        public static MTM101BaldiDevAPI Instance;

        internal static List<ScriptableObject> keepInMemory = new List<ScriptableObject>();

        public static ItemMetaStorage itemMetadata = new ItemMetaStorage();
        public static NPCMetaStorage npcMetadata = new NPCMetaStorage();

        internal static AssetManager AssetMan = new AssetManager();

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
            Instance = this;

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");
			BaseUnityPlugin namemenu = GameObject.FindObjectsOfType<BaseUnityPlugin>().ToList().Find(x => x.Info.Metadata.Name == "BB+ Name Menu API");
			if (namemenu != null)
			{
				Application.Quit();
			}

            harmony.PatchAllConditionals();

            ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string myPath) =>
            {
                if (isSave)
                {
                    File.WriteAllText(Path.Combine(myPath, "testData.txt"), "This data doesn't actually store anything (yet)!!");
                }
            });

            Log = base.Logger;
        }
    }


    //Handle patching appropiate functions to allow for the version number to be patched
    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    public class InjectAPINameName
    {
        static void Postfix(NameManager __instance)
        {
            //the version number stuff
            Transform t = __instance.transform.parent.Find("Version Number");
            TMPro.TMP_Text text = t.gameObject.GetComponent<TMPro.TMP_Text>();
            text.text += "API " + MTM101BaldiDevAPI.VersionNumber;
            t.localPosition += new Vector3(0f, 28f);
            if (MTM101BaldiDevAPI.CalledInitialize) return;
            // define all metadata before we call OnAllAssetsLoaded, so we can atleast be a bit more sure no other mods have activated and added their stuff yet.

            // INITIALIZE ITEM METADATA
            ItemObject grapplingHook = null;
            Resources.FindObjectsOfTypeAll<ItemObject>().Do(x =>
            {
                switch (x.itemType)
                {
                    case Items.PortalPoster:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Physical);
                        break;
                    case Items.GrapplingHook:
                        if (grapplingHook == null)
                        {
                            grapplingHook = x;
                        }
                        break;
                    case Items.Bsoda:
                        ItemMetaData bm = x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.Physical);
                        bm.tags.Add("food");
                        bm.tags.Add("drink");
                        break;
                    case Items.AlarmClock:
                    case Items.ChalkEraser:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.Physical);
                        break;
                    case Items.Boots:
                    case Items.Teleporter:
                    case Items.Nametag:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists);
                        break;
                    case Items.Apple:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.NoUses).tags.Add("food");
                        break;
                    case Items.None:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.NoUses);
                        break;
                    case Items.ZestyBar:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None).tags.Add("food");
                        break;
                    case Items.Quarter:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None).tags.Add("currency");
                        break;
                    case Items.Wd40:
                    case Items.DetentionKey:
                    case Items.Tape:
                    case Items.Scissors:
                    case Items.PrincipalWhistle:
                    case Items.DoorLock:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None);
                        break;
                    default:
                        MTM101BaldiDevAPI.Log.LogWarning("Unknown core item: " + x.itemType.ToString() + "! Can't add metadata!");
                        break;
                }
            });
            ItemMetaData grappleMeta = new ItemMetaData(MTM101BaldiDevAPI.Instance.Info, (ItemObject[])((ITM_GrapplingHook)grapplingHook.item).ReflectionGetVariable("allVersions"));
            grappleMeta.flags = ItemFlags.Physical | ItemFlags.MultipleUse | ItemFlags.Persists;
            grappleMeta.itemObjects.Do(x =>
            {
                x.AddMeta(grappleMeta);
            });
            // INITIALIZE CHARACTER METADATA
            NPC[] NPCs = Resources.FindObjectsOfTypeAll<NPC>();
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Baldi).ToArray(), "Baldi", NPCFlags.StandardAndHear));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Principal).ToArray(), "Principal", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Beans).ToArray(), "Beans", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Chalkles).ToArray(), "ChalkFace", NPCFlags.StandardNoCollide | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Cumulo).ToArray(), "CloudyCopter", NPCFlags.Standard)); // they do have a trigger it just doesn't do anything
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Bully).ToArray(), "Bully", NPCFlags.StandardNoCollide | NPCFlags.IsBlockade));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Pomp).ToArray(), "Mrs Pomp", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Playtime).ToArray(), "Playtime", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Crafters).ToArray(), "Arts and Crafters", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Sweep).ToArray(), "Gotta Sweep", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.LookAt).ToArray(), "LookAt", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Prize).ToArray(), "FirstPrize", NPCFlags.Standard | NPCFlags.MakeNoise));


            MTM101BaldiDevAPI.CalledInitialize = true;
            //everything else
            if (LoadingEvents.OnAllAssetsLoaded != null)
            {
                LoadingEvents.OnAllAssetsLoaded.Invoke();
            }
            SceneObject[] objs = Resources.FindObjectsOfTypeAll<SceneObject>();
            foreach (SceneObject obj in objs)
            {
                if (obj.levelObject == null) continue;
#if DEBUG
                MTM101BaldiDevAPI.Log.LogInfo(String.Format("Invoking SceneObject({0})({2}) Generation Changes for {1}!", obj.levelTitle, obj.levelObject.ToString(), obj.levelNo.ToString()));
#endif
                GeneratorManagement.Invoke(obj.levelTitle, obj.levelNo, obj.levelObject);
            }
            foreach (KeyValuePair<string, byte[]> kvp in AssetLoader.MidisToBeAdded)
            {
                AssetLoader.MidiFromBytes(kvp.Key, kvp.Value);
            }
            AssetLoader.MidisToBeAdded = null;
            if (LoadingEvents.OnAllAssetsLoadedPost != null)
            {
                LoadingEvents.OnAllAssetsLoadedPost.Invoke();
            }

        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    [HarmonyPatch("SetSave")]
    class DisableSave
    {
        static void Prefix(ref bool val)
        {
            val = val & MTM101BaldiDevAPI.SavesEnabled;
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
