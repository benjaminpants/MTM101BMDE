using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    [Flags]
    public enum RandomEventFlags
    {
        None = 0,
        CharacterSpecific = 1, // This event requires a specific character to work
        RoomSpecific = 2, // This event requires a specific room to work
        Permanent = 4, // This event makes permanent changes to the level
    }


    public class RandomEventMetadata : IMetadata<RandomEvent>
    {
        public RandomEventFlags flags { private set; get; }

        private static FieldInfo foKey = AccessTools.Field(typeof(RandomEvent), "eventDescKey");
        private RandomEvent rEvent;
        public RandomEvent value => rEvent;

        public RandomEventType type => rEvent.Type;
        public string descKey => (string)foKey.GetValue(value);

        public List<string> tags => _tags;
        private List<string> _tags = new List<string>();

        public PluginInfo info => _info;
        private PluginInfo _info;

        public RandomEventMetadata(PluginInfo pInfo, RandomEvent randomEvent, RandomEventFlags flags = RandomEventFlags.None)
        {
            _info = pInfo;
            rEvent = randomEvent;
            this.flags = flags;
        }

        public Character[] GetRequiredCharacters()
        {
            List<Character> characters = new List<Character>();
            for (int i = 0; i < _tags.Count; i++)
            {
                string currentTag = _tags[i];
                if (currentTag.Contains("requiredC_"))
                {
                    characters.Add(EnumExtensions.GetFromExtendedName<Character>(currentTag.Replace("requiredC_","")));
                }
            }
            return characters.ToArray();
        }

        public RoomCategory[] GetRequiredRooms()
        {
            List<RoomCategory> rooms = new List<RoomCategory>();
            for (int i = 0; i < _tags.Count; i++)
            {
                string currentTag = _tags[i];
                if (currentTag.Contains("requiredR_"))
                {
                    rooms.Add(EnumExtensions.GetFromExtendedName<RoomCategory>(currentTag.Replace("requiredR_", "")));
                }
            }
            return rooms.ToArray();
        }

        public RandomEventMetadata(PluginInfo pInfo, RandomEvent randomEvent, Character[] requiredCharacters)
        {
            _info = pInfo;
            rEvent = randomEvent;
            flags = RandomEventFlags.CharacterSpecific;
            for (int i = 0; i < requiredCharacters.Length; i++)
            {
                _tags.Add("requiredC_" + requiredCharacters[i].ToStringExtended());
            }
        }

        public RandomEventMetadata(PluginInfo pInfo, RandomEvent randomEvent, RoomCategory[] requiredRooms)
        {
            _info = pInfo;
            rEvent = randomEvent;
            flags = RandomEventFlags.RoomSpecific;
            for (int i = 0; i < requiredRooms.Length; i++)
            {
                _tags.Add("requiredR_" + requiredRooms[i].ToStringExtended());
            }
        }
    }

    public class RandomEventMetaStorage : MetaStorage<RandomEventType, RandomEventMetadata, RandomEvent>
    {
        public static RandomEventMetaStorage Instance => MTM101BaldiDevAPI.rngEvStorage;

        public override void Add(RandomEventMetadata toAdd)
        {
            metas.Add(toAdd.type, toAdd);
        }

        public override RandomEventMetadata Get(RandomEvent value)
        {
            return Get(value.Type);
        }
    }
}
