using HarmonyLib;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTM101BaldAPI
{
    public class SaveManagerMenu : CustomOptionsCategory
    {

        public void UpdateModList()
        {
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            for (int i = (modListPage * modList.Length); i < ((modListPage + 1) * modList.Length); i++)
            {
                if (i < game.mods.Length)
                {
                    modList[i % modList.Length].text = game.mods[i];
                }
                else
                {
                    modList[i % modList.Length].text = "";
                }
            }
        }

        public void ShiftModList(int shiftBy)
        {
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            int maxPage = Mathf.Max(Mathf.CeilToInt((float)game.mods.Length / modList.Length) - 1,0);
            modListPage = Mathf.Clamp(modListPage + shiftBy, 0, maxPage);
            leftPageButton.gameObject.SetActive((maxPage > 0) && (modListPage != 0));
            rightPageButton.gameObject.SetActive(((maxPage > 0) && (modListPage != maxPage)));
            UpdateModList();
        }

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
            modListPage = 0;
            UpdateModList();
            ShiftModList(0);
            return;
            deleteButton.text.color = deletingAllowed ? Color.black : Color.gray;
            deleteButton.tag = deletingAllowed ? "Button" : "Untagged";
            transferButton.text.color = game.canBeMoved ? Color.black : Color.gray;
            transferButton.tag = game.canBeMoved ? "Button" : "Untagged";
        }

        // prevent certain oddities with blank elements
        public void HideTooltipForModListElement(int index)
        {
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            int finalIndex = (modListPage * modList.Length) + index;
            if (finalIndex >= game.mods.Length) return;
            tooltipController.CloseTooltip();
        }

        public void ShowTooltipForModListElement(int index)
        {
            PartialModdedSavedGame game = Singleton<ModdedFileManager>.Instance.saveDatas[externalIndex];
            int finalIndex = (modListPage * modList.Length) + index;
            if (finalIndex >= game.mods.Length) return;
            string mod = game.mods[finalIndex];
            if (ModdedSaveGame.ModdedSaveGameHandlers.ContainsKey(mod))
            {
                tooltipController.UpdateTooltip(ModdedSaveGame.ModdedSaveGameHandlers[mod].DisplayTags(game.tags[mod]));
            }
            else
            {
                tooltipController.UpdateTooltip(ModdedSaveGameIOBinary.DisplayTagsDefault(game.tags[mod]));
            }
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

        int modListPage = 0;
        int internalIndex = 0;
        int externalIndex => Singleton<ModdedFileManager>.Instance.saveIndexes[internalIndex];

        TextMeshProUGUI idText;
        TextMeshProUGUI seedText;
        TextMeshProUGUI[] modList = new TextMeshProUGUI[6];
        TextMeshProUGUI warnText;
        StandardMenuButton deleteButton;
        StandardMenuButton transferButton;
        StandardMenuButton leftPageButton;
        StandardMenuButton rightPageButton;
        Action currentConfirmAction;

        GameObject mainScreen;
        GameObject warnScreen;

        bool deletingAllowed;

        public override void Build()
        {
            mainScreen = new GameObject("MainScreen");
            mainScreen.transform.SetParent(transform, false);
            warnScreen = new GameObject("WarnScreen");
            warnScreen.transform.SetParent(transform, false);
            warnScreen.SetActive(false);
            float textDist = 62f;
            StandardMenuButton arrowLeft = CreateButton(() => { ShiftMenu(-1); }, menuArrowLeft, menuArrowLeftHighlight, "LeftShiftButton", new Vector3(-150f, 45f));
            StandardMenuButton arrowRight = CreateButton(() => { ShiftMenu(1); }, menuArrowRight, menuArrowRightHighlight, "RightShiftButton", new Vector3(-150f + textDist, 45f));
            idText = CreateText("IDText", "999", new Vector3(-150f + (textDist / 2f), 45f), BaldiFonts.ComicSans24, TextAlignmentOptions.Midline, new Vector3(textDist, 32f), Color.black, false);
            seedText = CreateText("SeedText", "SEED: 1234567890", new Vector3(-150f + textDist + 16f, 45f), BaldiFonts.ComicSans24, TextAlignmentOptions.MidlineLeft, new Vector2(260f, 32f), Color.black, false);
            seedText.rectTransform.pivot = new Vector2(0f,0.5f);

            TextMeshProUGUI modListHeader = CreateText("ModListHeader", "<b><u>USED MODS:", new Vector3(0f,32f), BaldiFonts.ComicSans12, TextAlignmentOptions.Bottom, new Vector2(260f,32f), Color.black, false);
            modListHeader.rectTransform.pivot = new Vector2(0.5f, 1f);
            for (int i = 0; i < modList.Length; i++)
            {
                modList[i] = CreateText("ModList" + i, "mtm101.rulerp.bbplus.baldidevapi", new Vector3(0f, (0f) + (i * -16f)), BaldiFonts.ComicSans12, TextAlignmentOptions.Top, new Vector2(300f, 17f), Color.black, false);
                modList[i].rectTransform.pivot = new Vector2(0.5f, 1f);
                StandardMenuButton menButton = modList[i].gameObject.ConvertToButton<StandardMenuButton>();
                menButton.underlineOnHigh = true;
                modList[i].raycastTarget = true;
                modList[i].overflowMode = TextOverflowModes.Ellipsis;
                menButton.audConfirmOverride = silence;
                menButton.eventOnHigh = true;
                int currentIndex = i; //if i dont do this then i will always be modList.Length
                menButton.OnHighlight.AddListener(() => { ShowTooltipForModListElement(currentIndex); });
                menButton.OffHighlight.AddListener(() => { HideTooltipForModListElement(currentIndex); });
            }

            leftPageButton = CreateButton(() => { ShiftModList(-1); }, menuArrowLeft, menuArrowLeftHighlight, "LeftPageButton", new Vector3(-160f, -45f));
            rightPageButton = CreateButton(() => { ShiftModList(1); }, menuArrowRight, menuArrowRightHighlight, "RightPageButton", new Vector3(160f, -45f));

            //deleteButton = CreateTextButton(() => { }, "DeleteButton", "<b>Delete", new Vector3(0f,-132f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(100f, 32f), Color.black);

            SwitchToMain(false);

            /*
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
            arrowLeft.highlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrowHighlight");
            arrowLeft.swapOnHigh = true;
            arrowLeft.unhighlightedSprite = MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow");
            arrowLeft.OnPress.AddListener(() =>
            {
                me.ShiftMenu(-1);
            });

            StandardMenuButton arrowRight = UIHelpers.CreateImage(MTM101BaldiDevAPI.AssetMan.Get<Sprite>("MenuArrow"), me.mainScreen.transform, new Vector3(185, 136) + topBarOffset, true).gameObject.ConvertToButton<StandardMenuButton>(); ;
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
                    ModdedSaveGame.ModdedSaveGameHandlers.Do(x => x.Value.Reset()); //reset all
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
            */
        }

        internal static void MenuHook(OptionsMenu __instance, CustomOptionsHandler handler)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler != SavedGameDataHandler.Modded) return;
            handler.AddCategory<SaveManagerMenu>("Modded\nSaved Games");
        }
    }
}
