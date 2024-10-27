using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using MTM101BaldAPI.UI;

namespace MTM101BaldAPI.OptionsAPI
{
    public static class CustomOptionsCore
    {
        /// <summary>
        /// This gets called when the Option's Menu gets created. This function may be called multiple times.
        /// </summary>
        public static event Action<OptionsMenu, CustomOptionsHandler> OnMenuInitialize;

        /// <summary>
        /// This gets called whenever the options menu gets closed, so you can save your stuff.
        /// </summary>
        public static event Action<OptionsMenu, CustomOptionsHandler> OnMenuClose;

        public static void CallOnMenuInitialize(OptionsMenu m, CustomOptionsHandler handler)
        {
            if (OnMenuInitialize != null)
            {
                OnMenuInitialize(m, handler);
            }
        }

        public static void CallOnMenuClose(OptionsMenu m, CustomOptionsHandler handler)
        {
            if (OnMenuClose != null)
            {
                OnMenuClose(m, handler);
            }
        }
    }

    /// <summary>
    /// A class to be inherited from that has all the options menu building functionality.
    /// </summary>
    public abstract class CustomOptionsCategory : MonoBehaviour
    {
        public abstract void Build();

        public TooltipController tooltipController;

        public Transform toolTipHotspot;

        protected Sprite bar => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("Bar");
        protected Sprite barFaded => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("BarTransparent");
        protected Sprite menuArrowLeft => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowLeft");
        protected Sprite menuArrowLeftHighlight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowLeftHighlight");
        protected Sprite menuArrowRight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowRight");
        protected Sprite menuArrowRightHighlight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowRightHighlight");
        protected Sprite checkBox => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("CheckBox");
        protected Sprite checkMark => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("Check");
        protected SoundObject silence => MTM101BaldiDevAPI.AssetMan.Get<SoundObject>("Silence");


        static FieldInfo _textBox = AccessTools.Field(typeof(TextLocalizer), "textBox");
        protected TextMeshProUGUI CreateText(string name, string text, Vector3 position, BaldiFonts font, TextAlignmentOptions alignment, Vector2 sizeDelta, Color color, bool createLocalizer = true)
        {
            TextMeshProUGUI resultText = UIHelpers.CreateText<TextMeshProUGUI>(font, text, transform, position, false);
            resultText.alignment = alignment;
            resultText.rectTransform.sizeDelta = sizeDelta;
            resultText.color = color;
            resultText.name = name;
            if (createLocalizer)
            {
                TextLocalizer textLocal = resultText.gameObject.AddComponent<TextLocalizer>();
                _textBox.SetValue(textLocal, resultText); //why the fuck doesn't awake work
                textLocal.key = text;
                textLocal.GetLocalizedText(text);
            }
            else
            {
                resultText.text = text;
            }
            return resultText;
        }

        protected StandardMenuButton CreateTextButton(UnityAction action, string name, string text, Vector3 position, BaldiFonts font, TextAlignmentOptions alignment, Vector2 sizeDelta, Color color)
        {
            TextMeshProUGUI textTransform = CreateText(name, text, position, font, alignment, sizeDelta, color);
            textTransform.raycastTarget = true;
            StandardMenuButton but = textTransform.gameObject.ConvertToButton<StandardMenuButton>(true);
            but.underlineOnHigh = true;
            but.OnPress.AddListener(action);
            return but;
        }

        protected Image CreateImage(Sprite background, string name, Vector3 position, Vector2? sizeDelta = null)
        {
            Image newImage = new GameObject(name).AddComponent<Image>();
            newImage.transform.SetParent(transform, false);
            newImage.transform.localScale = Vector3.one; //i hate everything and everyone
            newImage.name = name;
            newImage.transform.localPosition = position;
            newImage.gameObject.layer = LayerMask.NameToLayer("UI");
            if (sizeDelta != null)
            {
                newImage.rectTransform.sizeDelta = sizeDelta.Value;
            }
            else
            {
                newImage.rectTransform.sizeDelta = background.rect.size;
            }
            newImage.sprite = background;
            return newImage;
        }

        protected StandardMenuButton CreateButton(UnityAction action, Sprite background, string name, Vector3 position, Vector2? sizeDelta = null)
        {
            Image image = CreateImage(background, name, position, sizeDelta);
            image.raycastTarget = true;
            StandardMenuButton menButton = image.gameObject.ConvertToButton<StandardMenuButton>();
            menButton.OnPress.AddListener(action);
            return menButton;
        }

