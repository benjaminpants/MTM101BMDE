using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public class LevelTypeMeta : IMetadata<LevelType>
    {
        private LevelType _type;
        public LevelType value => _type;

        private HashSet<string> _tags = new HashSet<string>();
        public HashSet<string> tags => _tags;

        private PluginInfo _info;
        public PluginInfo info => _info;

        public PosterObject poster;

        public LevelTypeMeta(PluginInfo info, LevelType type, PosterObject poster)
        {
            _info = info;
            _type = type;
            this.poster = poster;
        }
    }

    public class LevelTypeMetaStorage : BasicMetaStorage<LevelTypeMeta, LevelType>
    {
        public static LevelTypeMetaStorage Instance => MTM101BaldiDevAPI.levelTypeMeta;

        public void AddMeta(PluginInfo info, LevelType enm, PosterObject poster)
        {
            Add(new LevelTypeMeta(info, enm, poster));
        }

        public void AddMeta(PluginInfo info, LevelType enm, string chalkboardKey)
        {
            Add(new LevelTypeMeta(info, enm, ObjectCreators.CreateLevelTypeChalkboard(chalkboardKey)));
        }
    }

    public static class LevelTypeExtensions
    {
        public static LevelTypeMeta GetMeta(this LevelType type)
        {
            return LevelTypeMetaStorage.Instance.Get(type);
        }
    }
}
