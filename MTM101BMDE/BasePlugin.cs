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
using MTM101BaldAPI.UI;
using UnityEngine.UI;
using MidiPlayerTK;
using MTM101BaldAPI.Patches;
using MTM101BaldAPI.Components;

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
        internal static ManualLogSource Log = new ManualLogSource("BB+ Dev API Pre Initialization");

        public const string VersionNumber = "5.0.0.0";

        /// <summary>
        /// The version of the API, applicable when BepInEx cache messes up the version number.
        /// </summary>
        public static Version Version => new Version(VersionNumber);

        internal static bool CalledInitialize = false;

        public static MTM101BaldiDevAPI Instance;

        internal static List<UnityEngine.Object> keepInMemory = new List<UnityEngine.Object>();

        internal ConfigEntry<bool> usingMidiFix;

        internal Sprite[] questionMarkSprites;

        public static ItemMetaStorage itemMetadata = new ItemMetaStorage();
        public static NPCMetaStorage npcMetadata = new NPCMetaStorage();
        public static RandomEventMetaStorage randomEventStorage = new RandomEventMetaStorage();
        public static ObjectBuilderMetaStorage objBuilderMeta = new ObjectBuilderMetaStorage();
        public static SkyboxMetaStorage skyboxMeta = new SkyboxMetaStorage();

        public static RoomAssetMetaStorage roomAssetMeta = new RoomAssetMetaStorage();

        internal ConfigEntry<bool> useOldAudioLoad;

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
            /*set
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
            }*/
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
            Singleton<GlobalCam>.Instance.StopCurrentTransition();
            // INITIALIZE ITEM METADATA
            ItemObject grapplingHook = null;
            List<ItemObject> pointObjects = new List<ItemObject>();
            Resources.FindObjectsOfTypeAll<ItemObject>().Do(x =>
            {
                switch (x.itemType)
                {
                    case Items.PortalPoster:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None);
                        break;
                    case Items.GrapplingHook:
                        if ((grapplingHook == null) &&
                        (((ITM_GrapplingHook)x.item).uses == 4))
                        {
                            grapplingHook = x;
                        }
                        break;
                    case Items.DietBsoda:
                    case Items.Bsoda:
                        ItemMetaData bm = x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity);
                        bm.tags.Add("food");
                        bm.tags.Add("drink");
                        break;
                    case Items.AlarmClock:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity).tags.AddRange(new string[] { "technology", "makes_noise" });
                        break;
                    case Items.ChalkEraser:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity);
                        break;
                    case Items.Boots:
                    case Items.Teleporter:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists).tags.Add("technology");
                        break;
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
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None).tags.Add("sharp");
                        break;
                    case Items.PrincipalWhistle:
                    case Items.DoorLock:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.None);
                        break;
                    case Items.NanaPeel:
                        ItemMetaData bana = x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.Persists | ItemFlags.CreatesEntity);
                        bana.tags.Add("food");
                        break;
                    case Items.Points:
                        pointObjects.Add(x);
                        break;
                    case Items.Map:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.InstantUse).tags.Add("shop_dummy");
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
            // handle point metadata
            pointObjects.Sort((a, b) =>
            {
                return ((int)a.ReflectionGetVariable("value")).CompareTo((int)b.ReflectionGetVariable("value"));
            });
            ItemMetaData pointItemData = new ItemMetaData(MTM101BaldiDevAPI.Instance.Info, pointObjects.ToArray());
            pointItemData.flags = ItemFlags.InstantUse;
            pointObjects.ForEach(x =>
            {
                x.AddMeta(pointItemData);
            });
            // INITIALIZE CHARACTER METADATA
            NPC[] NPCs = Resources.FindObjectsOfTypeAll<NPC>();
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Baldi).ToArray(), "Baldi", NPCFlags.StandardAndHear));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Principal).ToArray(), "Principal", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Beans).ToArray(), "Beans", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Chalkles).ToArray(), "ChalkFace", NPCFlags.StandardNoCollide | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Cumulo).ToArray(), "CloudyCopter", NPCFlags.Standard)); // they do have a trigger it just doesn't do anything
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Bully).ToArray(), "Bully", (NPCFlags.Standard | NPCFlags.IsBlockade) & ~NPCFlags.CanMove));
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

            Cubemap[] cubemaps = Resources.FindObjectsOfTypeAll<Cubemap>();
            skyboxMeta.Add(new SkyboxMetadata(Info, cubemaps.Where(x => x.name == "Cubemap_DayStandard").First(), Color.white));
            skyboxMeta.Add(new SkyboxMetadata(Info, cubemaps.Where(x => x.name == "Cubemap_Twilight").First(), Color.white /*new Color(239f / 255f, 188f / 255f, 162f / 255f)*/));

            MTM101BaldiDevAPI.CalledInitialize = true;

            if (usingMidiFix.Value)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream("MTM101BaldAPI.gm.sf2");
                if (stream == null)
                {
                    throw new Exception("Midifix stream Is null! Turn off the midifix in BepInEx/config!");
                }
                File.WriteAllBytes(Path.Combine(Application.temporaryCachePath, "gm.sf2"), stream.ToByteArray());
                MidiPlayerGlobal.MPTK_LoadLiveSF("file://" + Path.Combine(Application.temporaryCachePath, "gm.sf2"));
                stream.Dispose();
            }

            MTM101BaldiDevAPI.Instance.AssetsLoadPre();

            // loading screen
            CursorController.Instance.DisableClick(true);
            Texture2D whiteTexture = new Texture2D(480, 360);
            whiteTexture.name = "WhiteBG";
            List<Color> pixels = new List<Color>();
            for (int i = 0; i < 480 * 360; i++)
            {
                pixels.Add(Color.white);
            }
            whiteTexture.SetPixels(pixels.ToArray());
            whiteTexture.Apply();
            Transform transformToTry = null;
            transformToTry = GameObject.Find(GameObject.Find("NameEntry") ? "NameEntry" : "Menu").transform;
            Image whiteBG = UIHelpers.CreateImage(AssetLoader.SpriteFromTexture2D(whiteTexture, 1f), transformToTry, Vector3.zero, false);
            whiteBG.gameObject.AddComponent<ModLoadingScreenManager>();
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
        }
