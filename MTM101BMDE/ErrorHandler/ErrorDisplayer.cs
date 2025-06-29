using HarmonyLib;
using MTM101BaldAPI.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace MTM101BaldAPI.ErrorHandler
{

    [ConditionalPatchConfig(MTM101BaldiDevAPI.ModGUID, "General", "Visible Exceptions")]
    [HarmonyPatch(typeof(HudManager))]
    [HarmonyPatch("Awake")]
    static class ErrorDisplayPatch
    {
        static void Postfix(HudManager __instance)
        {
            ErrorDisplayer errorDisp = __instance.gameObject.AddComponent<ErrorDisplayer>();
            errorDisp.errorSound = MTM101BaldiDevAPI.AssetMan.Get<AudioClip>("ErrorSound");
        }
    }

    public class ErrorDisplayer : MonoBehaviour
    {
        public static List<ErrorDisplayer> allErrorDisplayers = new List<ErrorDisplayer>();

        Canvas canvas;

        TextMeshProUGUI text;

        static FieldInfo _canvas = AccessTools.Field(typeof(HudManager), "canvas");
        AudioSource audSource;
        public AudioClip errorSound;
        IEnumerator currentError;

        void Awake()
        {
            allErrorDisplayers.Add(this);
            canvas = (Canvas)_canvas.GetValue(GetComponent<HudManager>());
            audSource = gameObject.AddComponent<AudioSource>();
        }

        void Start()
        {
            text = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "[!] mtm101.rulerp.bbplus.baldidevapi is generating exceptions!", canvas.transform, Vector3.zero);
            text.name = "ErrorDisplay";
            text.rectTransform.sizeDelta = new Vector2(800f, 100f);
            text.alignment = TextAlignmentOptions.Top;
            text.transform.localPosition = Vector3.up * 85f;
            text.color = Color.red;
            text.gameObject.SetActive(false);
        }

        IEnumerator ErrorCoroutine(string errorText, float time)
        {
            text.text = errorText;
            text.gameObject.SetActive(true);
            text.color = Color.red;
            yield return new WaitForSecondsRealtime(time);
            float fadeTime = 0f;
            while (fadeTime < 1f)
            {
                fadeTime += Time.unscaledDeltaTime;
                text.color = new Color(1f,0f,0f,Mathf.Round((1f - fadeTime) * 8f) / 8f);
                yield return null;
            }
            text.gameObject.SetActive(false);
            currentError = null;
            yield break;
        }

        public void ShowError(string text, float time)
        {
            if (currentError != null)
            {
                StopCoroutine(currentError);
            }
            currentError = ErrorCoroutine(text, time);
            StartCoroutine(currentError);
            if (audSource.isPlaying) return;
            audSource.PlayOneShot(errorSound);
        }

        void OnDestroy()
        {
            allErrorDisplayers.Remove(this);
        }
    }
}
