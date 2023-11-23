using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using TMPro;

namespace MTM101BaldAPI
{
    public static class StringHelpers
    {

        public static TextLocalizer GetLocalizer(this TMP_Text text)
        {
            return text.GetComponent<TextLocalizer>();
        }

        public static string GetName(this Items en)
        {
            string e = en.ToString();
            try
            {
                e = Singleton<LocalizationManager>.Instance.GetLocalizedText(en.GetFirstInstance().nameKey);
            }
            catch(Exception E)
            {
                MTM101BaldiDevAPI.Log.LogWarning(E.Message);
            }
            return e;
        }

        public static string GetName(this Character en)
        {
            string e = en.ToString();
            try
            {
                e = Singleton<LocalizationManager>.Instance.GetLocalizedText(en.GetFirstInstance().Poster.textData[0].textKey);
            }
            catch (Exception E)
            {
                MTM101BaldiDevAPI.Log.LogWarning(E.Message);
            }
            return e;
        }
    }
}