        protected StandardMenuButton CreateButton(UnityAction action, Sprite unhighlighted, Sprite highlighted, string name, Vector3 position, Vector2? sizeDelta = null)
        {
            Image image = CreateImage(unhighlighted, name, position, sizeDelta);
            image.raycastTarget = true;
            StandardMenuButton menButton = image.gameObject.ConvertToButton<StandardMenuButton>();
            menButton.unhighlightedSprite = unhighlighted;
            menButton.highlightedSprite = highlighted;
            menButton.swapOnHigh = true;
            menButton.OnPress.AddListener(action);
            return menButton;
        }

        static FieldInfo _bars = AccessTools.Field(typeof(AdjustmentBars), "bars");
        static FieldInfo _highlighted = AccessTools.Field(typeof(AdjustmentBars), "highlighted");
        static FieldInfo _unhighlighted = AccessTools.Field(typeof(AdjustmentBars), "unhighlighted");

        protected AdjustmentBars CreateBars(UnityAction onChanged, string name, Vector3 position, int count)
        {
            GameObject barObject = new GameObject(name, typeof(RectTransform));
            barObject.SetActive(false); // disable so that the awake function doesn't active prematurely
            barObject.layer = LayerMask.NameToLayer("UI");
            barObject.transform.SetParent(transform, false);
            AdjustmentBars adjustBar = barObject.AddComponent<AdjustmentBars>();
            StandardMenuButton butLeft = CreateButton(() => { adjustBar.Adjust(-1); }, menuArrowLeft, menuArrowLeftHighlight, "LeftCategoryButton", Vector3.zero);
            butLeft.transform.SetParent(barObject.transform, false);
            butLeft.transform.localScale = Vector3.one;
            Vector3 offset = Vector3.right * ((butLeft.unhighlightedSprite.rect.size.x / 2f) + 6f);
            barObject.SetActive(true);
            GameObject barsFolderObject = new GameObject("Bars");
            barsFolderObject.transform.SetParent(barObject.transform, false);
            barsFolderObject.transform.localScale = Vector3.one;
            barsFolderObject.layer = LayerMask.NameToLayer("UI");
            Image[] barImages = new Image[count];
            for (int i = 0; i < count; i++)
            {
                Image image = CreateImage(barFaded, "Bar" + i, Vector3.zero);
                image.transform.SetParent(barsFolderObject.transform);
                image.transform.localScale = Vector3.one;
                image.transform.localPosition = Vector3.right * (i * 10f) + offset;
                image.raycastTarget = false;
                barImages[i] = image;
            }
            StandardMenuButton butRight = CreateButton(() => { adjustBar.Adjust(1); }, menuArrowRight, menuArrowRightHighlight, "RightCategoryButton", (Vector3.right * ((butLeft.unhighlightedSprite.rect.size.x + 2f) + (count * 10f))));
            butRight.transform.SetParent(barObject.transform, false);
            butRight.transform.localScale = Vector3.one;
            _bars.SetValue(adjustBar, barImages);
            _highlighted.SetValue(adjustBar, bar);
            _unhighlighted.SetValue(adjustBar, barFaded);
            adjustBar.onValueChanged = new UnityEvent();
            adjustBar.onValueChanged.AddListener(onChanged);
            barObject.transform.localPosition = position;
            return adjustBar;
        }

        protected void SetupTooltipHotspots()
        {
            toolTipHotspot = new GameObject("TooltipHotspots").transform;
            toolTipHotspot.SetParent(transform, false);
            toolTipHotspot.localScale = Vector3.one;
            toolTipHotspot.localPosition = Vector3.zero;
            toolTipHotspot.SetAsFirstSibling();
        }

        protected void AddTooltip(StandardMenuButton button, string tooltip)
        {
            button.eventOnHigh = true;
            button.OnHighlight.AddListener(() => { tooltipController.UpdateTooltip(tooltip); });
            button.OffHighlight.AddListener(() => { tooltipController.CloseTooltip(); });
        }

