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
            if (ModdedSaveGame.ModdedSaveGameHandlers.ContainsKey(mod) && (!MTM101BaldiDevAPI.Instance.ignoringTagDisplays.Value))
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
                modList[i] = CreateText("ModList" + i, MTM101BaldiDevAPI.ModGUID, new Vector3(0f, (0f) + (i * -16f)), BaldiFonts.ComicSans12, TextAlignmentOptions.Top, new Vector2(300f, 17f), Color.black, false);
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

            deleteButton = CreateTextButton(() => {
                SwitchToWarning("Deleting a saved game <i>CANNOT</i> be undone!\nAre you sure?", () =>
                {
                    Singleton<ModdedFileManager>.Instance.DeleteIndexedGame(externalIndex);
                    optionsMenu.GetComponent<AudioManager>().PlaySingle(MTM101BaldiDevAPI.AssetMan.Get<SoundObject>("Explosion"));
                    SwitchToMain(false);
                });
            }, "DeleteButton", "<b>Delete", new Vector3(0f,-112f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(100f, 32f), Color.black);
            transferButton = CreateTextButton(() => {
                SwitchToWarning("This will transfer the game to this save, not duplicate it! If you transfer, you won't be able to load this save with the old mods! Are you sure?", () =>
                {
                    // load the saved game
                    Dictionary<string, string[]> currentTags = new Dictionary<string, string[]>();
                    // do this seperately incase mods depend on eachother for tags. they shouldn't but just incase!
                    ModdedSaveGame.ModdedSaveGameHandlers.Do(x =>
                    {
                        currentTags.Add(x.Key, x.Value.GenerateTags());
                    });
                    ModdedSaveGame.ModdedSaveGameHandlers.Do(x => x.Value.Reset()); //reset all
                    Singleton<ModdedFileManager>.Instance.LoadGameWithIndex(ModdedSaveSystem.GetSaveFolder(MTM101BaldiDevAPI.Instance, Singleton<PlayerFileManager>.Instance.fileName), externalIndex, false);
                    Singleton<ModdedFileManager>.Instance.DeleteIndexedGame(externalIndex);
                    Singleton<ModdedFileManager>.Instance.saveData.modTags = currentTags;
                    Singleton<ModdedFileManager>.Instance.UpdateCurrentPartialSave();
                    optionsMenu.GetComponent<AudioManager>().PlaySingle(MTM101BaldiDevAPI.AssetMan.Get<SoundObject>("Xylophone"));
                    SwitchToMain(false);
                });
            }, "TransferButton", "<b>Transfer To Current Game", new Vector3(0f, -150f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(300f, 32f), Color.black);

            arrowLeft.transform.SetParent(mainScreen.transform, false);
            arrowRight.transform.SetParent(mainScreen.transform, false);
            idText.transform.SetParent(mainScreen.transform, false);
            seedText.transform.SetParent(mainScreen.transform, false);
            modListHeader.transform.SetParent(mainScreen.transform, false);
            leftPageButton.transform.SetParent(mainScreen.transform, false);
            rightPageButton.transform.SetParent(mainScreen.transform, false);
            deleteButton.transform.SetParent(mainScreen.transform, false);
            transferButton.transform.SetParent(mainScreen.transform, false);
            for (int i = 0; i < modList.Length; i++)
            {
                modList[i].transform.SetParent(mainScreen.transform, false);
            }

            warnText = CreateText("WarningText", "Deleting a saved game <i>CANNOT</i> be undone!\nAre you sure?", Vector2.down * 5f, BaldiFonts.ComicSans24, TextAlignmentOptions.Top, new Vector2(300f, 150f), Color.red, false);
            StandardMenuButton yesButton = CreateTextButton(() => { currentConfirmAction(); }, "YesButton", "<b>YES", new Vector2(-75f, -100f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(82f, 32f), Color.black);
            StandardMenuButton noButton = CreateTextButton(() => { SwitchToMain(true); }, "NoButton", "<b>NO", new Vector2(75f, -100f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(82f, 32f), Color.black);

            warnText.transform.SetParent(warnScreen.transform, false);
            yesButton.transform.SetParent(warnScreen.transform, false);
            noButton.transform.SetParent(warnScreen.transform, false);

            SwitchToMain(false);
        }

        internal static void MenuHook(OptionsMenu __instance, CustomOptionsHandler handler)
        {
            if (MTM101BaldiDevAPI.SaveGamesHandler == SavedGameDataHandler.Modded)
            {
                handler.AddCategory<SaveManagerMenu>("Modded\nSaved Games");
            }
            if (MTM101BaldiDevAPI.HighscoreHandler == SavedGameDataHandler.Modded)
            {
                handler.AddCategory<ModdedHighscoreMenu>("Modded\nHighscores");
            }
        }
    }
}
