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

		[Obsolete("Please use ExtendEnum<Items>(string extendName) instead.")]
		public static Items CreateItemEnum(string name)
		{
			return ExtendEnum<Items>(name);
		}

        [Obsolete("Please use GetExtendedName<Items>(int val) instead.")]
        public static string GetItemName(Items num)
		{
			return GetExtendedName<Items>((int)num);
		}

        [Obsolete("Please use ExtendEnum<RoomCategory>(string extendName) instead.")]
        public static RoomCategory CreateRoomCategoryEnum(string name)
		{
            return ExtendEnum<RoomCategory>(name);
        }

        [Obsolete("Please use GetExtendedName<RoomCategory>(int val) instead.")]
        public static string GetRoomCategoryName(RoomCategory num)
		{
            return GetExtendedName<Items>((int)num);
        }

        [Obsolete("Please use ExtendEnum<RandomEventType>(string extendName) instead.")]
        public static RandomEventType CreateEventEnum(string name)
		{
            return ExtendEnum<RandomEventType>(name);
        }

        [Obsolete("Please use GetExtendedName<RandomEventType>(int val) instead.")]
        public static string GetEventName(RandomEventType num)
		{
            return GetExtendedName<RandomEventType>((int)num);
        }

        [Obsolete("Please use ExtendEnum<Character>(string extendName) instead.")]
        public static Character CreateCharacterEnum(string name)
		{
            return ExtendEnum<Character>(name);
        }

        [Obsolete("Please use GetExtendedName<Character>(int val) instead.")]
        public static string GetCharacterName(Character num)
		{
            return GetExtendedName<Character>((int)num);
        }

        [Obsolete("Please use ExtendEnum<Obstacle>(string extendName) instead.")]
        public static Obstacle CreateObstacleEnum(string name)
		{
            return ExtendEnum<Obstacle>(name);
        }

        [Obsolete("Please use GetExtendedName<Obstacle>(int val) instead.")]
        public static string GetObstacleName(Obstacle num)
		{
            return GetExtendedName<Character>((int)num);
        }

	}
}
