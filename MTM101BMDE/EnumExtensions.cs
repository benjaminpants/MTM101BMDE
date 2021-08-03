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
		private readonly static int amountofitems = Enum.GetNames(typeof(Items)).Length;

		private readonly static int amountofcharacters = Enum.GetNames(typeof(Character)).Length;

		private readonly static int amountofobstacles = Enum.GetNames(typeof(Obstacle)).Length;

		private readonly static int amountofevents = Enum.GetNames(typeof(RandomEventType)).Length;

		private readonly static int amountofroomcats = Enum.GetNames(typeof(RoomCategory)).Length;

		private static Dictionary<string, int> ItemExtensions = new Dictionary<string, int>();

		private static Dictionary<string, int> CharacterExtensions = new Dictionary<string, int>();

		private static Dictionary<string, int> ObstacleExtensions = new Dictionary<string, int>();

		private static Dictionary<string, int> EventExtensions = new Dictionary<string, int>();

		private static Dictionary<string, int> RoomCatExtensions = new Dictionary<string, int>();


		public static Items CreateItemEnum(string name)
		{
			int value = amountofitems + ItemExtensions.Count;
			ItemExtensions.Add(name, value);
			return (Items)value;
		}

		public static string GetItemName(Items num)
		{
			string name = num.ToString();

			if (((int)num).ToString() == name)
			{
				foreach (KeyValuePair<string, int> kvp in ItemExtensions)
				{
					if (kvp.Value == (int)num) return kvp.Key;
				}
			}
			return name;
		}

		public static RoomCategory CreateRoomCategoryEnum(string name)
		{
			int value = amountofroomcats + RoomCatExtensions.Count;
			RoomCatExtensions.Add(name, value);
			return (RoomCategory)value;
		}

		public static string GetRoomCategoryName(RoomCategory num)
		{
			string name = num.ToString();

			if (((int)num).ToString() == name)
			{
				foreach (KeyValuePair<string, int> kvp in RoomCatExtensions)
				{
					if (kvp.Value == (int)num) return kvp.Key;
				}
			}
			return name;
		}

		public static RandomEventType CreateEventEnum(string name)
		{
			int value = amountofevents + EventExtensions.Count;
			EventExtensions.Add(name, value);
			return (RandomEventType)value;
		}

		public static string GetEventName(RandomEventType num)
		{
			string name = num.ToString();

			if (((int)num).ToString() == name)
			{
				foreach (KeyValuePair<string, int> kvp in EventExtensions)
				{
					if (kvp.Value == (int)num) return kvp.Key;
				}
			}
			return name;
		}

		public static Character CreateCharacterEnum(string name)
		{
			int value = amountofcharacters + CharacterExtensions.Count;
			CharacterExtensions.Add(name, value);
			return (Character)value;
		}

		public static string GetCharacterName(Character num)
		{
			string name = num.ToString();

			if (((int)num).ToString() == name)
			{
				foreach (KeyValuePair<string, int> kvp in CharacterExtensions)
				{
					if (kvp.Value == (int)num) return kvp.Key;
				}
			}
			return name;
		}

		public static Obstacle CreateObstacleEnum(string name)
		{
			int value = amountofobstacles + ObstacleExtensions.Count;
			ObstacleExtensions.Add(name, value);
			return (Obstacle)value;
		}

		public static string GetObstacleName(Obstacle num)
		{
			string name = num.ToString();

			if (((int)num).ToString() == name)
			{
				foreach (KeyValuePair<string, int> kvp in ObstacleExtensions)
				{
					if (kvp.Value == (int)num) return kvp.Key;
				}
			}
			return name;
		}

	}
}
