using HarmonyLib;
using MTM101BaldAPI.PlusExtensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(TextTextureGenerator))]
    [HarmonyPatch("GenerateTextTexture")]
    class ExtendedPosterPatch
    {
        static void Postfix(TextTextureGenerator __instance, Texture2D __result, PosterObject poster, Camera ___renderCamera, RenderTexture ___renderTexture, Rect ____readRect)
        {
            if (!(poster is ExtendedPosterObject)) return; // if we aren't an extended poster, there is no reason to run this logic
            MTM101BaldiDevAPI.Log.LogInfo("Found extended poster object!");
            Canvas canvas = __instance.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            ExtendedPosterObject extendedPoster = (ExtendedPosterObject)poster;
            // deactivate all text
            for (int i = 0; i < __instance.textureTMPPre.Length; i++)
            {
                __instance.textureTMPPre[i].gameObject.SetActive(false);
            }

            List<RawImage> images = new List<RawImage>();
            for (int i = 0; i < extendedPoster.overlayData.Length; i++)
            {
                PosterImageData overlayData = extendedPoster.overlayData[i];
                GameObject imageObject = new GameObject("Image" + i);
                imageObject.transform.SetParent(canvas.transform, false);
                imageObject.transform.localPosition = Vector3.zero;
                imageObject.layer = LayerMask.NameToLayer("UI");
                imageObject.transform.localScale = Vector3.one;
                RawImage image = imageObject.AddComponent<RawImage>();
                image.texture = overlayData.texture;
                image.rectTransform.sizeDelta = new Vector2((float)overlayData.size.x, (float)overlayData.size.z);
                image.rectTransform.localPosition = new Vector2((float)overlayData.position.x, (float)overlayData.position.z);
                image.maskable = true;
                MTM101BaldiDevAPI.Log.LogInfo("Setting up: " + i + "!");
                MTM101BaldiDevAPI.Log.LogInfo(imageObject.transform.localScale);
                MTM101BaldiDevAPI.Log.LogInfo(imageObject.transform.localPosition);
                images.Add(image);
            }
            // todo: complete
            ___renderCamera.Render(); // re-render everything
            RenderTexture.active = ___renderTexture;

            Texture2D ourTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            ourTex.filterMode = FilterMode.Point;
            ourTex.filterMode = FilterMode.Point;

            ourTex.ReadPixels(____readRect,0,0);
            ourTex.Apply(); //is this apply necessary
            Color[] pixels = __result.GetPixels();

            Color[] array = ourTex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = Mathf.Lerp(pixels[i].r, array[i].r, array[i].a);
                pixels[i].g = Mathf.Lerp(pixels[i].g, array[i].g, array[i].a);
                pixels[i].b = Mathf.Lerp(pixels[i].b, array[i].b, array[i].a);
                pixels[i].g = 0;
            }

            __result.SetPixels(pixels);
            __result.Apply();

            GameObject.Destroy(ourTex); //cleanup

            for (int i = 0; i < images.Count; i++)
            {
                GameObject.Destroy(images[i]);
            }
        }
    }
}
