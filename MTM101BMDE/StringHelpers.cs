using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace MTM101BaldAPI
{
    public static class StringHelpers
    {
        public static string GetName(this Items en)
        {
            string e = en.ToString();
            try
            {
                e = Singleton<LocalizationManager>.Instance.GetLocalizedText(en.GetFirstInstance().nameKey);
            }
            catch(Exception E)
            {
                Debug.LogWarning(E.Message);
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
                Debug.LogWarning(E.Message);
            }
            return e;
        }
    }
}
