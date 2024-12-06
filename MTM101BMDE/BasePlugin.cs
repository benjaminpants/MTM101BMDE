﻿using System;
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
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MTM101BaldAPI.ErrorHandler;

namespace MTM101BaldAPI
{
    public enum SavedGameDataHandler
    {
        Vanilla,
        Modded,
        None
    }

    [BepInPlugin("mtm101.rulerp.bbplus.baldidevapi", "Baldi's Basics Plus Dev API", VersionNumber)]
    public class MTM101BaldiDevAPI : BaseUnityPlugin
    {
        internal static ManualLogSource Log = new ManualLogSource("Baldi's Basics Plus Dev API Pre Initialization");

        public const string VersionNumber = "6.0.0.0";

        /// <summary>
        /// The version of the API, applicable when BepInEx cache messes up the version number.
        /// </summary>
        public static Version Version => new Version(VersionNumber);

        internal static bool CalledInitialize = false;

        public static MTM101BaldiDevAPI Instance;

        internal static List<UnityEngine.Object> keepInMemory = new List<UnityEngine.Object>();

        internal ConfigEntry<bool> usingMidiFix;

        internal ConfigEntry<bool> ignoringTagDisplays;

        internal ConfigEntry<bool> attemptOnline;

        internal Sprite[] questionMarkSprites;

        public static ItemMetaStorage itemMetadata = new ItemMetaStorage();
        public static NPCMetaStorage npcMetadata = new NPCMetaStorage();
        public static RandomEventMetaStorage randomEventStorage = new RandomEventMetaStorage();
        public static SkyboxMetaStorage skyboxMeta = new SkyboxMetaStorage();
        public static SceneObjectMetaStorage sceneMeta = new SceneObjectMetaStorage();

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

        public static bool SaveGameHasMods
        {
            get
            {
                return ModdedSaveGame.ModdedSaveGameHandlers.Count > 0;
            }
        }

        public static SavedGameDataHandler SaveGamesHandler
        {
            get
            {
                return saveHandler;
            }
        }

        internal static SavedGameDataHandler saveHandler = SavedGameDataHandler.Vanilla;

        public static GameLoader gameLoader;

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

        private IEnumerator WaitForSoundfontLoad(string toDelete)
        {
            yield return null;
            yield return null;
            while (!MidiPlayerGlobal.MPTK_SoundFontLoaded)
            {
                yield return null;
            }
            yield return null;
            MTM101BaldiDevAPI.Log.LogDebug("Soundfont loaded! Deleting from temp folder...");
            File.Delete(toDelete);
        }

        public string newestGBVersion = "unknown";

        IEnumerator GetCurrentGamebananaVersion()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get("https://api.gamebanana.com/Core/Item/Data?itemtype=Mod&itemid=383711&fields=Updates().aGetLatestUpdates()");
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Log.LogError("Unable to access Gamebanana API! Is Gamebanana down?");
                yield break;
            }
            string json = webRequest.downloadHandler.text;
            JToken gbResults = JToken.Parse(json); //parse as jtoken instead of JObject

