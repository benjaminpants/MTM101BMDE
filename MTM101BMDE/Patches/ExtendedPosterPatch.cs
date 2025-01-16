using HarmonyLib;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(TextTextureGenerator))]
    [HarmonyPatch("GenerateTextTexture")]
    class ExtendedPosterPatch
    {
        static void Postfix(TextTextureGenerator __instance, Texture2D __result, PosterObject poster, Camera ___renderCamera)
        {
            if (!(poster is ExtendedPosterObject)) return; // if we aren't an extended poster, there is no reason to run this logic
            Canvas canvas = __instance.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            ExtendedPosterObject extendedPoster = (ExtendedPosterObject)poster;
            // deactivate all text
            for (int i = 0; i < __instance.textureTMPPre.Length; i++)
            {
                __instance.textureTMPPre[i].gameObject.SetActive(false);
            }

            // set up textures
            Texture2D texture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;

            List<RawImage> images = new List<RawImage>();
            for (int i = 0; i < extendedPoster.overlayData.Length; i++)
            {
                PosterImageData overlayData = extendedPoster.overlayData[i];
                GameObject imageObject = new GameObject("Image" + i);
                imageObject.transform.SetParent(canvas.transform, false);
                imageObject.transform.localPosition = Vector3.zero;
                RawImage image = imageObject.AddComponent<RawImage>();
                image.texture = overlayData.texture;
                images.Add(image);
            }
            // todo: complete
        }
    }
}
