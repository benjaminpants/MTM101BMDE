using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI
{
    public class SaveManagerMenu : MonoBehaviour
    {
        public void ShiftMenu(int shiftBy)
        {
            internalIndex = Mathf.Clamp(internalIndex + shiftBy,0, Singleton<ModdedFileManager>.Instance.saveIndexes.Count - 1);
            int externalIndex = Singleton<ModdedFileManager>.Instance.saveIndexes[internalIndex];
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            idText.text = externalIndex.ToString("D3");
            seedText.text = game.hasFile ? "SEED: " + game.seed : "NO DATA";
        }

        int internalIndex = 0;

        TMP_Text idText;
        TMP_Text seedText;

        internal static void MenuHook(OptionsMenu __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
            GameObject ob = CustomOptionsCore.CreateNewCategory(__instance, "Save\nManagement");
            SaveManagerMenu me = ob.AddComponent<SaveManagerMenu>();
            StandardMenuButton arrowLeft = UIHelpers.CreateImage(MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow"), ob.transform, new Vector3(120, 136), true).gameObject.ConvertToButton<StandardMenuButton>(); ;
            arrowLeft.transform.localScale /= 2;
            arrowLeft.highlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowHighlight");
            arrowLeft.swapOnHigh = true;
            arrowLeft.unhighlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow");
            arrowLeft.OnPress.AddListener(() =>
            {
                me.ShiftMenu(-1);
            });

            StandardMenuButton arrowRight = UIHelpers.CreateImage(MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow"), ob.transform, new Vector3(185, 136), true).gameObject.ConvertToButton<StandardMenuButton>(); ;
            arrowRight.transform.localScale /= 2;
            arrowRight.transform.localScale = new Vector3(-arrowRight.transform.localScale.x, arrowRight.transform.localScale.y, arrowRight.transform.localScale.z);
            arrowRight.highlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowHighlight");
            arrowRight.swapOnHigh = true;
            arrowRight.unhighlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow");
            arrowRight.OnPress.AddListener(() =>
            {
                me.ShiftMenu(1);
            });
            TMP_Text text = CustomOptionsCore.CreateText(__instance, Vector3.zero, "ID").GetComponent<TMP_Text>();
            GameObject.Destroy(text.GetComponent<TextLocalizer>());
            text.transform.SetParent(ob.transform, false);
            text.alignment = TextAlignmentOptions.Midline;
            text.text = "000";
            text.transform.localPosition = new Vector3(-88f, 44f);

            me.idText = text;

            TMP_Text seed = CustomOptionsCore.CreateText(__instance, Vector3.zero, "Seed").GetComponent<TMP_Text>();
            GameObject.Destroy(seed.GetComponent<TextLocalizer>());
            seed.transform.SetParent(ob.transform, false);
            seed.alignment = TextAlignmentOptions.MidlineLeft;
            seed.text = "Seed: 153268942";
            seed.transform.localPosition = new Vector3(60f, 44f);
            seed.rectTransform.offsetMax += (Vector2.right * 60f);

            me.seedText = seed;

            me.internalIndex = Singleton<ModdedFileManager>.Instance.saveIndexes.IndexOf(Singleton<ModdedFileManager>.Instance.saveIndex);

            me.ShiftMenu(0);
        }
    }
}
