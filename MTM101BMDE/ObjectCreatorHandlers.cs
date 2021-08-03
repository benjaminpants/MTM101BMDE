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
	static class ObjectCreatorHandlers
	{
		public static ItemObject CreateObject(string localizedtext, string desckey, Sprite smallicon, Sprite largeicon, Items type, int price, int generatorcost)
		{
			ItemObject obj = ScriptableObject.CreateInstance<ItemObject>();
			obj.nameKey = localizedtext;
			obj.itemSpriteSmall = smallicon;
			obj.itemSpriteLarge = largeicon;
			obj.itemType = type;
			obj.descKey = desckey;
			obj.cost = generatorcost;
			obj.price = price;

			return obj;
		}

		public static SoundObject CreateSoundObject(AudioClip clip, string subtitle, SoundType type, Color color, float sublength = -1f)
		{
			SoundObject obj = ScriptableObject.CreateInstance<SoundObject>();
			obj.soundClip = clip;
			obj.subDuration = sublength == -1 ? clip.length + 1f : sublength;
			obj.soundType = type;
			obj.soundKey = subtitle;
			obj.color = color;
			return obj;

		}
	}
}
