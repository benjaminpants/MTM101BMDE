using HarmonyLib;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.UI;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(TextTextureGenerator))]
    [HarmonyPatch("LoadPosterData")]
    class ExtendedPosterPatch
    {
        static void Postfix(TextTextureGenerator __instance, PosterObject poster, TMP_Text[] ___textureTMPPre)
        {
            if (!(poster is ExtendedPosterObject)) return; // if we aren't an extended poster, there is no reason to run this logic
            Canvas canvas = __instance.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            ExtendedPosterObject extendedPoster = (ExtendedPosterObject)poster;
            for (int i = 0; i < extendedPoster.overlayData.Length; i++)
            {
                PosterImageData overlayData = extendedPoster.overlayData[i];
                //GameObject imageObject = new GameObject("Image" + i);
                RawImage image = canvas.transform.Find("OverlayImage" + i).GetComponent<RawImage>();
                image.gameObject.SetActive(true);
                image.texture = overlayData.texture;
                image.rectTransform.sizeDelta = new Vector2((float)overlayData.size.x, (float)overlayData.size.z);
                image.rectTransform.anchoredPosition = new Vector2((float)overlayData.position.x, (float)overlayData.position.z);
            }
            // now modify the text
            for (int i = 0; i < extendedPoster.textData.Length; i++)
            {
                if (!(extendedPoster.textData[i] is ExtendedPosterTextData)) continue; // nothing needs to be done here
                ExtendedPosterTextData data = (ExtendedPosterTextData)extendedPoster.textData[i];
                if (data.formats.Length != 0)
                {
                    string[] localizedFormats = new string[data.formats.Length];
                    for (int j = 0; j < data.formats.Length; j++)
                    {
                        localizedFormats[j] = Singleton<LocalizationManager>.Instance.GetLocalizedText(data.formats[j]);
                    }
                    ___textureTMPPre[i].text = string.Format(___textureTMPPre[i].text, localizedFormats);
                }
                for (int j = 0; j < data.replacementRegex.Length; j++)
                {
                    ___textureTMPPre[i].text = Regex.Replace(___textureTMPPre[i].text, data.replacementRegex[j][0], data.replacementRegex[j][1]);
                }
            }
        }
    }



    [HarmonyPatch(typeof(TextTextureGenerator))]
    [HarmonyPatch("GenerateTextTexture")]
    class ExtendedPosterCleanupPatch
    {
        static void Postfix(TextTextureGenerator __instance, PosterObject poster)
        {
            if (!(poster is ExtendedPosterObject)) return; // if we aren't an extended poster, there is no reason to run this logic
            Canvas canvas = __instance.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            int i = 0;
            while (canvas.transform.Find("OverlayImage" + i))
            {
                canvas.transform.Find("OverlayImage" + i).gameObject.SetActive(false);
                i++;
            }
        }
    }

    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("BuildPoster")]
    [HarmonyPatch(new Type[] { typeof(PosterObject), typeof(Cell), typeof(Direction), typeof(bool) })]
    [HarmonyPatch(new Type[] { typeof(PosterObject), typeof(Cell), typeof(Direction), typeof(System.Random) })]
    class ExtendedPosterEvenWithoutTextPatch
    {
        static void Prefix(PosterObject poster, out bool __state)
        {
            __state = false;
            if (!(poster is ExtendedPosterObject)) return;
            if (poster.textData.Length == 0)
            {
                __state = true;
                poster.textData = new PosterTextData[]
                {
                    new PosterTextData()
                    {
                        size = new IntVector2(0,0),
                        font = BaldiFonts.ComicSans12.FontAsset(),
                        alignment = TextAlignmentOptions.Left,
                        color = Color.clear,
                        fontSize = 1,
                        position = new IntVector2(0,0),
                        style = TMPro.FontStyles.Normal,
                        textKey = ""
                    }
                };
            }
        }

        static void Postfix(PosterObject poster, bool __state)
        {
            if (__state)
            {
                poster.textData = new PosterTextData[0];
            }
        }
    }
}
