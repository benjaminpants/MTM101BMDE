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
    }
}
