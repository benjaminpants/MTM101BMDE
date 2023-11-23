using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Reflection
{
    public static class ReflectionHelpers
    {
        public static void ReflectionSetVariable(this object me, string name, object setTo)
        {
            AccessTools.Field(me.GetType(), name).SetValue(me, setTo);
        }

        public static object ReflectionGetVariable(this object me, string name)
        {
            return AccessTools.Field(me.GetType(), name).GetValue(me);
        }
    }
}
