﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MTM101BaldAPI.OptionsAPI
{
    public static class CustomOptionsCore
    {
        /// <summary>
        /// This gets called when the Option's Menu gets created. This function may be called multiple times.
        /// </summary>
        public static event Action<OptionsMenu> OnMenuInitialize;

        /// <summary>
        /// This gets called whenever the options menu gets closed, so you can save your stuff.
        /// </summary>
        public static event Action<OptionsMenu> OnMenuClose;

        public static void CallOnMenuInitialize(OptionsMenu m)
        {
            if (OnMenuInitialize != null)
            {
                OnMenuInitialize(m);
            }
        }

        public static void CallOnMenuClose(OptionsMenu m)
        {
            if (OnMenuClose != null)
            {
                OnMenuClose(m);
            }
        }


        //reflection stuff
        static FieldInfo OptM_categories = AccessTools.Field(typeof(OptionsMenu), "categories");

        static FieldInfo MT_hotspot = AccessTools.Field(typeof(MenuToggle), "hotspot");

        static FieldInfo AB_valueCurve = AccessTools.Field(typeof(AdjustmentBars), "valueCurve");

        static FieldInfo AB_bars = AccessTools.Field(typeof(AdjustmentBars), "bars");

        static void GetCategoryStrings(GameObject obj, out TMP_Text Title, out TMP_Text NextTitle, out TMP_Text PreviousTitle)
        {
            if (obj.name == "Data")
            {
                obj = obj.transform.Find("Main").gameObject; //dear mystman12: what the fuck.
            }

            TMP_Text title = obj.transform.Find("Title").GetComponent<TMP_Text>();
            Title = title;

            TMP_Text nextTitle = obj.transform.Find("NextTitle").GetComponent<TMP_Text>();
            NextTitle = nextTitle;


            TMP_Text previousTitle = obj.transform.Find("PreviousTitle").GetComponent<TMP_Text>();
            PreviousTitle = previousTitle;
        }

        public static GameObject CreateNewCategory(OptionsMenu m, string optName)
        {
            GameObject obj = new GameObject(optName, typeof(RectTransform));
            obj.transform.SetParent(m.transform, true); //set the parent!!

            obj.layer = LayerMask.NameToLayer("UI");

            GameObject[] categories = (GameObject[])OptM_categories.GetValue(m);

            obj.transform.position = categories[0].transform.position; //going insane


            //get the title stuff and clone em
            TMP_Text Title = categories[0].transform.Find("Title").GetComponent<TMP_Text>();
            Title = GameObject.Instantiate(Title); //i hate stupidly long lines so I did this. Someone will kill me for it.
            Title.name = "Title";
            Title.GetComponent<TextLocalizer>().key = optName;

            TMP_Text NextTitle = categories[0].transform.Find("NextTitle").GetComponent<TMP_Text>();
            NextTitle = GameObject.Instantiate(NextTitle);
            NextTitle.name = "NextTitle";

            TMP_Text PreviousTitle = categories[0].transform.Find("PreviousTitle").GetComponent<TMP_Text>();
            PreviousTitle = GameObject.Instantiate(PreviousTitle);
            PreviousTitle.name = "PreviousTitle";



            GetCategoryStrings(categories[0], out TMP_Text nextT, out TMP_Text nextNT, out TMP_Text nextPT);
            GetCategoryStrings(categories[categories.Length - 1], out TMP_Text prevT, out TMP_Text prevNT, out TMP_Text prevPT);


            //set this categories text
            Title.transform.SetParent(obj.transform, false);
            NextTitle.GetComponent<TextLocalizer>().key = nextT.text;
            NextTitle.transform.SetParent(obj.transform, false);
            PreviousTitle.GetComponent<TextLocalizer>().key = prevT.text;
            PreviousTitle.transform.SetParent(obj.transform, false);

            //set the previous and next categories text
            nextPT.GetComponent<TextLocalizer>().key = optName;
            prevNT.GetComponent<TextLocalizer>().key = optName;



            Array.Resize(ref categories, categories.Length + 1);
            categories[categories.Length - 1] = obj;
            obj.transform.localScale = Vector3.one; //why do i even have to do this

            obj.SetActive(false); //no.

            obj.transform.SetSiblingIndex(1); //fix layering issues
            OptM_categories.SetValue(m, categories);

            return obj;
        }

        public static void SetTooltip(StandardMenuButton smb, string tooltipText)
        {
            //TODO: Find a better way of doing this.
            TooltipController t = (TooltipController)smb.OnHighlight.GetPersistentTarget(0);
            smb.OnHighlight = new UnityEvent();
            smb.OnHighlight.AddListener(() =>
            {
                t.UpdateTooltip(tooltipText);
            });
        }


        public static MenuToggle CreateToggleButton(OptionsMenu m, Vector2 pos, string toggleBut, bool startState, string tooltipText)
        {
            GameObject audObj = m.transform.Find("Audio").gameObject;
            GameObject checkObj = GameObject.Instantiate(audObj.transform.Find("SubtitlesToggle").gameObject);
            checkObj.name = toggleBut;
            TMP_Text Text = checkObj.transform.Find("ToggleText").GetComponent<TMP_Text>();
            Text.transform.GetComponent<TextLocalizer>().key = toggleBut;
            MenuToggle tog = checkObj.GetComponent<MenuToggle>();

            StandardMenuButton smb = ((GameObject)MT_hotspot.GetValue(tog)).GetComponent<StandardMenuButton>();


            SetTooltip(smb,tooltipText);

            tog.Set(startState);

            checkObj.transform.position = new Vector3(pos.x, pos.y, checkObj.transform.position.z);

            return tog;
        }

        public static AdjustmentBars CreateAdjustmentBar(OptionsMenu m, Vector2 pos, string name, int barCount, string tooltipText, UnityAction act)
        {
            GameObject barobj = GameObject.Instantiate(m.transform.Find("Audio").transform.Find("EffectsAdjustment").gameObject);
            barobj.name = name;
            AdjustmentBars bar = barobj.GetComponent<AdjustmentBars>();
            AB_valueCurve.SetValue(bar,new AnimationCurve());

            StandardMenuButton leftBut = barobj.transform.Find("LeftCategoryButton").GetComponent<StandardMenuButton>();
            StandardMenuButton rightBut = barobj.transform.Find("RightCategoryButton").GetComponent<StandardMenuButton>();
            leftBut.OnPress = new UnityEvent();
            rightBut.OnPress = new UnityEvent();
            bar.onValueChanged = new UnityEvent();
            bar.onValueChanged.AddListener(act);
            leftBut.OnPress.AddListener(() =>
            {
                bar.Adjust(-1);
            });
            rightBut.OnPress.AddListener(() =>
            {
                bar.Adjust(1);
            });

            SetTooltip(leftBut, tooltipText);
            SetTooltip(rightBut, tooltipText);

            Transform BarContainer = barobj.transform.Find("Bars");

            Transform BarClone = BarContainer.GetChild(0); //-10

            //22

            //destroy the other bars, except the first
            for (int i = 1; i < 10; i++)
            {
                GameObject.Destroy(BarContainer.GetChild(i).gameObject);
            }

            List<Image> Bars = new List<Image>();

            Bars.Add(BarClone.GetComponent<Image>());

            for (int i = 1; i < barCount; i++) //start at one since the first bar already exists!
            {
                Image clone = GameObject.Instantiate<Image>(BarClone.gameObject.GetComponent<Image>(),BarContainer);
                clone.transform.localPosition += new Vector3(i * 10f, 0f);
                Bars.Add(clone);
            }

            rightBut.transform.localPosition = Bars[Bars.Count - 1].transform.localPosition + new Vector3(22f,0f);

            AB_bars.SetValue(bar,Bars.ToArray());

            barobj.transform.position = new Vector3(pos.x, pos.y, barobj.transform.position.z);

            return bar;
        }

        public static TextLocalizer CreateText(OptionsMenu m, Vector2 pos, string text)
        {
            GameObject txtobj = GameObject.Instantiate(m.transform.Find("Audio").transform.Find("EffectsText").gameObject);
            txtobj.name = text;
            TextLocalizer txt = txtobj.GetComponent<TextLocalizer>();
            txt.key = text;
            txtobj.transform.position = new Vector3(pos.x, pos.y, txtobj.transform.position.z);
            return txt;
        }

        public static StandardMenuButton CreateApplyButton(OptionsMenu m, string toolTip, UnityAction actOnPress)
        {
            GameObject appobj = GameObject.Instantiate(m.transform.Find("Graphics").transform.Find("ApplyButton").gameObject);
            StandardMenuButton btn = appobj.GetComponent<StandardMenuButton>();
            btn.OnPress = new UnityEvent();
            btn.OnPress.AddListener(actOnPress);
            SetTooltip(btn,toolTip);
            return btn;
        }
    }

    public class CustomOptionsHandler : MonoBehaviour //will this ever do ANYTHING besides exist? Who knows!
    {

    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Awake")]
    class OnOptionAwake
    {
        static void Postfix(OptionsMenu __instance)
        {
            if (__instance.gameObject.GetComponent<CustomOptionsHandler>()) return;
            __instance.gameObject.AddComponent<CustomOptionsHandler>();
            CustomOptionsCore.CallOnMenuInitialize(__instance);
        }
    }

    [HarmonyPatch(typeof(OptionsMenu))]
    [HarmonyPatch("Close")]
    class OnOptionClose
    {
        static void Postfix(OptionsMenu __instance)
        {
            CustomOptionsCore.CallOnMenuClose(__instance);
        }
    }
}