        protected void AddTooltip(AdjustmentBars bar, string tooltip)
        {
            AddTooltip(bar.transform.Find("LeftCategoryButton").GetComponent<StandardMenuButton>(), tooltip);
            AddTooltip(bar.transform.Find("RightCategoryButton").GetComponent<StandardMenuButton>(), tooltip);
            float width = Mathf.Abs(bar.transform.Find("LeftCategoryButton").position.x - bar.transform.Find("RightCategoryButton").position.x) / 3f;
            AddTooltipRegion(bar.name + "Region", bar.transform.localPosition + (Vector3.right * (width / 2f)), new Vector2(width, 32f), tooltip, false);
        }

        protected StandardMenuButton AddTooltipRegion(string name, Vector3 position, Vector2 size, string tooltip, bool visible = false)
        {
            if (toolTipHotspot == null)
            {
                SetupTooltipHotspots();
            }
            GameObject obj = new GameObject(name, typeof(Image));
            obj.transform.SetParent(toolTipHotspot, false);
            Image image = obj.GetComponent<Image>();
            image.color = new Color(1f, 1f, 0f, visible ? 1f : 0f);
            obj.transform.localPosition = position;
            image.rectTransform.sizeDelta = size;
            StandardMenuButton menButton = obj.ConvertToButton<StandardMenuButton>();
            menButton.audConfirmOverride = silence;
            AddTooltip(menButton, tooltip);
            return menButton;
        }

        static FieldInfo _val = AccessTools.Field(typeof(MenuToggle), "val");
        static FieldInfo _checkmark = AccessTools.Field(typeof(MenuToggle), "checkmark");
        static FieldInfo _disableCover = AccessTools.Field(typeof(MenuToggle), "disableCover");
        static FieldInfo _hotspot = AccessTools.Field(typeof(MenuToggle), "hotspot");
        protected MenuToggle CreateToggle(string name, string text, bool value, Vector3 position, float width)
        {
            Vector2 size = new Vector2(width, 32f);
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(transform, false);
            obj.transform.localScale = Vector3.one;
            obj.layer = LayerMask.NameToLayer("UI");
            obj.GetComponent<RectTransform>().sizeDelta = size + new Vector2(8f,0f);
            TextMeshProUGUI textObj = CreateText("ToggleText", text, Vector3.zero, BaldiFonts.ComicSans24, TextAlignmentOptions.TopRight, size, Color.black, true);
            textObj.transform.SetParent(obj.transform);
            textObj.rectTransform.pivot = new Vector2(1f, 0.5f);
            textObj.transform.localScale = Vector3.one;
            textObj.transform.localPosition = new Vector3(-8f,0f,0f);
            MenuToggle toggle = obj.AddComponent<MenuToggle>();
            Image box = CreateImage(checkBox, "Box", Vector3.zero);
            box.transform.SetParent(obj.transform);
            box.transform.localScale = Vector3.one;
            box.transform.localPosition = new Vector3(8f, 0f, 0f);
            box.rectTransform.pivot = new Vector2(0f, 0.5f);
            Image check = CreateImage(checkMark, "Check", Vector3.zero, Vector2.one * 32f);
            check.transform.SetParent(box.transform);
            check.transform.localScale = Vector3.one;
            check.transform.localPosition = new Vector3(21f, 6f, 0f);
            StandardMenuButton hotSpot = CreateButton(() => { toggle.Toggle(); }, null, "HotSpot", Vector3.zero, new Vector2(size.x / 1.25f, size.y));
            hotSpot.image.color = Color.clear;
            hotSpot.transform.SetParent(obj.transform);
            hotSpot.transform.localScale = Vector3.one;
            hotSpot.text = textObj;
            hotSpot.underlineOnHigh = true;
            hotSpot.transform.localPosition += Vector3.left * ((width / 1.25f) / 4f);

            // finally set up the buttom
            _val.SetValue(toggle, value);
            _checkmark.SetValue(toggle, check.gameObject);
            _hotspot.SetValue(toggle, hotSpot.gameObject);
            check.gameObject.SetActive(value);


            obj.transform.localPosition = position;

            return toggle;
        }

        protected StandardMenuButton CreateApplyButton(UnityAction onApply)
        {
            StandardMenuButton menBut = CreateButton(onApply, null, "ApplyButton", new Vector3(136f, -160f, 0f), new Vector2(100f, 32f));
            TextMeshProUGUI text = CreateText("ApplyText", "Opt_Apply", Vector3.zero, BaldiFonts.ComicSans24, TextAlignmentOptions.TopRight, new Vector2(96f, 32f), Color.black);
            text.transform.SetParent(menBut.transform, false);
            text.transform.localScale = Vector3.one;
            menBut.text = text;
            menBut.underlineOnHigh = true;
            menBut.transform.localScale = Vector3.one; // what the fuck i don't even CHANGE THE BUTTONS PARENT AND ITS SCALE STILL GETS COMPLETELY ***FUCKED.***
            return menBut;
        }