            newestGBVersion = gbResults[0][0]["_sVersion"].Value<string>();
            yield break;
        }

        internal void OnSceneUnload()
        {
            gameLoader = Resources.FindObjectsOfTypeAll<GameLoader>().First(x => x.GetInstanceID() >= 0);
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
                    case Items.BusPass:
                        x.AddMeta(MTM101BaldiDevAPI.Instance, ItemFlags.NoUses);
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
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Baldi).ToArray(), "Baldi", NPCFlags.StandardAndHear, new string[] { "teacher", "faculty" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Principal).ToArray(), "Principal", NPCFlags.Standard, new string[] { "faculty" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Beans).ToArray(), "Beans", NPCFlags.Standard, new string[] { "student" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Chalkles).ToArray(), "ChalkFace", NPCFlags.StandardNoCollide | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Cumulo).ToArray(), "CloudyCopter", NPCFlags.Standard)); // they do have a trigger it just doesn't do anything
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Bully).ToArray(), "Bully", (NPCFlags.Standard | NPCFlags.IsBlockade) & ~NPCFlags.CanMove, new string[] { "student" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Pomp).ToArray(), "Mrs Pomp", NPCFlags.Standard | NPCFlags.MakeNoise, new string[] { "teacher", "faculty" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Playtime).ToArray(), "Playtime", NPCFlags.Standard, new string[] { "student" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Crafters).ToArray(), "Arts and Crafters", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Sweep).ToArray(), "Gotta Sweep", NPCFlags.Standard, new string[] { "faculty" }));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.LookAt).ToArray(), "LookAt", NPCFlags.Standard));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.Prize).ToArray(), "FirstPrize", NPCFlags.Standard | NPCFlags.MakeNoise));
            NPCMetaStorage.Instance.Add(new NPCMetadata(MTM101BaldiDevAPI.Instance.Info, NPCs.Where(x => x.Character == Character.DrReflex).ToArray(), "DrReflex", NPCFlags.StandardAndHear, new string[] { "faculty" }));
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


            // get all sceneobjects to add metadata
            Resources.FindObjectsOfTypeAll<SceneObject>().Where(x => x.GetInstanceID() >= 0).Do(x =>
            {
                switch (x.name)
                {
                    case "MainLevel_1":
                    case "MainLevel_2":
                    case "MainLevel_3":
                        x.AddMeta(this, new string[] { "main", "found_on_main" });
                        break;
                    case "EndlessRandomMedium":
                    case "EndlessPremadeMedium":
                        x.AddMeta(this, new string[] { "endless" });
                        break;
                    case "PlaceholderEnding":
                        x.AddMeta(this, new string[] { "ending", "found_on_main" });
                        break;
                    case "Pitstop":
                        x.AddMeta(this, new string[] { "pitstop" });
                        break;
                    case "Camping":
                        x.AddMeta(this, new string[] { "fieldtrip" });
                        break;
                    case "StealthyChallenge":
                    case "SpeedyChallenge":
                    case "GrappleChallenge":
                        x.AddMeta(this, new string[] { "challenge" });
                        break;
                    case "Farm": //farm isnt a fieldtrip because its just a dummy sceneobject for the option in the menu, and isnt actually usable as a field trip
                        x.AddMeta(this, new string[0]);
                        break;
                    default:
                        MTM101BaldiDevAPI.Log.LogWarning("Unknown root SceneObject: " + x.name + ". Unable to add meta!");
                        break;
                }
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
                string sf2Path = Path.Combine(Application.temporaryCachePath, "gm.sf2");
                File.WriteAllBytes(sf2Path, stream.ToByteArray());
                MidiPlayerGlobal.MPTK_LoadLiveSF("file://" + sf2Path);
                StartCoroutine(WaitForSoundfontLoad(sf2Path));
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

        class TestOptionsCat : CustomOptionsCategory
        {
            public override void Build()
            {
                CreateText("test", "Test!", Vector3.zero, BaldiFonts.ComicSans36, TextAlignmentOptions.Center, new Vector2(200f, 70f), Color.black);
                AddTooltip(CreateTextButton(() =>
                {
                    MTM101BaldiDevAPI.Log.LogInfo("This is a test!");
                }, "testButton", "TestButton!", Vector3.down * 140f, BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(150f, 70f), Color.black), "prints \"This is a test!\"");
                StandardMenuButton greenSquare = CreateButton(() =>
                {
                    MTM101BaldiDevAPI.Log.LogInfo("This is another test!");
                }, null, "TestImage", Vector3.left * 60f, Vector2.one * 30f);
                greenSquare.image.color = Color.green;
                AddTooltip(greenSquare, "Prints \"This is another test!\" and is also green.");
                AdjustmentBars bar = CreateBars(() => { }, "Test", Vector3.down * 70f, 10);
                AddTooltip(bar, "This is a bar with a tooltip!");
                AddTooltip(CreateBars(() => { }, "Test2", Vector3.down * 105f + (Vector3.left * 60f), 15), "This is a longer bar with a tooltip!");
                AddTooltipRegion("TestRegion", Vector3.up * 50f, Vector2.one * 100f, "Test tooltip region!", true);
                MenuToggle tog = CreateToggle("TestToggle", "Test Test 2!", false, Vector3.down * 40f, 300f);
                AddTooltip(tog, "This is a toggle!");
                AddTooltip(CreateApplyButton(() => { MTM101BaldiDevAPI.Log.LogInfo("Applied!"); }), "Prints \"Applied!\" to the console.");
            }
        }

        void OnMen(OptionsMenu __instance, CustomOptionsHandler handler)
        {
            handler.AddCategory<TestOptionsCat>("Test Menu");
        }
#endif

        /// <summary>
        /// The internal subobject used for converting objects into prefabs. 
        /// Use this with GameObject.Instantiate as the parent transform to avoid manually having to clean stuff up.
        /// </summary>
        internal static GameObject PrefabSubObject;

        /// <summary>
        /// The internal transform used for object to prefab conversion.
        /// Use this with GameObject.Instantiate to prevent awake scripts from activating.
        /// </summary>
        public static Transform prefabTransform => PrefabSubObject.transform;

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
            // TODO: create TemplateNPC from scratch and stop duplicating beans
            Beans beansToCopy = Resources.FindObjectsOfTypeAll<Beans>().First();
            beansToCopy.gameObject.SetActive(false);
            NPC templateNpc = GameObject.Instantiate<NPC>(beansToCopy);
            beansToCopy.gameObject.SetActive(true);
            templateNpc.GetComponent<Entity>().SetActive(false); //disable the entity
            templateNpc.name = "TemplateNPC";
            GameObject templateObject = templateNpc.gameObject;
            GameObject.DestroyImmediate(templateObject.GetComponent<Beans>());
            GameObject.DestroyImmediate(templateObject.GetComponent<Animator>());
            templateObject.layer = LayerMask.NameToLayer("NPCs");
            templateObject.ConvertToPrefab(false);
            AssetMan.Add<GameObject>("TemplateNPC", templateObject);
            MTM101BaldAPI.Registers.Buttons.ButtonColorManager.InitializeButtonColors();
            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            AssetMan.Add<Sprite>("MenuArrowLeft",allSprites.First(x => x.name == "MenuArrowSheet_2"));
            AssetMan.Add<Sprite>("MenuArrowLeftHighlight", allSprites.First(x => x.name == "MenuArrowSheet_0"));
            AssetMan.Add<Sprite>("MenuArrowRight", allSprites.Where(x => x.name == "MenuArrowSheet_3").First());
            AssetMan.Add<Sprite>("MenuArrowRightHighlight", allSprites.First(x => x.name == "MenuArrowSheet_1"));
            AssetMan.Add<Sprite>("Bar", allSprites.First(x => x.name == "MenuBarSheet_0"));
            AssetMan.Add<Sprite>("BarTransparent", allSprites.First(x => x.name == "MenuBarSheet_1"));
            AssetMan.Add<Sprite>("CheckBox", allSprites.First(x => x.name == "CheckBox"));
            AssetMan.Add<Sprite>("Check", allSprites.First(x => x.name == "YCTP_IndicatorsSheet_0"));
            AssetMan.Add<Material>("tileStandard", Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "TileBase"));
            AssetMan.AddFromResources<Shader>();
            questionMarkSprites = allSprites.Where(x => x.texture.name == "QMarkSheet").ToArray();
            SoundObject[] allSoundObjects = Resources.FindObjectsOfTypeAll<SoundObject>();
            AssetMan.Add<SoundObject>("Xylophone", allSoundObjects.First(x => x.name == "NotebookCollect"));
            AssetMan.Add<SoundObject>("Explosion", allSoundObjects.First(x => x.name == "GlassBreak"));
            AssetMan.AddFromResources<TMPro.TMP_FontAsset>();
            AssetMan.AddFromResources<Material>();
            AssetMan.Add<SoundObject>("Silence", allSoundObjects.First(x => x.name == "Silence"));
            AssetMan.Add<AudioClip>("ErrorSound", Resources.FindObjectsOfTypeAll<AudioClip>().First(x => x.name == "Activity_Incorrect"));
        }

        internal void ConvertAllLevelObjects()
        {
            SceneObject[] sceneObjects = Resources.FindObjectsOfTypeAll<SceneObject>();
            Dictionary<LevelObject, CustomLevelObject> oldToNewMapping = new Dictionary<LevelObject, CustomLevelObject>();
            foreach (SceneObject objct in sceneObjects)
            {
                if (objct.levelObject == null) continue;
                CustomLevelObject customizedObject = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(objct.levelObject);
                customizedObject.name = objct.levelObject.name;
                oldToNewMapping.Add(objct.levelObject, customizedObject);
                objct.levelObject = customizedObject;
                objct.levelObject.MarkAsNeverUnload();
                objct.MarkAsNeverUnload();
            }
            foreach (KeyValuePair<LevelObject,CustomLevelObject> kvp in oldToNewMapping)
            {
                kvp.Value.previousLevels = new LevelObject[kvp.Key.previousLevels.Length];
                // remap the old previousLevels to the new one.
                for (int i = 0; i < kvp.Key.previousLevels.Length; i++)
                {
                    kvp.Value.previousLevels[i] = oldToNewMapping[kvp.Key.previousLevels[i]];
                }
            }
            // destroy the old objects (do this in a seperate loop so we can preserve the keys until the very end
            foreach (KeyValuePair<LevelObject, CustomLevelObject> kvp in oldToNewMapping)
            {
                Destroy(kvp.Key);
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
                    Singleton<ModdedFileManager>.Instance.LoadGameWithIndex(myPath, Singleton<ModdedFileManager>.Instance.saveIndex, true);
                }
                Singleton<ModdedFileManager>.Instance.UpdateCurrentPartialSave();
            });

            useOldAudioLoad = Config.Bind("Technical",
                "Use Old Audio Loading Method",
                false,
                "Whether or not the old legacy method of loading audio should be used. Do not turn on as it is not needed anymore.");

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

            ConfigEntry<bool> alwaysModdedSave = Config.Bind("General",
                "Always Use Modded Save System",
                false,
                "If true, the modded save system will always be used, even if there are no mods that enable it.");

            ConfigEntry<bool> useErrorReporter = Config.Bind("General",
                "Visible Exceptions",
                true,
                "If true, any exceptions that occur during gameplay will be flashed onto the screen.");

            attemptOnline = Config.Bind("General",
                "Connect to Gamebanana",
                true,
                "If true, the mod will attempt to connect to Gamebanana to get the latest version of the API.");

            ignoringTagDisplays = Config.Bind("Technical",
                "Ignore Custom Tag Displays",
                false,
                "If true, mod save tags will always be shown as if the mod wasn't installed. It is suggested to leave this off unless you are debugging.");

            if (alwaysModdedSave.Value)
            {
                saveHandler = SavedGameDataHandler.Modded;
            }

            if (useErrorReporter.Value)
            {
                BepInEx.Logging.Logger.Listeners.Add(new APIErrorListener());
            }

            harmony.PatchAllConditionals();

            Log = base.Logger;
            if (attemptOnline.Value)
            {
                StartCoroutine(GetCurrentGamebananaVersion());
            }
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
            text.text += "\nAPI " + MTM101BaldiDevAPI.VersionNumber;
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
            Transform reminder = __instance.transform.Find("Reminder");
            TMPro.TMP_Text text = reminder.gameObject.GetComponent<TMPro.TMP_Text>();
            text.gameObject.SetActive(true); // so the pre-releases don't hide the version number
            text.text = "Modding API " + MTM101BaldiDevAPI.VersionNumber;
            text.gameObject.transform.position += new Vector3(-11f,0f, 0f);
            if (!MTM101BaldiDevAPI.Instance.attemptOnline.Value)
            {
                return;
            }
            text.raycastTarget = true;
            StandardMenuButton button = text.gameObject.ConvertToButton<StandardMenuButton>();
            button.underlineOnHigh = true;
            if (MTM101BaldiDevAPI.Instance.newestGBVersion != "unknown")
            {
                if (new Version(MTM101BaldiDevAPI.Instance.newestGBVersion) > MTM101BaldiDevAPI.Version)
                {
                    text.text += "\nOutdated!\n(Latest is " + MTM101BaldiDevAPI.Instance.newestGBVersion + ")";
                }
                else if (MTM101BaldiDevAPI.Version > new Version(MTM101BaldiDevAPI.Instance.newestGBVersion))
                {
                    text.text += "\nUnreleased!";
                }
            }
            else
            {
                text.text += "Unable to connect!";
            }
            button.OnPress.AddListener(() => { Application.OpenURL("https://gamebanana.com/mods/383711"); });
        }
    }
}
