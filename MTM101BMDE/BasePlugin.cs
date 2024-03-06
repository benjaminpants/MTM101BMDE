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
using TMPro;
using System.Collections;

namespace MTM101BaldAPI
{
    public enum SavedGameDataHandler
    {
        Vanilla,
        Modded,
        None
    }

    [BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "BB+ Dev API", VersionNumber)]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public const string VersionNumber = "3.2.1.0";

        internal static bool CalledInitialize = false;

        public static MTM101BaldiDevAPI Instance;

        internal static List<UnityEngine.Object> keepInMemory = new List<UnityEngine.Object>();

        public static ItemMetaStorage itemMetadata = new ItemMetaStorage();
        public static NPCMetaStorage npcMetadata = new NPCMetaStorage();
        public static RandomEventMetaStorage randomEventStorage = new RandomEventMetaStorage();
        public static ObjectBuilderMetaStorage objBuilderMeta = new ObjectBuilderMetaStorage();

        public static RoomAssetMetaStorage roomAssetMeta = new RoomAssetMetaStorage();

        internal static AssetManager AssetMan = new AssetManager();

        public static bool SaveGamesEnabled
        {
            get
            {
                return saveHandler != SavedGameDataHandler.None;
            }
            set
            {
                saveHandler = value ? (ModdedSaveGame.ModdedSaveGameHandlers.Count > 0 ? SavedGameDataHandler.Modded : SavedGameDataHandler.Vanilla) : SavedGameDataHandler.None;
            }
        }

        public static SavedGameDataHandler SaveGamesHandler
        {
            get
            {
                return saveHandler;
            }
            set
            {
                switch (saveHandler)
                {
                    case SavedGameDataHandler.Modded:
                    case SavedGameDataHandler.None:
                        saveHandler = value;
                        break;
                    case SavedGameDataHandler.Vanilla:
                        if (ModdedSaveGame.ModdedSaveGameHandlers.Count == 0)
                        {
                            saveHandler = value;
                        }
                        saveHandler = SavedGameDataHandler.Modded;
                        break;
                }
            }
        }

        internal static SavedGameDataHandler saveHandler = SavedGameDataHandler.Vanilla;

        internal IEnumerator ReloadScenes()
        {
            AsyncOperation waitForSceneLoad = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            while (!waitForSceneLoad.isDone)
            {
                yield return null;
            }
            AsyncOperation waitForSceneUnload = SceneManager.UnloadSceneAsync("Game");
            while (!waitForSceneUnload.isDone)
            {
                yield return null;
            }
            OnSceneUnload();
            yield break;
        }

        internal void OnSceneUnload()
        {
            // INITIALIZE ITEM METADATA
            ItemObject grapplingHook = null;
            Resources.FindObjectsOfTypeAll<ItemObject>().Do(x =>
            {
                switch (x.itemType)
                {
                    case Items.PortalPoster:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists); //todo: double check
                        break;
                    case Items.GrapplingHook:
                        if ((grapplingHook == null) &&
                        (((ITM_GrapplingHook)x.item).uses == 4))
                        {
                            grapplingHook = x;
                        }
                        break;
                    case Items.Bsoda:
                        ItemMetaData bm = x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity);
                        bm.tags.Add("food");
                        bm.tags.Add("drink");
                        break;
                    case Items.AlarmClock:
                    case Items.ChalkEraser:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity);
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
                        // modded items start at 256, so we somehow have initialized after the mod in question, ignore the data.
                        if ((int)x.itemType < 256)
                        {
                            MTM101BaldiDevAPI.Log.LogWarning("Unknown core item: " + x.itemType.ToString() + "! Can't add metadata!");
                        }
                        break;
                }
            });
            ItemMetaData grappleMeta = new ItemMetaData(MTM101BaldiDevAPI.Instance.Info, (ItemObject[])((ITM_GrapplingHook)grapplingHook.item).ReflectionGetVariable("allVersions"));
            grappleMeta.itemObjects = grappleMeta.itemObjects.AddItem(grapplingHook).ToArray();
            grappleMeta.flags = ItemFlags.CreatesEntity | ItemFlags.MultipleUse | ItemFlags.Persists;
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
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Bully).ToArray(), "Bully", NPCFlags.Standard | NPCFlags.IsBlockade));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Pomp).ToArray(), "Mrs Pomp", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Playtime).ToArray(), "Playtime", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Crafters).ToArray(), "Arts and Crafters", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Sweep).ToArray(), "Gotta Sweep", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.LookAt).ToArray(), "LookAt", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Prize).ToArray(), "FirstPrize", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.DrReflex).ToArray(), "DrReflex", NPCFlags.Standard));
            Resources.FindObjectsOfTypeAll<RoomAsset>().Do(x =>
            {
                RoomAssetMetaStorage.Instance.Add(new RoomAssetMeta(MTM101BaldiDevAPI.Instance.Info, x));
            });

            Resources.FindObjectsOfTypeAll<RandomEvent>().Do(x =>
            {
                switch (x.Type)
                {
                    default:
                        MTM101BaldiDevAPI.Log.LogWarning("Unknown random event type: " + x.Type.ToStringExtended() + ". Unable to add meta!");
                        break;
                    case RandomEventType.Party:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x, new RoomCategory[1] { RoomCategory.Office }));
                        break;
                    case RandomEventType.Snap:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x, new Character[1] { Character.Baldi }));
                        break;
                    case RandomEventType.Fog:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x));
                        break;
                    case RandomEventType.Gravity:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x));
                        break;
                    case RandomEventType.MysteryRoom:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x, RandomEventFlags.AffectsGenerator));
                        break;
                    case RandomEventType.Flood:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x));
                        break;
                    case RandomEventType.Lockdown:
                        RandomEventMetaStorage.Instance.Add(new RandomEventMetadata(MTM101BaldiDevAPI.Instance.Info, x, RandomEventFlags.Permanent));
                        break;
                }
            });
            Resources.FindObjectsOfTypeAll<ObjectBuilder>().Do(x =>
            {
                ObjectBuilderMeta meta = new ObjectBuilderMeta(MTM101BaldiDevAPI.Instance.Info, x);
                ObjectBuilderMetaStorage.Instance.Add(meta);
            });

            MTM101BaldiDevAPI.CalledInitialize = true;

            MTM101BaldiDevAPI.Instance.AssetsLoadPre();
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
            // dumb dumb test code
            /*LevelObject testObj = objs.Where(x => x.levelTitle == "F1").First().levelObject;
            testObj.forcedNpcs = testObj.forcedNpcs.AddToArray(ObjectCreators.CreateNPC<TestNPC>("TestMan", Character.Null));*/
        }

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

        internal void AssetsLoadPre()
        {
            AssetMan.Add("ErrorTemplate", Resources.FindObjectsOfTypeAll<Canvas>().Where(x => x.name == "EndingError").First());
            AssetMan.Add("WindowTemplate", Resources.FindObjectsOfTypeAll<WindowObject>().Where(x => x.name == "WoodWindow").First());
            AssetMan.Add("DoorTemplate", Resources.FindObjectsOfTypeAll<StandardDoorMats>().Where(x => x.name == "ClassDoorSet").First());
            PosterObject baldiposter = Resources.FindObjectsOfTypeAll<PosterObject>().Where(x => x.name == "BaldiPoster").First();
            PosterObject posterTemplate = ScriptableObject.Instantiate<PosterObject>(baldiposter);
            posterTemplate.name = "CharacterPosterTemplate";
            posterTemplate.baseTexture = null;
            AssetMan.Add<PosterObject>("CharacterPosterTemplate", posterTemplate);
            NPC templateNpc = GameObject.Instantiate<NPC>(Resources.FindObjectsOfTypeAll<Beans>().First());
            templateNpc.GetComponent<Entity>().SetActive(false); //disable the entity
            templateNpc.gameObject.SetActive(false);
            templateNpc.name = "TemplateNPC";
            // handle audio manager stuff
            PropagatedAudioManager audMan = templateNpc.GetComponent<PropagatedAudioManager>();
            GameObject.Destroy(audMan.audioDevice.gameObject);
            audMan.sourceId = 0; //reset source id
            AudioManager.totalIds--; //decrement total ids
            GameObject templateObject = templateNpc.gameObject;
            GameObject.DontDestroyOnLoad(templateObject);
            GameObject.DestroyImmediate(templateObject.GetComponent<Beans>());
            GameObject.DestroyImmediate(templateObject.GetComponent<Animator>());
            AssetMan.Add<GameObject>("TemplateNPC", templateObject);
            templateObject.layer = LayerMask.NameToLayer("NPCs");
            MTM101BaldAPI.Registers.Buttons.ButtonColorManager.InitializeButtonColors();
        }

        // "GUYS IM GONNA USE THIS FOR MY CUSTOM ERROR SCREEN FOR MY FUNNY 4TH WALL BREAK IN MY MOD!"
        // just dont. please. only use this function for actual errors.
        public static void CauseCrash(PluginInfo plug, Exception e)
        {
            Canvas template = MTM101BaldiDevAPI.AssetMan.Get<Canvas>("ErrorTemplate");
            if (template == null)
            {
                MTM101BaldiDevAPI.Log.LogError("Attempted to cause a crash before the ErrorTemplate was found!");
            }
            GameObject error = GameObject.Instantiate<Canvas>(template).gameObject;
            error.GetComponent<Canvas>().sortingOrder = 99; //make this appear above everything
            TextMeshProUGUI text = error.GetComponentInChildren<TextMeshProUGUI>();
            text.text = String.Format(@"
{0} HAS ENCOUNTERED AN ERROR(S)
{1} <size=60%>{2}<size=100%>
PLEASE REPORT TO THE MOD DEVELOPER(S).
PRESS ALT+F4 TO EXIT THE GAME.
", plug.Metadata.GUID.ToUpper(), e.Message, e.StackTrace);
            if (Singleton<BaseGameManager>.Instance != null)
            {
                Singleton<BaseGameManager>.Instance.Ec.AddTimeScale(new TimeScaleModifier() { environmentTimeScale = 0, npcTimeScale = 0 });
                Time.timeScale = 0f;
            }
            if (CursorController.Instance != null)
            {
                CursorController.Instance.enabled = false;
            }
            error.gameObject.SetActive(true);
            throw e; //rethrow the error
        }

        void Awake()
        {
#if DEBUG
            CustomOptionsCore.OnMenuInitialize += OnMen;
#endif
            Instance = this;

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");

            harmony.PatchAllConditionals();

            ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string myPath) =>
            {
                if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
                if (isSave)
                {
                    //File.WriteAllText(Path.Combine(myPath, "testData.txt"), "This data doesn't actually store anything (yet)!!");
                    FileStream fs = File.OpenWrite(Path.Combine(myPath, "savedgame0.bbapi"));
                    fs.SetLength(0); // make sure to clear the contents before writing to it!
                    BinaryWriter writer = new BinaryWriter(fs);
                    Singleton<ModdedFileManager>.Instance.saveData.Save(writer);
                    writer.Close();
                }
                else
                {
                    if (!File.Exists(Path.Combine(myPath, "savedgame0.bbapi"))) return;
                    FileStream fs = File.OpenRead(Path.Combine(myPath, "savedgame0.bbapi"));
                    BinaryReader reader = new BinaryReader(fs);
                    ModdedSaveLoadStatus status = Singleton<ModdedFileManager>.Instance.saveData.Load(reader);
                    reader.Close();
                    switch (status)
                    {
                        default:
                            break;
                        case ModdedSaveLoadStatus.MissingHandlers:
                            MTM101BaldiDevAPI.Log.LogWarning("Failed to load save because one or more mod handlers were missing!");
                            Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
                            break;
                        case ModdedSaveLoadStatus.MissingItems:
                            if (itemMetadata.All().Length == 0) break; //item metadata hasnt loaded yet!
                            MTM101BaldiDevAPI.Log.LogWarning("Failed to load save because one or more items couldn't be found!");
                            Singleton<ModdedFileManager>.Instance.saveData.saveAvailable = false;
                            break;
                        case ModdedSaveLoadStatus.NoSave:
                            MTM101BaldiDevAPI.Log.LogInfo("No save data was found.");
                            break;
                        case ModdedSaveLoadStatus.Success:
                            MTM101BaldiDevAPI.Log.LogInfo("Modded Savedata was succesfully loaded!");
                            break;
                    }
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

            MTM101BaldiDevAPI.Instance.StartCoroutine(MTM101BaldiDevAPI.Instance.ReloadScenes());
            /*
            SceneManager.LoadScene("Game", LoadSceneMode.Additive);
#pragma warning disable CS0618 // Type or member is obsolete
            SceneManager.UnloadScene("Game"); // we need it to be synced
#pragma warning restore CS0618 // Type or member is obsolete
            */

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
            text.gameObject.transform.position += new Vector3(-7f,0f, 0f);
        }
    }
}
