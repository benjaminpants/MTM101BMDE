using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI
{
    public class ModLoadingScreenManager : MonoBehaviour
    {
        internal Sprite barActive = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("Bar");
        internal Sprite barInactive = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("BarTransparent");
        LoadingBar modLoadingBar;
        LoadingBar apiLoadingBar;
        TextMeshProUGUI apiLoadText;
        TextMeshProUGUI modLoadText;
        TextMeshProUGUI modIdText;

        public static bool doneLoading = false;

        void SetBarValueRaw(LoadingBar bar, int amount)
        {
            for (int i = 0; i < bar.bars.Length; i++)
            {
                bar.bars[i].sprite = (i < amount) ? barActive : barInactive;
            }
        }

        void SetBarValue(LoadingBar bar, float percent)
        {
            SetBarValueRaw(bar, Mathf.FloorToInt(bar.count * percent));
        }


        struct LoadingBar
        {
            public Image[] bars;
            public int count;
        }

        void CreateRandomQMark()
        {
            Sprite selectedSprite = MTM101BaldiDevAPI.Instance.questionMarkSprites[UnityEngine.Random.Range(0, MTM101BaldiDevAPI.Instance.questionMarkSprites.Length)];
            UIHelpers.CreateImage(selectedSprite, this.transform, new Vector2(UnityEngine.Random.Range(-230f, 230f), UnityEngine.Random.Range(-170f, 170f)), false).transform.SetAsFirstSibling();
        }

        void Start()
        {
            for (int i = 0; i < 8; i++)
            {
                CreateRandomQMark();
            }
            apiLoadingBar = CreateBar(new Vector2(24f, 164f), 55);
            modLoadingBar = CreateBar(new Vector2(24f, 164f + 80f), 55);
            TextMeshProUGUI loadingText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans36, "Loading...", this.transform, new Vector3(24f + (54f * 4f), 98f), true);
            loadingText.color = Color.black;
            loadingText.alignment = TextAlignmentOptions.Center;

            apiLoadText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, "", this.transform, new Vector3(24f + (54f * 4f), 164f + 28f), true);
            apiLoadText.color = Color.black;
            apiLoadText.fontStyle = FontStyles.Bold;
            apiLoadText.rectTransform.sizeDelta = new Vector2(480f, 32f);
            apiLoadText.alignment = TextAlignmentOptions.Center;

            modLoadText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, "", this.transform, new Vector3(24f + (54f * 4f), 164f + 28f + 80f), true);
            modLoadText.color = Color.black;
            modLoadText.fontStyle = FontStyles.Bold;
            modLoadText.rectTransform.sizeDelta = new Vector2(480f, 32f);
            modLoadText.alignment = TextAlignmentOptions.Center;

            modIdText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "", this.transform, new Vector3(24f + (54f * 4f), 164f + 28f + 80f + 28f), true);
            modIdText.color = Color.black;
            modIdText.rectTransform.sizeDelta = new Vector2(480f, 32f);
            modIdText.alignment = TextAlignmentOptions.Center;

            BeginLoadProcess();
        }

        void BeginLoadProcess()
        {
            LoadingEvents.SortLoadingEvents();
            StartCoroutine(LoadEnumerator());
        }

        void LoadingEnded()
        {
            Singleton<GlobalCam>.Instance.Transition(UiTransition.Dither, 0.01666667f * 2.5f);
            if (GameObject.Find("NameList")) { GameObject.Find("NameList").GetComponent<AudioSource>().enabled = true; }
            CursorController.Instance.DisableClick(false);
            Destroy(this.gameObject);
        }

        IEnumerator LoadEnumerator()
        {
            yield return BeginLoadEnumerator(MainLoad(), apiLoadingBar, apiLoadText);
            apiLoadText.text = "Done!";
            modIdText.text = "";
            modLoadText.text = "";
            doneLoading = true;
            LoadingEnded();
            yield break;
        }

        IEnumerator BeginLoadEnumerator(IEnumerator numerator, LoadingBar barToAdjust, TMP_Text textToChange)
        {
            if (!numerator.MoveNext())
            {
                throw new Exception("IEnumerator provided to loading ended before expected time!");
            }
            int maxSteps = ((int)numerator.Current); // loading method calculated max, yeah.
            if (!numerator.MoveNext())
            {
                throw new Exception("IEnumerator provided to loading ended before expected time!");
            }
            textToChange.text = (string)numerator.Current;
            int totalSteps = 0;
            SetBarValue(barToAdjust, 0f);
            yield return null; // not having this here caused an issue where if something only took a brief moment the old text would carry over
            while (numerator.MoveNext())
            {
                if (numerator.Current.GetType() != typeof(string))
                {
                    // in the rare case we genuinely can not predict what will happen next (LootTables...)
                    if (numerator.Current.GetType() == typeof(int))
                    {
                        maxSteps += (int)numerator.Current;
                    }
                    else
                    {
                        yield return numerator.Current;
                    }
                }
                else
                {
                    textToChange.text = (string)numerator.Current;
                    totalSteps++;
                    CreateRandomQMark();
                    SetBarValue(barToAdjust, (float)totalSteps / (float)maxSteps);
                }
                yield return null;
            }
            SetBarValue(barToAdjust, 1f); //incase it returns early, still set the bar to full
            yield break;
        }

        static readonly FieldInfo _potentialItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "potentialItems");
        static readonly FieldInfo _guaranteedItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "guaranteedItems");

        IEnumerator ModifyFieldtripLoot(FieldTripObject trip)
        {
            yield return GeneratorManagement.fieldtripLootChanges.Count;
            yield return "Loading...";
            FieldTripBaseRoomFunction roomFunction = trip.tripHub.room.roomFunctionContainer.GetComponent<FieldTripBaseRoomFunction>();
            FieldTripLoot tripLoot = new FieldTripLoot();
            tripLoot.potentialItems = ((WeightedItemObject[])_potentialItems.GetValue(roomFunction)).ToList();
            tripLoot.guaranteedItems = ((List<ItemObject>)_guaranteedItems.GetValue(roomFunction)).ToList();
            foreach (KeyValuePair<BaseUnityPlugin, Action<FieldTrips, FieldTripLoot>> kvp in GeneratorManagement.fieldtripLootChanges)
            {
                yield return kvp.Key;
                kvp.Value.Invoke(trip.trip, tripLoot);
            }
            if (GeneratorManagement.fieldtripLootChanges.Count > 0)
            {
                trip.MarkAsNeverUnload();
                trip.tripHub.room.MarkAsNeverUnload();
                trip.tripHub.room.roomFunctionContainer.MarkAsNeverUnload();
            }
            _potentialItems.SetValue(roomFunction, tripLoot.potentialItems.ToArray());
            _guaranteedItems.SetValue(roomFunction, tripLoot.guaranteedItems.ToList());
        }


        IEnumerator MainLoad()
        {
            SceneObject[] objs = Resources.FindObjectsOfTypeAll<SceneObject>().Where(x =>
            {
                if (x.levelObject != null)
                {
                    return true;
                }
                if (x.randomizedLevelObject != null)
                {
                    return x.randomizedLevelObject.Length > 0;
                }
                return false;
            }).ToArray();
            List<SceneObject> objList = new List<SceneObject>(objs);
            objList.Sort((a,b) => (b.manager is MainGameManager).CompareTo((a.manager is MainGameManager)));
            objs = objList.ToArray();
            FieldTripObject[] foundTrips = Resources.FindObjectsOfTypeAll<FieldTripObject>().Where(x => x.tripHub != null).ToArray(); // ignore junk
            yield return (5 + objs.Length) + LoadingEvents.LoadingEventsPost.Count + LoadingEvents.LoadingEventsPre.Count + LoadingEvents.LoadingEventsStart.Count + foundTrips.Length + LoadingEvents.LoadingEventsFinal.Count;
            for (int i = 0; i < LoadingEvents.LoadingEventsStart.Count; i++)
            {
                LoadingEvents.LoadingEvent load = LoadingEvents.LoadingEventsStart[i];
                modIdText.text = load.info.Metadata.GUID;
                yield return "Loading Mod Assets... (" + i + "/" + LoadingEvents.LoadingEventsStart.Count + ")";
                yield return BeginLoadEnumerator(load.loadingNumerator, modLoadingBar, modLoadText);
            }
            modLoadText.text = "";
            modIdText.text = "";
            yield return "Converting LevelObjects to CustomLevelObjects...";
            MTM101BaldiDevAPI.Instance.ConvertAllLevelObjects();
            for (int i = 0; i < LoadingEvents.LoadingEventsPre.Count; i++)
            {
                LoadingEvents.LoadingEvent load = LoadingEvents.LoadingEventsPre[i];
                modIdText.text = load.info.Metadata.GUID;
                yield return "Invoking Mod Asset Pre-Loading... (" + i + "/" + LoadingEvents.LoadingEventsPre.Count + ")";
                yield return BeginLoadEnumerator(load.loadingNumerator, modLoadingBar, modLoadText);
            }
            modLoadText.text = "";
            modIdText.text = "";
            MTM101BaldiDevAPI.tooLateForGeneratorBasedFeatures = true;
            foreach (SceneObject obj in objs)
            {
                yield return "Changing " + obj.levelTitle + "...";
                if (obj.levelObject != null)
                {
                    if (!(obj.levelObject is CustomLevelObject))
                    {
                        MTM101BaldiDevAPI.Log.LogWarning(String.Format("Can't invoke SceneObject({0})({2}) Generation Changes for {1}! Not a CustomLevelObject!", obj.levelTitle, obj.levelObject.ToString(), obj.name));
                        continue;
                    }
                }
                if (obj.randomizedLevelObject != null)
                {
                    for (int i = 0; i < obj.randomizedLevelObject.Length; i++)
                    {
                        if (!(obj.randomizedLevelObject[i].selection is CustomLevelObject))
                        {
                            MTM101BaldiDevAPI.Log.LogWarning(String.Format("Can't invoke SceneObject({0})({2}) Generation Changes for {1}! Not a CustomLevelObject!", obj.levelTitle, obj.randomizedLevelObject[i].selection.ToString(), obj.name));
                            continue;
                        }
                    }
                }
                MTM101BaldiDevAPI.Log.LogInfo(String.Format("Invoking SceneObject({0})({1}) Generation Changes!", obj.levelTitle, obj.name));
                GeneratorManagement.Invoke(obj.levelTitle, obj.levelNo, obj);
            }
            yield return "Changing modded SceneObjects...";
            GeneratorManagement.queuedModdedScenes.Sort((a, b) => (b.manager is MainGameManager).CompareTo((a.manager is MainGameManager)));
            while (GeneratorManagement.queuedModdedScenes.Count > 0)
            {
                SceneObject obj = GeneratorManagement.queuedModdedScenes[0];
                GeneratorManagement.queuedModdedScenes.RemoveAt(0);
                MTM101BaldiDevAPI.Log.LogInfo(String.Format("Invoking SceneObject({0})({1}) Generation Changes!", obj.levelTitle, obj.name));
                GeneratorManagement.Invoke(obj.levelTitle, obj.levelNo, obj);
            }

            foreach (FieldTripObject trip in foundTrips)
            {
                yield return "Changing " + trip.name + " loot...";
                yield return BeginLoadEnumerator(ModifyFieldtripLoot(trip), modLoadingBar, modLoadText);
            }
            modLoadText.text = "";
            modIdText.text = "";
            yield return "Adding MIDIs...";
            foreach (KeyValuePair<string, byte[]> kvp in AssetLoader.MidisToBeAdded)
            {
                AssetLoader.MidiFromBytes(kvp.Key, kvp.Value);
            }
            AssetLoader.MidisToBeAdded = null;
            for (int i = 0; i < LoadingEvents.LoadingEventsPost.Count; i++)
            {
                LoadingEvents.LoadingEvent load = LoadingEvents.LoadingEventsPost[i];
                modIdText.text = load.info.Metadata.GUID;
                yield return "Invoking Mod Asset Post-Loading... (" + i + "/" + LoadingEvents.LoadingEventsPost.Count + ")";
                yield return BeginLoadEnumerator(load.loadingNumerator, modLoadingBar, modLoadText);
            }
            yield return "Reloading Localization...";
            Singleton<LocalizationManager>.Instance.ReflectionInvoke("Start", null);
            yield return "Reloading highscores...";
            if (MTM101BaldiDevAPI.highscoreHandler == SavedGameDataHandler.Unset)
            {
                if (ModdedHighscoreManager.tagList.Count > 0)
                {
                    MTM101BaldiDevAPI.highscoreHandler = SavedGameDataHandler.Modded;
                }
                else
                {
                    MTM101BaldiDevAPI.highscoreHandler = SavedGameDataHandler.Vanilla;
                }
            }
            Singleton<HighScoreManager>.Instance.Load(); //reload
            for (int i = 0; i < LoadingEvents.LoadingEventsFinal.Count; i++)
            {
                LoadingEvents.LoadingEvent load = LoadingEvents.LoadingEventsFinal[i];
                modIdText.text = load.info.Metadata.GUID;
                yield return "Invoking Mod Asset Finalizing... (" + i + "/" + LoadingEvents.LoadingEventsFinal.Count + ")";
                yield return BeginLoadEnumerator(load.loadingNumerator, modLoadingBar, modLoadText);
            }
            yield break;
        }

        LoadingBar CreateBar(Vector2 position, int length)
        {
            List<Image> sprites = new List<Image>();
            for (int i = 0; i < length; i++)
            {
                sprites.Add(UIHelpers.CreateImage(barInactive, this.transform, position + ((Vector2.right * 8) * i), true));
            }
            return new LoadingBar()
            {
                bars = sprites.ToArray(),
                count = sprites.Count
            };
        }
    }
}
