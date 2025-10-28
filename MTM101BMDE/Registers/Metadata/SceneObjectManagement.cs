using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public class SceneObjectMetadata : IMetadata<SceneObject>
    {
        public SceneObject value => _value;
        private SceneObject _value;

        public bool randomlyGenerated {
            get
            {
                if (_value.levelObject != null)
                {
                    return true;
                }
                if (_value.randomizedLevelObject != null)
                {
                    return _value.randomizedLevelObject.Length > 0;
                }
                return false;
            }
        }
        public int number => _value.levelNo;
        public string title => _value.levelTitle;

        public HashSet<string> tags => _tags;
        private HashSet<string> _tags = new HashSet<string>();

        public PluginInfo info => _info;
        private PluginInfo _info;

        public SceneObjectMetadata(PluginInfo info, SceneObject obj)
        {
            _info = info;
            _value = obj;
        }

        /// <summary>
        /// Get the level types supported by this SceneObject.
        /// </summary>
        /// <returns></returns>
        public LevelType[] GetSupportedLevelTypes()
        {
            return value.GetCustomLevelObjects().Select(x => x.type).Distinct().ToArray();
        }
    }

    public class SceneObjectMetaStorage : BasicMetaStorage<SceneObjectMetadata, SceneObject>
    {
        public static SceneObjectMetaStorage Instance => MTM101BaldiDevAPI.sceneMeta;
    }
}
