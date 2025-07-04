using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;
using MTM101BaldAPI.SaveSystem;
using UnityEngine.Events;
using MTM101BaldAPI.Registers;

namespace MTM101BaldAPI
{
    internal class ModdedHighscoreMenu : CustomOptionsCategory
    {
        GameObject mainScreen;
        GameObject warnScreen;
        TextMeshProUGUI warnText;
        TextMeshProUGUI countText;
        Action currentConfirmAction;

        public void SwitchToMain()
        {
            warnScreen.SetActive(false);
            mainScreen.SetActive(true);
            UpdateText();
        }

        public void SwitchToWarning(string textToDisplay, Action onConfirm)
        {
            warnScreen.SetActive(true);
            mainScreen.SetActive(false);
            warnText.text = "<b>WARNING!</b>\n" + textToDisplay;
            currentConfirmAction = onConfirm;
        }

        public void UpdateText()
        {
            countText.text = "Total Scores: " + ModdedHighscoreManager.moddedScores.Count + "\nActive Scores: " + ModdedHighscoreManager.activeModdedScores.Count;
        }

        void Awake()
        {
            UpdateText();
        }

        void ResetAllScores()
        {
            ModdedHighscoreManager.moddedScores.Clear();
            ModdedHighscoreManager.UpdateActiveScores();
            ModdedHighscoreManager.Save();
            ModdedHighscoreManager.UpdateRealHighscoreManager();
            SwitchToMain();
        }

        void ResetAllNonActiveScores()
        {
            ModdedHighscoreManager.moddedScores.Clear();
            ModdedHighscoreManager.moddedScores.AddRange(ModdedHighscoreManager.activeModdedScores);
            ModdedHighscoreManager.Save();
            ModdedHighscoreManager.UpdateRealHighscoreManager();
            SwitchToMain();
        }

        public override void Build()
        {
            mainScreen = new GameObject("MainScreen");
            mainScreen.transform.SetParent(transform, false);
            warnScreen = new GameObject("WarnScreen");
            warnScreen.transform.SetParent(transform, false);
            warnScreen.SetActive(false);

            warnText = CreateText("WarningText", "Texty text text!", Vector2.down * 5f, BaldiFonts.ComicSans24, TextAlignmentOptions.Top, new Vector2(300f, 150f), Color.red, false);
            StandardMenuButton yesButton = CreateTextButton(() => { currentConfirmAction(); }, "YesButton", "<b>YES", new Vector2(-75f, -100f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(82f, 32f), Color.black);
            StandardMenuButton noButton = CreateTextButton(() => { SwitchToMain(); }, "NoButton", "<b>NO", new Vector2(75f, -100f), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(82f, 32f), Color.black);

            warnText.transform.SetParent(warnScreen.transform, false);
            yesButton.transform.SetParent(warnScreen.transform, false);
            noButton.transform.SetParent(warnScreen.transform, false);

            countText = CreateText("CountText", "Total Scores: 4763\nActive Scores: 324", Vector2.up * 20f, BaldiFonts.ComicSans24, TextAlignmentOptions.Top, new Vector2(300f, 100f), Color.black, false);
            countText.transform.SetParent(mainScreen.transform, false);

            StandardMenuButton clearAllScores = CreateTextButton(() => { SwitchToWarning("This will delete ALL of your scores across all modded instances! Are you sure you want to do this?", ResetAllScores); }, "ClearAllScores", "Delete <b>ALL</b> Scores", Vector2.down * 25f, BaldiFonts.ComicSans18, TextAlignmentOptions.Center, new Vector2(200f,18f), Color.black);
            clearAllScores.transform.SetParent(mainScreen.transform, false);

            StandardMenuButton clearAllScoresForNonActive = CreateTextButton(() => { SwitchToWarning("This will delete all of your scores outside this current session! Are you sure?", ResetAllNonActiveScores); }, "ClearNonActiveScores", "Delete <b>Non-Active</b> Scores", Vector2.down * 75f, BaldiFonts.ComicSans18, TextAlignmentOptions.Center, new Vector2(300f, 18f), Color.black);
            clearAllScoresForNonActive.transform.SetParent(mainScreen.transform, false);
        }
    }
}
