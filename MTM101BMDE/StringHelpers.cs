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
            return Singleton<LocalizationManager>.Instance.GetLocalizedText(en.GetFirstInstance().nameKey);
        }

        public static string GetName(this Character en)
        {
            return Singleton<LocalizationManager>.Instance.GetLocalizedText(en.GetFirstInstance().Poster.textData[0].textKey);
        }
    }
}
