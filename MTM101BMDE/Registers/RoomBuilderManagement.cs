using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    [Flags]
    public enum RoomBuilderFlags
    {
        None = 0,
        NonStandard = 1, // This room builder is non-standard and is not meant for special floors
        SpecialBuildersOnly = 2, // This room builder is meant specifically for special rooms.

    }

    public class RoomBuilderMeta : IMetadata<RoomBuilder>
    {
        public RoomBuilderFlags flags = RoomBuilderFlags.None;

        public RoomCategory roomCategory { private set; get; }

        private RoomBuilder _value;
        public RoomBuilder value => _value;

        private List<string> _tags = new List<string>();
        public List<string> tags => _tags;

        private PluginInfo _info;
        public PluginInfo info => _info;

        public RoomBuilderMeta(PluginInfo info, RoomBuilder builder, RoomCategory rmc)
        {
            _info = info;
            _value = builder;
            roomCategory = rmc;
        }
    }

    public class RoomBuilderMetaStorage : BasicMetaStorage<RoomBuilderMeta, RoomBuilder>
    {
        public static RoomBuilderMetaStorage Instance => MTM101BaldiDevAPI.roomBuilderStorage;

        public RoomBuilderMeta[] GetAll(RoomCategory cat)
        {
            return FindAll(x =>
            {
                return (x.roomCategory == cat && !x.flags.HasFlag(RoomBuilderFlags.NonStandard));
            });
        }

        public RoomBuilderMeta Get(RoomCategory cat)
        {
            RoomBuilderMeta firstAttempt = Find(x =>
            {
                return (x.roomCategory == cat && !x.flags.HasFlag(RoomBuilderFlags.NonStandard));
            });
            if (firstAttempt != null) return firstAttempt;
            return Find(x =>
            {
                return (x.roomCategory == cat);
            });
        }
    }
}
