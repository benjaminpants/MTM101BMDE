using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.OptionsAPI
{
    public static class OptionsExtensions
    {
        static FieldInfo _val = AccessTools.Field(typeof(AdjustmentBars), "val");

        public static int GetRaw(this AdjustmentBars b)
        {
            return (int)_val.GetValue(b);
        }
    }
}
