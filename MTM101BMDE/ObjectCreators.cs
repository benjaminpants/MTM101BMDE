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
	public static partial class ObjectCreators
	{

        public static ItemObject CreateItemObject(string localizedtext, string desckey, Sprite smallicon, Sprite largeicon, Items type, int price, int generatorcost)
		{
			ItemObject obj = ScriptableObject.CreateInstance<ItemObject>();
			obj.nameKey = localizedtext;
			obj.itemSpriteSmall = smallicon;
			obj.itemSpriteLarge = largeicon;
			obj.itemType = type;
			obj.descKey = desckey;
			obj.cost = generatorcost;
			obj.price = price;
			obj.name = localizedtext;

			return obj;
		}

		public static StandardDoorMats CreateDoorDataObject(string name, Texture2D openTex, Texture2D closeTex)
		{
			StandardDoorMats template = MTM101BaldiDevAPI.AssetMan.Get<StandardDoorMats>("DoorTemplate");
			StandardDoorMats mat = ScriptableObject.CreateInstance<StandardDoorMats>();
            mat.open = new Material(template.open);
            mat.open.SetMainTexture(openTex);
            mat.shut = new Material(template.shut);
            mat.shut.SetMainTexture(closeTex);
			mat.name = name;


            return mat;

        }

		public static WindowObject CreateWindowObject(string name, Texture2D texture, Texture2D brokenTexture, Texture2D mask = null)
		{
			WindowObject obj = ScriptableObject.CreateInstance<WindowObject>();
			WindowObject template = MTM101BaldiDevAPI.AssetMan.Get<WindowObject>("WindowTemplate");
			obj.name = name;
			if (mask != null)
			{
				Material maskMat = new Material(template.mask);
				maskMat.SetMaskTexture(mask);
				obj.mask = maskMat;
			}
			else
			{
				obj.mask = template.mask;
			}
			Material standMat = new Material(template.overlay.First());
			standMat.SetMainTexture(texture);
			obj.overlay = new Material[] { standMat, standMat };
            Material BrokeMat = new Material(template.open.First());
            BrokeMat.SetMainTexture(texture);
            obj.open = new Material[] { BrokeMat, BrokeMat };
			obj.windowPre = template.windowPre;

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
			obj.name = subtitle;
			return obj;

		}


		public static FieldTripObject CreateFieldTripObject(FieldTrips trip, FieldTripManager manager, string messageendkey, string animation)
		{
			FieldTripObject obj = ScriptableObject.CreateInstance<FieldTripObject>();
			obj.trip = trip;
			obj.tripPre = manager;
			obj.messageKey = messageendkey;
			obj.animation = animation;
			obj.name = EnumExtensions.GetExtendedName<FieldTrips>((int)trip);
			return obj;

		}

        public static PosterObject CreatePosterObject(Texture2D postertex, PosterTextData[] text)
        {
            PosterObject obj = ScriptableObject.CreateInstance<PosterObject>();
            obj.baseTexture = postertex;
            obj.textData = text;
            obj.name = postertex.name + "Poster";

            return obj;
        }

        public static PosterObject CreatePosterObject(Texture2D[] postertexs)
        {
			if (postertexs.Length == 0) throw new ArgumentNullException();
            PosterObject obj = ScriptableObject.CreateInstance<PosterObject>();
			obj.textData = new PosterTextData[0];
			if (postertexs.Length == 1)
			{
                obj.name = postertexs[0].name + "Poster";
				obj.baseTexture = postertexs.First();
            }
			else
			{
				List<PosterObject> otherPosters = new List<PosterObject>();
				for (int i = 0; i < postertexs.Length; i++)
				{
					otherPosters.Add(CreatePosterObject(postertexs[i],new PosterTextData[0]));
				}
				obj.multiPosterArray = otherPosters.ToArray();
                obj.name = postertexs[0].name + "PosterChain";
            }

            return obj;
        }
    }
}
