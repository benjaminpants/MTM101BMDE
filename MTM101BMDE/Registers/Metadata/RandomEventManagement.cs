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
        /// <summary>
        /// This event requires a specific character to work
        /// </summary>
        CharacterSpecific = 1,
        /// <summary>
        /// This event requires a specific room to work
        /// </summary>
        RoomSpecific = 2,
        /// <summary>
        /// This event makes permanent changes to the level
        /// </summary>
        Permanent = 4,
        /// <summary>
        /// This event affects the generator in some way(adding a character or room)
        /// </summary>
        AffectsGenerator = 8,
        /// <summary>
        /// This event isn't called through regular means, and shouldn't be added to the random event pool.
        /// </summary>
        Special = 16
    }


    public class RandomEventMetadata : IMetadata<RandomEvent>
    {
        public RandomEventFlags flags { private set; get; }

        private static FieldInfo foKey = AccessTools.Field(typeof(RandomEvent), "eventIntro");
        private RandomEvent rEvent;
        public RandomEvent value => rEvent;

        public RandomEventType type => rEvent.Type;

        public SoundObject introSound => ((SoundObject)foKey.GetValue(value));

        public string descKey => introSound.soundKey;

        public HashSet<string> tags => _tags;
        private HashSet<string> _tags = new HashSet<string>();

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
            foreach (var currentTag in tags)
            {
                if (currentTag.Contains("requiredC_"))
                {
                    characters.Add(EnumExtensions.GetFromExtendedName<Character>(currentTag.Replace("requiredC_", "")));
                }
            }
            return characters.ToArray();
        }

        public RoomCategory[] GetRequiredRooms()
        {
            List<RoomCategory> rooms = new List<RoomCategory>();
            foreach (var currentTag in tags)
            {
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
        public static RandomEventMetaStorage Instance => MTM101BaldiDevAPI.randomEventStorage;

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
