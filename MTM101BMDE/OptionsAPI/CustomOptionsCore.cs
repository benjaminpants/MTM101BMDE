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

        protected Sprite bar => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("Bar");
        protected Sprite barFaded => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("BarTransparent");
        protected Sprite menuArrowLeft => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowLeft");
        protected Sprite menuArrowLeftHighlight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowLeftHighlight");
        protected Sprite menuArrowRight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowRight");
        protected Sprite menuArrowRightHighlight => MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowRightHighlight");

        protected TextMeshProUGUI CreateText(string name, string text, Vector3 position, BaldiFonts font, TextAlignmentOptions alignment, Vector2 sizeDelta, Color color)
        {
            TextMeshProUGUI resultText = UIHelpers.CreateText<TextMeshProUGUI>(font, text, transform, position, false);
            resultText.alignment = alignment;
            resultText.rectTransform.sizeDelta = sizeDelta;
            resultText.color = color;
            resultText.name = name;
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
            newImage.transform.SetParent(transform);
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

        public OptionsMenu optionsMenu;
        int indexToSetTransformBehind;


        void Awake()
        {
            optionsMenu = GetComponent<OptionsMenu>();
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
