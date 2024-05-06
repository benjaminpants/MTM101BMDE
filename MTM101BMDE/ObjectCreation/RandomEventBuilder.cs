using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class RandomEventBuilder<T> where T : RandomEvent
    {
        PluginInfo _info;
        RandomEventType _type;
        string _enumName = "";
        string _eventName = null;
        string _description = "Uh oh! Some event happened, but a description wasn't assigned!";
        float _minTime = 60f;
        float _maxTime = 60f;
        List<string> _tags = new List<string>();
        List<string> characters = new List<string>();
        RandomEventFlags _flags = RandomEventFlags.None;
        List<WeightedRoomAsset> potentialRoomAssets = new List<WeightedRoomAsset>();



        static FieldInfo _eventType = AccessTools.Field(typeof(RandomEvent), "eventType");
        static FieldInfo _eventDescKey = AccessTools.Field(typeof(RandomEvent), "eventDescKey");
        static FieldInfo _minEventTime = AccessTools.Field(typeof(RandomEvent), "minEventTime");
        static FieldInfo _maxEventTime = AccessTools.Field(typeof(RandomEvent), "maxEventTime");
        static FieldInfo _potentialRoomAssets = AccessTools.Field(typeof(RandomEvent), "potentialRoomAssets");
        public T Build()
        {
            GameObject eventObject = new GameObject();
            T evnt = eventObject.AddComponent<T>();
            RandomEventType type = _type;
            if (_enumName != "")
            {
                type = EnumExtensions.ExtendEnum<RandomEventType>(_enumName);
            }
            _eventType.SetValue(evnt, type);
            _eventDescKey.SetValue(evnt, _description);
            _minEventTime.SetValue(evnt, _minTime);
            _maxEventTime.SetValue(evnt, _maxTime);
            _potentialRoomAssets.SetValue(evnt, potentialRoomAssets.ToArray());
            eventObject.name = _eventName;
            RandomEventMetadata meta = new RandomEventMetadata(_info, evnt, _flags);
            meta.tags.AddRange(_tags);
            meta.tags.AddRange(characters);
            RandomEventMetaStorage.Instance.Add(meta);
            evnt.gameObject.ConvertToPrefab(true);
            return evnt;
        }

        public RandomEventBuilder(PluginInfo info)
        {
            _info = info;
        }

        public RandomEventBuilder<T> AddRequiredCharacter(Character character)
        {
            _flags |= RandomEventFlags.CharacterSpecific;
            characters.Add("requiredC_" + EnumExtensions.GetExtendedName<Character>((int)character));
            return this;
        }

        public RandomEventBuilder<T> SetMeta(RandomEventFlags flags, params string[] tags)
        {
            _flags = flags;
            if (characters.Count > 0)
            {
                _flags |= RandomEventFlags.CharacterSpecific;
            }
            _tags = tags.ToList();
            return this;
        }

        public RandomEventBuilder<T> SetDescription(string desc)
        {
            _description = desc;
            return this;
        }

        public RandomEventBuilder<T> AddRoomAsset(RoomAsset asset, int weight = 100)
        {
            potentialRoomAssets.Add(new WeightedRoomAsset()
            {
                selection = asset,
                weight = weight
            });
            return this;
        }

        public RandomEventBuilder<T> AddRoomAssets(params WeightedRoomAsset[] assets)
        {
            potentialRoomAssets.AddRange(assets);
            return this;
        }
        
        public RandomEventBuilder<T> SetName(string name)
        {
            _eventName = name;
            return this;
        }

        public RandomEventBuilder<T> SetMinMaxTime(float min, float max)
        {
            SetMinTime(min);
            SetMaxTime(max);
            return this;
        }

        void SetMinTime(float min)
        {
            _minTime = min;
            _maxTime = Math.Max(_maxTime, min);
        }

        void SetMaxTime(float max)
        {
            _maxTime = max;
            _minTime = Math.Min(_minTime, max);
        }

        public RandomEventBuilder<T> SetEnum(RandomEventType typ)
        {
            _type = typ;
            _enumName = "";
            return this;
        }

        public RandomEventBuilder<T> SetEnum(string enumToRegister)
        {
            _type = RandomEventType.Fog;
            _enumName = enumToRegister;
            return this;
        }
    }
}