        protected void AddTooltip(MenuToggle toggle, string tooltip)
        {
            AddTooltip(((GameObject)_hotspot.GetValue(toggle)).GetComponent<StandardMenuButton>(), tooltip);
        }
    }

    public class CustomOptionsHandler : MonoBehaviour
    {

        public struct OptionsCategory
        {
            public string localizationName;
            public GameObject gameObject;

            public OptionsCategory(string name, GameObject page)
            {
                localizationName = name;
                gameObject = page;
            }
        }

        private List<OptionsCategory> categories = new List<OptionsCategory>();

        public OptionsCategory[] Categories => categories.ToArray();

        static FieldInfo _categoryKeys = AccessTools.Field(typeof(OptionsMenu), "categoryKeys");
        static FieldInfo _categories = AccessTools.Field(typeof(OptionsMenu), "categories");
        static FieldInfo _currentCategory = AccessTools.Field(typeof(OptionsMenu), "currentCategory");

        public TooltipController tooltipController;
        public OptionsMenu optionsMenu;
        int indexToSetTransformBehind;


        void Awake()
        {
            optionsMenu = GetComponent<OptionsMenu>();
            tooltipController = GetComponent<TooltipController>();
            indexToSetTransformBehind = optionsMenu.transform.Find("TooltipBase").GetSiblingIndex();
            LoadCategories();
        }

        void LoadCategories()
        {
            string[] keys = (string[])_categoryKeys.GetValue(optionsMenu);
            GameObject[] objects = (GameObject[])_categories.GetValue(optionsMenu);
            for (int i = 0; i < keys.Length; i++)
            {
                categories.Add(new OptionsCategory(keys[i], objects[i]));
            }
        }

        private OptionsCategory? currentWaitingCategory;

        void PrepareForRebuild()
        {
            currentWaitingCategory = categories[(int)_currentCategory.GetValue(optionsMenu)];
        }

        void RebuildPages()
        {
            _categoryKeys.SetValue(optionsMenu, categories.Select(x => x.localizationName).ToArray());
            _categories.SetValue(optionsMenu, categories.Select(x => x.gameObject).ToArray());
            if (!currentWaitingCategory.HasValue)
            {
                optionsMenu.ChangeCategory(0);
                return;
            }
            int newCategoryIndex = categories.IndexOf(currentWaitingCategory.Value);
            if (newCategoryIndex != -1)
            {
                _currentCategory.SetValue(optionsMenu, newCategoryIndex);
            }
            currentWaitingCategory = null;
            optionsMenu.ChangeCategory(0);
        }

        public CategoryType InsertCategory<CategoryType>(string pageName, int indexToInsert) where CategoryType : CustomOptionsCategory
        {
            PrepareForRebuild();
            GameObject newPage = new GameObject(pageName, typeof(RectTransform));
            newPage.SetActive(false);
            newPage.layer = LayerMask.NameToLayer("UI");
            newPage.transform.SetParent(optionsMenu.transform, false);
            categories.Insert(indexToInsert, new OptionsCategory(pageName, newPage));
            newPage.transform.SetSiblingIndex(indexToSetTransformBehind - 1);
            CategoryType page = newPage.AddComponent<CategoryType>();
            page.tooltipController = tooltipController;
            page.Build();
            RebuildPages();
            return page;
        }

        public CategoryType AddCategory<CategoryType>(string pageName) where CategoryType : CustomOptionsCategory
        {
            return InsertCategory<CategoryType>(pageName, categories.Count);
        }
    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Awake")]
    class OnOptionAwake
    {
        static void Postfix(OptionsMenu __instance)
        {
            if (__instance.gameObject.GetComponent<CustomOptionsHandler>()) return;
            CustomOptionsCore.CallOnMenuInitialize(__instance, __instance.gameObject.AddComponent<CustomOptionsHandler>());
        }
    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Close")]
    class OnOptionClose
    {
        static void Postfix(OptionsMenu __instance)
        {
            CustomOptionsCore.CallOnMenuClose(__instance, __instance.GetComponent<CustomOptionsHandler>());
        }
    }
}
