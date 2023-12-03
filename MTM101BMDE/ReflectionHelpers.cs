using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Reflection
{
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Use sparingly, as these are not cached, and will waste memory if called constantly.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="name"></param>
        /// <param name="setTo"></param>
        public static void ReflectionSetVariable(this object me, string name, object setTo)
        {
            AccessTools.Field(me.GetType(), name).SetValue(me, setTo);
        }

        /// <summary>
        /// Use sparingly, as these are not cached, and will waste memory if called constantly.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object ReflectionGetVariable(this object me, string name)
        {
            return AccessTools.Field(me.GetType(), name).GetValue(me);
        }

        /// <summary>
        /// Use sparingly, as these are not cached, and will waste memory if called constantly.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ReflectionInvoke(this object me, string name, object[] parameters)
        {
            return AccessTools.Method(me.GetType(), name).Invoke(me, parameters);
        }
    }
}