#endif

        internal static GameObject PrefabSubObject;


        static FieldInfo _allEntities = AccessTools.Field(typeof(Entity), "allEntities");
        internal void AssetsLoadPre()
        {
            GameObject internalIdentity = Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.name == "InternalIdentityTransform");
            GameObject subChild = new GameObject("SubObject");
            subChild.transform.SetParent(internalIdentity.transform, false);
            subChild.AddComponent<DestroyOnAwakeInstantWithWarning>();
            PrefabSubObject = subChild;
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
            GameObject.DestroyImmediate(templateObject.GetComponent<Beans>());
            GameObject.DestroyImmediate(templateObject.GetComponent<Animator>());
            templateObject.layer = LayerMask.NameToLayer("NPCs");
            ((List<Entity>)_allEntities.GetValue(null)).Remove(templateObject.GetComponent<Entity>());
            templateObject.ConvertToPrefab(false);
            AssetMan.Add<GameObject>("TemplateNPC", templateObject);
            MTM101BaldAPI.Registers.Buttons.ButtonColorManager.InitializeButtonColors();
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            AssetMan.Add<Sprite>("MenuArrow",allSprites.Where(x => x.name == "MenuArrowSheet_2").First());
            AssetMan.Add<Sprite>("MenuArrowHighlight", allSprites.Where(x => x.name == "MenuArrowSheet_0").First());
            AssetMan.Add<Sprite>("Bar", allSprites.Where(x => x.name == "MenuBarSheet_0").First());
            AssetMan.Add<Sprite>("BarTransparent", allSprites.Where(x => x.name == "MenuBarSheet_1").First());
            AssetMan.AddFromResources<Shader>();
            questionMarkSprites = allSprites.Where(x => x.texture.name == "QMarkSheet").ToArray();
            SoundObject[] allSoundObjects = Resources.FindObjectsOfTypeAll<SoundObject>();
            AssetMan.Add<SoundObject>("Xylophone", allSoundObjects.Where(x => x.name == "NotebookCollect").First());
            AssetMan.Add<SoundObject>("Explosion", allSoundObjects.Where(x => x.name == "GlassBreak").First());
            AssetMan.AddFromResources<TMPro.TMP_FontAsset>();
            AssetMan.AddFromResources<Material>();
        }

        internal void ConvertAllLevelObjects()
        {
            SceneObject[] sceneObjects = Resources.FindObjectsOfTypeAll<SceneObject>();
            foreach (SceneObject objct in sceneObjects)
            {
                if (objct.levelObject == null) continue;
                CustomLevelObject customizedObject = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(objct.levelObject);
                customizedObject.name = objct.levelObject.name;
                Destroy(objct.levelObject);
                objct.levelObject = customizedObject;
                objct.levelObject.MarkAsNeverUnload();
                objct.MarkAsNeverUnload();
            }
        }

        /// <summary>
        /// Please only use this function for actual errors.
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="e"></param>
        public static void CauseCrash(PluginInfo plug, Exception e)
        {
            Canvas template = MTM101BaldiDevAPI.AssetMan.Get<Canvas>("ErrorTemplate");
            if (template == null)
            {
                MTM101BaldiDevAPI.Log.LogError("Attempted to cause a crash before the ErrorTemplate was found!");
                return;
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

        public static void AddWarningScreen(string text, bool fatal)
        {
            if (fatal)
            {
                WarningScreenContainer.criticalScreens.Add(text);
            }
            else
            {
                WarningScreenContainer.nonCriticalScreens.Add(text);
            }
        }

        void Awake()
        {
#if DEBUG
            CustomOptionsCore.OnMenuInitialize += OnMen;
#endif
            CustomOptionsCore.OnMenuInitialize += SaveManagerMenu.MenuHook;
            Instance = this;

            Harmony harmony = new Harmony("mtm101.rulerp.bbplus.baldidevapi");

            ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string myPath) =>
            {
                if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
                int appropiateId = Singleton<ModdedFileManager>.Instance.FindAppropiateSaveGame(myPath, false);
                Singleton<ModdedFileManager>.Instance.saveIndex = appropiateId;
                if (isSave)
                {
                    Singleton<ModdedFileManager>.Instance.SaveGameWithIndex(myPath, Singleton<ModdedFileManager>.Instance.saveIndex);
                    Singleton<ModdedFileManager>.Instance.SaveFileList(myPath);
                }
                else
                {
                    Singleton<ModdedFileManager>.Instance.LoadGameWithIndex(myPath, Singleton<ModdedFileManager>.Instance.saveIndex);
                }
                Singleton<ModdedFileManager>.Instance.UpdateCurrentPartialSave();
            });

            useOldAudioLoad = Config.Bind("Technical",
                "Use Old Audio Loading Method",
                false,
                "Whether or not the old, legacy method of loading audio should be used. (ONLY TURN ON IF YOU GET MENTIONS OF AN AUDIO LOADING ERROR!)");

            usingMidiFix = Config.Bind("Technical",
                "Use Midi Fix",
                true,
                "Whether or not the midi fix should be used to increase the amount of instruments available to the midi player, there shouldn't be a reason for you to disable this.");

            if (useOldAudioLoad.Value)
            {
                AddWarningScreen("Old Audio Loading is <b>on!</b>\nYou should not need this anymore as of API 4.0!\nTurn it off, and if mods are still broken, report it to MTM101!", false);
            }

            //handled by the patch system so i do not need to check it
            Config.Bind("Generator",
                "Enable Skybox Patches",
                true,
                "Whether or not outdoors areas will have different light colors depending on the skybox used. Only disable for legacy mods.");

            ConfigEntry<bool> genConfig = Config.Bind("Generator",
                "Enable Custom Room Support",
                true,
                "Enables/Disables the support for Custom Rooms provided by the CustomLevelData class. ONLY TURN OFF IF YOU ABSOLUTELY HAVE TO! THIS WILL BREAK MODS!");

            if (!genConfig.Value)
            {
                AddWarningScreen("Custom Room Support is <b>off</b>!\nCertain mods may break or otherwise not function!",false);
            }
            harmony.PatchAllConditionals();

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
            if (GameObject.Find("NameList")) { GameObject.Find("NameList").GetComponent<AudioSource>().enabled = false; }

        }
    }

    [HarmonyPatch(typeof(MenuInitializer))]
    [HarmonyPatch("Start")]
    public class CallAPIInitializationFunctions
    {
        static void Prefix()
        {
            if (MTM101BaldiDevAPI.CalledInitialize) return;
            // define all metadata before we call OnAllAssetsLoaded, so we can atleast be a bit more sure no other mods have activated and added their stuff yet.
            MTM101BaldiDevAPI.Instance.StartCoroutine(MTM101BaldiDevAPI.Instance.ReloadScenes());
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
            text.gameObject.SetActive(true); // so the pre-releases don't hide the version number
            text.text = "Modding API " + MTM101BaldiDevAPI.VersionNumber;
            text.gameObject.transform.position += new Vector3(-7f,0f, 0f);
        }
    }
}
