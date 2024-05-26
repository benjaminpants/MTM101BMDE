using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using TMPro;
using MTM101BaldAPI.Registers;

namespace MTM101BaldAPI
{
    public static class StringHelpers
    {

        public static TextLocalizer GetLocalizer(this TMP_Text text)
        {
            return text.GetComponent<TextLocalizer>();
        }

        [Obsolete("Please use ItemMetaStorage.Instance.FindByEnum(en).nameKey instead!")]
        public static string GetName(this Items en)
        {
            return ItemMetaStorage.Instance.FindByEnum(en).nameKey;
        }

        [Obsolete("Please use NPCMetaStorage.Instance.Get(en).nameLocalizationKey instead!")]
        public static string GetName(this Character en)
        {
            return NPCMetaStorage.Instance.Get(en).nameLocalizationKey;
        }
    }
}
