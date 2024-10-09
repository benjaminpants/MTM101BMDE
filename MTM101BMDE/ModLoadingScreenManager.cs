using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            while (numerator.MoveNext())
            {
                if (numerator.Current.GetType() != typeof(string))
                {
                    yield return numerator.Current;
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

        IEnumerator MainLoad()
        {
            SceneObject[] objs = Resources.FindObjectsOfTypeAll<SceneObject>().Where(x => x.levelObject != null).ToArray();
            yield return (2 + objs.Length) + LoadingEvents.LoadingEventsPost.Count + LoadingEvents.LoadingEventsPre.Count + LoadingEvents.LoadingEventsStart.Count;
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
            foreach (SceneObject obj in objs)
            {
                yield return "Changing " + obj.levelTitle + "...";
                if (!(obj.levelObject is CustomLevelObject))
                {
                    MTM101BaldiDevAPI.Log.LogWarning(String.Format("Can't invoke SceneObject({0})({2}) Generation Changes for {1}! Not a CustomLevelObject!", obj.levelTitle, obj.levelObject.ToString(), obj.levelNo.ToString()));
                    continue;
                }
                MTM101BaldiDevAPI.Log.LogInfo(String.Format("Invoking SceneObject({0})({2}) Generation Changes for {1}!", obj.levelTitle, obj.levelObject.ToString(), obj.levelNo.ToString()));
                obj.levelObject.shopItems = obj.shopItems;
                obj.levelObject.totalShopItems = obj.totalShopItems;
                obj.levelObject.mapPrice = obj.mapPrice;
                GeneratorManagement.Invoke(obj.levelTitle, obj.levelNo, (CustomLevelObject)obj.levelObject);
                obj.shopItems = obj.levelObject.shopItems;
                obj.totalShopItems = obj.levelObject.totalShopItems;
                obj.mapPrice = obj.levelObject.mapPrice;
            }
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
