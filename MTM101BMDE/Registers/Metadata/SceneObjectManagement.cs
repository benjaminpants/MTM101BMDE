using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public class SceneObjectMetadata : IMetadata<SceneObject>
    {
        public SceneObject value => _value;
        private SceneObject _value;

        public bool randomlyGenerated => _value.levelObject != null;
        public int number => _value.levelNo;
        public string title => _value.levelTitle;

        public List<string> tags => _tags;
        private List<string> _tags = new List<string>();

        public PluginInfo info => _info;
        private PluginInfo _info;

        public SceneObjectMetadata(PluginInfo info, SceneObject obj)
        {
            _info = info;
            _value = obj;
        }
    }

    public class SceneObjectMetaStorage : BasicMetaStorage<SceneObjectMetadata, SceneObject>
    {
        public static SceneObjectMetaStorage Instance => MTM101BaldiDevAPI.sceneMeta;
    }
}
