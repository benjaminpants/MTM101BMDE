using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Registers
{
    public class SkyboxMetadata : IMetadata<Cubemap>
    {
        private Cubemap _cubemap;
        public Cubemap value => _cubemap;

        public List<string> tags => _tags;
        List<string> _tags = new List<string>();

        public PluginInfo info => _info;
        private PluginInfo _info;

        private Color _lightColor;
        public Color lightColor => _lightColor;

        public SkyboxMetadata(PluginInfo info, Cubemap cubemap, Color lightColor)
        {
            _info = info;
            _cubemap = cubemap;
            _lightColor = lightColor;
        }
    }

    public class SkyboxMetaStorage : BasicMetaStorage<SkyboxMetadata, Cubemap>
    {
        public static SkyboxMetaStorage Instance => MTM101BaldiDevAPI.skyboxMeta;
    }
}
