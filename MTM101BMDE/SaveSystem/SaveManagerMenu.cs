using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            internalIndex = (internalIndex + shiftBy) % Singleton<ModdedFileManager>.Instance.saveIndexes.Count;
            if (internalIndex < 0)
            {
                internalIndex += Singleton<ModdedFileManager>.Instance.saveIndexes.Count;
            }
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            bool onCurrentSave = externalIndex == Singleton<ModdedFileManager>.Instance.saveIndex;
            deletingAllowed = !onCurrentSave;
            idText.text = (onCurrentSave ? "<b>" : "") + externalIndex.ToString("D3");
            seedText.text = game.hasFile ? "SEED: " + game.seed : "NO DATA";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("USED MODS:");
            for (int i = 0; i < game.mods.Length; i++)
            {
                builder.AppendLine(game.mods[i]);
            }
            modList.text = builder.ToString();
            deleteButton.text.color = deletingAllowed ? Color.black : Color.gray;
            deleteButton.tag = deletingAllowed ? "Button" : "Untagged";
            transferButton.text.color = game.canBeMoved ? Color.black : Color.gray;
            transferButton.tag = game.canBeMoved ? "Button" : "Untagged";
        }

        public void SwitchToMain(bool preserveCurrentIndex)
        {
            if (!preserveCurrentIndex)
            {
                internalIndex = Singleton<ModdedFileManager>.Instance.saveIndexes.IndexOf(Singleton<ModdedFileManager>.Instance.saveIndex);
            }
            warnScreen.SetActive(false);
            mainScreen.SetActive(true);
            ShiftMenu(0);
        }

        public void SwitchToWarning(string textToDisplay, Action onConfirm)
        {
            warnScreen.SetActive(true);
            mainScreen.SetActive(false);
            warnText.text = "<b>WARNING!</b>\n" + textToDisplay;
            currentConfirmAction = onConfirm;
        }

        int internalIndex = 0;
        int externalIndex => Singleton<ModdedFileManager>.Instance.saveIndexes[internalIndex];

        TMP_Text idText;
        TMP_Text seedText;
        TMP_Text modList;
        TMP_Text warnText;
        StandardMenuButton deleteButton;
        StandardMenuButton transferButton;
        Action currentConfirmAction;

        GameObject mainScreen;
        GameObject warnScreen;

        bool deletingAllowed;

        internal static void MenuHook(OptionsMenu __instance)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
            GameObject ob = CustomOptionsCore.CreateNewCategory(__instance, "Modded\nSaved Games");
            SaveManagerMenu me = ob.AddComponent<SaveManagerMenu>();
            me.mainScreen = new GameObject();
            me.mainScreen.name = "MainScreen";
            me.mainScreen.transform.SetParent(ob.transform,false);

            me.warnScreen = new GameObject();
            me.warnScreen.name = "WarnScreen";
            me.warnScreen.transform.SetParent(ob.transform, false);

            // define the main screen
            Vector3 topBarOffset = new Vector3(-30f, 0f);
            StandardMenuButton arrowLeft = UIHelpers.CreateImage(MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow"), me.mainScreen.transform, new Vector3(120, 136) + topBarOffset, true).gameObject.ConvertToButton<StandardMenuButton>(); ;
            arrowLeft.transform.localScale /= 2;
            arrowLeft.highlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowHighlight");
            arrowLeft.swapOnHigh = true;
            arrowLeft.unhighlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow");
            arrowLeft.OnPress.AddListener(() =>
            {
                me.ShiftMenu(-1);
            });

            StandardMenuButton arrowRight = UIHelpers.CreateImage(MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow"), me.mainScreen.transform, new Vector3(185, 136) + topBarOffset, true).gameObject.ConvertToButton<StandardMenuButton>(); ;
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
            text.transform.SetParent(me.mainScreen.transform, false);
            text.alignment = TextAlignmentOptions.Midline;
            text.text = "000";
            text.transform.localPosition = new Vector3(-88f, 44f) + topBarOffset;

            me.idText = text;

            TMP_Text seed = CustomOptionsCore.CreateText(__instance, Vector3.zero, "Seed").GetComponent<TMP_Text>();
            GameObject.Destroy(seed.GetComponent<TextLocalizer>());
            seed.transform.SetParent(me.mainScreen.transform, false);
            seed.alignment = TextAlignmentOptions.MidlineLeft;
            seed.text = "Seed: 153268942";
            seed.transform.localPosition = new Vector3(60f, 44f) + topBarOffset;
            seed.rectTransform.offsetMax += (Vector2.right * 60f);

            me.seedText = seed;

            TMP_Text modList = CustomOptionsCore.CreateText(__instance, Vector3.zero, "ModList").GetComponent<TMP_Text>();
            GameObject.Destroy(modList.GetComponent<TextLocalizer>());
            modList.transform.SetParent(me.mainScreen.transform, false);
            modList.alignment = TextAlignmentOptions.Top;
            modList.text = "BLAH BLAH BLAH!!";
            modList.fontSize = 14;
            modList.rectTransform.offsetMax += (Vector2.right * 60f);
            modList.transform.localPosition = new Vector3(0f, 0f);

            me.modList = modList;

            StandardMenuButton button = CustomOptionsCore.CreateTextButton(__instance, Vector2.zero, "<b>Delete", "Deletes this saved game.", () =>
            {
                me.SwitchToWarning("Deleting a saved game <i>CANNOT</i> be undone!\nAre you sure?", () =>
                {
                    Singleton<ModdedFileManager>.Instance.DeleteIndexedGame(me.externalIndex);
                    __instance.GetComponent<AudioManager>().PlaySingle(MTM101BaldiDevAPI.AssetMan.Get<SoundObject>("Explosion"));
                    me.SwitchToMain(false);
                });
            });
            button.transform.SetParent(me.mainScreen.transform, false);
            button.text.alignment = TextAlignmentOptions.Center;
            button.transform.localPosition = new Vector3(0f,-115f);

            me.deleteButton = button;

            StandardMenuButton transferbutton = CustomOptionsCore.CreateTextButton(__instance, Vector2.zero, "<b>Transfer To Current Game", "Transfers this game to the current save.", () =>
            {
                me.SwitchToWarning("This will transfer the game to this save, not duplicate it! If you transfer, you won't be able to load this save with the old mods! Are you sure?", () =>
                {
                    // load the saved game
                    Singleton<ModdedFileManager>.Instance.LoadGameWithIndex(ModdedSaveSystem.GetSaveFolder(MTM101BaldiDevAPI.Instance, Singleton<PlayerFileManager>.Instance.fileName), me.externalIndex);
                    Singleton<ModdedFileManager>.Instance.DeleteIndexedGame(me.externalIndex);
                    Singleton<ModdedFileManager>.Instance.UpdateCurrentPartialSave();
                    __instance.GetComponent<AudioManager>().PlaySingle(MTM101BaldiDevAPI.AssetMan.Get<SoundObject>("Xylophone"));
                    me.SwitchToMain(false);
                });
            });
            transferbutton.transform.SetParent(me.mainScreen.transform, false);
            transferbutton.text.alignment = TextAlignmentOptions.Center;
            transferbutton.text.rectTransform.offsetMax += (Vector2.right * 240f);
            transferbutton.text.rectTransform.localPosition = Vector3.zero;
            transferbutton.GetComponent<RectTransform>().offsetMax += (Vector2.right * 240f);
            transferbutton.transform.localPosition = new Vector3(0f, -155f);

            me.transferButton = transferbutton;

            // define the warn screen
            TMP_Text warnText = CustomOptionsCore.CreateText(__instance, Vector3.zero, "WARNING").GetComponent<TMP_Text>();
            GameObject.Destroy(warnText.GetComponent<TextLocalizer>());
            warnText.transform.SetParent(me.warnScreen.transform, false);
            warnText.alignment = TextAlignmentOptions.Top;
            warnText.color = Color.red;
            warnText.text = "<b>WARNING!</b>\nDELETING IS AN ACTION THAT CAN NOT BE UNDONE! ARE YOU SURE?";
            warnText.rectTransform.offsetMax += (Vector2.right * 140f);
            warnText.transform.localPosition = Vector3.up * 40f;
            me.warnText = warnText;

            StandardMenuButton yesButton = CustomOptionsCore.CreateTextButton(__instance, Vector2.zero, "YES", "", () =>
            {
                me.currentConfirmAction.Invoke();
            });

            yesButton.transform.SetParent(me.warnScreen.transform, false);
            yesButton.transform.localPosition = new Vector3(-100f,-150f);
            yesButton.text.alignment = TextAlignmentOptions.Center;
            yesButton.text.color = Color.red;

            StandardMenuButton noButton = CustomOptionsCore.CreateTextButton(__instance, Vector2.zero, "NO", "", () =>
            {
                me.SwitchToMain(true);
            });

            noButton.transform.SetParent(me.warnScreen.transform, false);
            noButton.transform.localPosition = new Vector3(100f, -150f);
            noButton.text.alignment = TextAlignmentOptions.Center;
            noButton.text.color = Color.red;

            me.SwitchToMain(false);
        }
    }
}
