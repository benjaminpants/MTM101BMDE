using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.UI
{
    public enum BaldiFonts
    {
        ComicSans12,
        BoldComicSans12,
        ComicSans18,
        ComicSans24,
        BoldComicSans24,
        ComicSans36,
        SmoothComicSans12,
        SmoothComicSans18,
        SmoothComicSans24,
        SmoothComicSans36
    }

    public static class UIExtensions
    {
        /// <summary>
        /// Converts a UI element to a button by adding the necessary components and initializing the correct variables and adding the "Button" tag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="autoAssign">Should the StandardMenuButtons image and text fields be automatically assigned using GetComponent?</param>
        /// <returns></returns>
        public static T ConvertToButton<T>(this GameObject obj, bool autoAssign = true) where T : StandardMenuButton
        {
            T smb = obj.AddComponent<T>();
            smb.InitializeAllEvents();
            if (autoAssign)
            {
                smb.image = obj.GetComponent<Image>();
                smb.text = obj.GetComponent<TMP_Text>();
            }
            smb.gameObject.tag = "Button";
            return smb;
        }

        public static float FontSize(this BaldiFonts font)
        {
            switch (font)
            {
                case BaldiFonts.ComicSans12:
                    return 12f;
                case BaldiFonts.BoldComicSans12:
                    return 12f;
                case BaldiFonts.ComicSans18:
                    return 18f;
                case BaldiFonts.ComicSans24:
                    return 24f;
                case BaldiFonts.BoldComicSans24:
                    return 24f;
                case BaldiFonts.ComicSans36:
                    return 36f;
                case BaldiFonts.SmoothComicSans12:
                    return 12f;
                case BaldiFonts.SmoothComicSans18:
                    return 18f;
                case BaldiFonts.SmoothComicSans24:
                    return 24f;
                case BaldiFonts.SmoothComicSans36:
                    return 36f;
                default:
                    throw new NotImplementedException();
            }
        }

        public static TMP_FontAsset FontAsset(this BaldiFonts font)
        {
            switch (font)
            {
                case BaldiFonts.ComicSans12:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_12_Pro");
                case BaldiFonts.BoldComicSans12:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_BOLD_12_Pro");
                case BaldiFonts.ComicSans18:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_18_Pro");
                case BaldiFonts.ComicSans24:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_24_Pro");
                case BaldiFonts.BoldComicSans24:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_BOLD_24_Pro");
                case BaldiFonts.ComicSans36:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_36_Pro");
                case BaldiFonts.SmoothComicSans12:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_12_Smooth_Pro");
                case BaldiFonts.SmoothComicSans18:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_18_Smooth_Pro");
                case BaldiFonts.SmoothComicSans24:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_24_Smooth_Pro");
                case BaldiFonts.SmoothComicSans36:
                    return MTM101BaldiDevAPI.AssetMan.Get<TMP_FontAsset>("COMIC_36_Smooth_Pro");
                default:
                    throw new NotImplementedException();
            }
        }

        public static StandardMenuButton InitializeAllEvents(this StandardMenuButton smb)
        {
            smb.OnPress = new UnityEngine.Events.UnityEvent();
            smb.OnHighlight = new UnityEngine.Events.UnityEvent();
            smb.OnRelease = new UnityEngine.Events.UnityEvent();
            smb.OffHighlight = new UnityEngine.Events.UnityEvent();
            return smb;
        }
    }

    public static class UIHelpers
    {
        /// <summary>
        /// Creates an image based off of the sprite, handling its RectTransform.
        /// </summary>
        /// <param name="spr"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="correctPosition">If the position should be corrected based off of the top left of a 4:3 screen. This is primarily for custom field trips and UI.</param>
        /// <param name="scale">The scale of the image</param>
        /// <returns></returns>
        public static Image CreateImage(Sprite spr, Transform parent, Vector3 position, bool correctPosition = false, float scale = 1f)
        {
            Image img = new GameObject().AddComponent<Image>();
            img.gameObject.layer = LayerMask.NameToLayer("UI");
            img.transform.SetParent(parent);
            img.sprite = spr;
            img.gameObject.transform.localScale = Vector3.one;
            img.rectTransform.offsetMin = new Vector2(-spr.rect.width / 2f, -spr.rect.height / 2f);
            img.rectTransform.offsetMax = new Vector2(spr.rect.width / 2f, spr.rect.height / 2f);
            img.rectTransform.anchorMin = new Vector2(0f, 1f);
            img.rectTransform.anchorMax = new Vector2(0f, 1f);
            if (correctPosition)
            {
                img.transform.localPosition = new Vector3(-240f, 180f) + (new Vector3(position.x, position.y * -1f));
            }
            else
            {
                img.transform.localPosition = position;
            }
            img.transform.localScale *= scale;
            return img;
        }

        static FieldInfo _canvas = AccessTools.Field(typeof(GlobalCamCanvasAssigner), "canvas");

        /// <summary>
        /// Adds a standard 4:3 cursor initiator to the respective canvas.
        /// </summary>
        /// <param name="canvas">The canvas to add to.</param>
        /// <returns></returns>
        public static CursorInitiator AddCursorInitiatorToCanvas(Canvas canvas)
        {
            return AddCursorInitiatorToCanvas(canvas, new Vector2(480f, 360f), null);
        }

        /// <summary>
        /// Adds a cursor initiator to the respective canvas with the specified screensize.
        /// </summary>
        /// <param name="canvas">The canvas to add to.</param>
        /// <param name="screenSize">The screen size for the initiator.</param>
        /// <param name="prefab">The prefab to use, if left null, it will use the standard one.</param>
        /// <returns></returns>
        public static CursorInitiator AddCursorInitiatorToCanvas(Canvas canvas, Vector2 screenSize, CursorController prefab = null)
        {
            CursorInitiator initiator = canvas.gameObject.AddComponent<CursorInitiator>();
            initiator.cursorPre = prefab == null ? MTM101BaldiDevAPI.AssetMan.Get<CursorController>("cursorController") : prefab;
            initiator.screenSize = screenSize;
            initiator.graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            return initiator;
        }


        /// <summary>
        /// Create a blank UI canvas, based off of the canvas' on the title screen.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="addCanvasAssigner"></param>
        /// <param name="startActive">Determines if the Canvas should start active or not.</param>
        /// <returns></returns>
        public static Canvas CreateBlankUIScreen(string name, bool addCanvasAssigner = true, bool startActive = true)
        {
            GameObject obj = new GameObject(name);
            obj.SetActive(false);
            obj.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.defaultSpriteDPI = 96f;
            scaler.fallbackScreenDPI = 96f;
            scaler.referencePixelsPerUnit = 100f;
            scaler.physicalUnit = CanvasScaler.Unit.Points;
            scaler.referenceResolution = new Vector2(480f, 360f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referencePixelsPerUnit = 1f;
            GraphicRaycaster raycaster = obj.AddComponent<GraphicRaycaster>();
            raycaster.blockingMask = -1;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            if (!addCanvasAssigner)
            {
                obj.AddComponent<PlaneDistance>();
                GlobalCamCanvasAssigner gcca = obj.AddComponent<GlobalCamCanvasAssigner>();
                _canvas.SetValue(gcca, canvas);
            }
            obj.SetActive(startActive);
            // todo: investigate plane distance, does it hold any relevancy?
            return canvas;
        }

        /// <summary>
        /// Add 4:3 borders to the respective canvas.
        /// </summary>
        /// <param name="canvas"></param>
        public static void AddBordersToCanvas(Canvas canvas)
        {
            Image image1 = new GameObject("Border1").AddComponent<Image>();
            image1.transform.localPosition = new Vector3(-752f, 0f, 0f);
            image1.rectTransform.anchorMin = Vector2.one / 2f;
            image1.rectTransform.anchorMax = Vector2.one / 2f;
            image1.rectTransform.sizeDelta = new Vector2(1024f, 360f);
            image1.color = Color.black;

            Image image2 = new GameObject("Border2").AddComponent<Image>();
            image2.transform.localPosition = new Vector3(752f, 0f, 0f);
            image2.rectTransform.anchorMin = Vector2.one / 2f;
            image2.rectTransform.anchorMax = Vector2.one / 2f;
            image2.rectTransform.sizeDelta = new Vector2(1024f, 360f);
            image2.color = Color.black;

            Image image3 = new GameObject("Border3").AddComponent<Image>();
            image3.transform.localPosition = new Vector3(0f, 360f, 0f);
            image3.rectTransform.anchorMin = Vector2.one / 2f;
            image3.rectTransform.anchorMax = Vector2.one / 2f;
            image3.rectTransform.sizeDelta = new Vector2(2528f, 360f);
            image3.color = Color.black;

            Image image4 = new GameObject("Border4").AddComponent<Image>();
            image4.transform.localPosition = new Vector3(0f, -360f, 0f);
            image4.rectTransform.anchorMin = Vector2.one / 2f;
            image4.rectTransform.anchorMax = Vector2.one / 2f;
            image4.rectTransform.sizeDelta = new Vector2(2528f, 360f);
            image4.color = Color.black;

            // i hate how hacky this is i hate unity
            GameObject holderObject = new GameObject("Bottom");
            holderObject.transform.SetParent(canvas.transform, true);
            image1.transform.SetParent(holderObject.transform);
            image2.transform.SetParent(holderObject.transform);
            image3.transform.SetParent(holderObject.transform);
            image4.transform.SetParent(holderObject.transform);
            holderObject.transform.localPosition = Vector3.zero;
            holderObject.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Creates an image based off of the sprite, handling its RectTransform.
        /// </summary>
        /// <param name="spr"></param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="correctPosition">If the position should be corrected based off of the top left of a 4:3 screen. This is primarily for UI.</param>
        /// <returns></returns>
        public static Image CreateImage(Sprite spr, Transform parent, Vector3 position, bool correctPosition = false)
        {
            return CreateImage(spr, parent, position, correctPosition, 1f);
        }

        /// <summary>
        /// Creates text object using the requested font.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="font">The font to use, represented as an enum.</param>
        /// <param name="text">The text to use</param>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="correctPosition">If the position should be corrected based off of the top left of a 4:3 screen. This is primarily for UI.</param>
        /// <returns></returns>
        public static T CreateText<T>(BaldiFonts font, string text, Transform parent, Vector3 position, bool correctPosition = false) where T : TMP_Text
        {
            T tmp = new GameObject().AddComponent<T>();
            tmp.name = "Text";
            tmp.gameObject.layer = LayerMask.NameToLayer("UI");
            tmp.transform.SetParent(parent);
            tmp.gameObject.transform.localScale = Vector3.one;
            tmp.fontSize = font.FontSize();
            tmp.font = font.FontAsset();
            if (correctPosition)
            {
                tmp.transform.localPosition = new Vector3(-240f, 180f) + (new Vector3(position.x, position.y * -1f));
            }
            else
            {
                tmp.transform.localPosition = position;
            }
            tmp.text = text;
            return tmp;
        }


    }
}
