using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Net;
//BepInEx stuff
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using BepInEx.Configuration;
using System.Linq;
using System.Collections.Generic;

namespace MTM101BaldAPI
{
	public static class EnumExtensions
	{
		private struct ExtendedEnumData
		{
			public int valueOffset;
			public List<string> Enums;

            public ExtendedEnumData(int offset)
            {
                valueOffset = offset;
                Enums = new List<string>();
            }
		}

		private static Dictionary<Type, ExtendedEnumData> ExtendedData = new Dictionary<Type, ExtendedEnumData>();


        /// <summary>
        /// Extends an enum, same effect could be achieved by casting an int, however this has a system to keep track of multiple enum additions from different mods to prevent conflicts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendName"></param>
        /// <returns></returns>
		public static T ExtendEnum<T>(string extendName) where T : Enum
		{
			if (!ExtendedData.TryGetValue(typeof(T), out ExtendedEnumData dat))
			{
				dat = new ExtendedEnumData(256); //Just so nothing conflicts and mods don't break when the game updates. If this becomes problematic let me know, I'll suffer but I will try to fix it.
                //dat.valueOffset = Enum.GetNames(typeof(T)).Length - 1;
                ExtendedData.Add(typeof(T),dat);
            }
            dat.Enums.Add(extendName);
            return (T)(object)(dat.valueOffset + (dat.Enums.Count - 1));
        }

        /// <summary>
        /// Enum.GetName but with support for extended enums.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
		public static string GetExtendedName<T>(int val) where T : Enum
		{
			string theName = Enum.GetName(typeof(T), val);
            if (theName == null)
			{
                if (ExtendedData.TryGetValue(typeof(T), out ExtendedEnumData dat))
                {
                    return dat.Enums[val - dat.valueOffset];
                }
				else
				{
					return val.ToString();
				}
            }
			return theName;

        }

        public static T GetFromExtendedName<T>(string name) where T : Enum
        {
            if (Enum.IsDefined(typeof(T), name))
            {
                return (T)Enum.Parse(typeof(T), name);
            }
            bool success = ExtendedData.TryGetValue(typeof(T),out ExtendedEnumData value);
            if (!success)
            {
                throw new KeyNotFoundException();
            }
            int index = value.Enums.FindIndex(x => x == name);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }
            return (T)(object)(value.valueOffset + index);
        }

	}
}
