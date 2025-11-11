using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{

    [Flags]
    public enum StickerFlags
    {
        None = 0,
        AffectsLevelGeneration = 1,
        IsBonus = 2
    }

    public class StickerMetaData : IMetadata<ExtendedStickerData>
    {
        private ExtendedStickerData _value;
        public ExtendedStickerData value => _value;

        private HashSet<string> _tags = new HashSet<string>();
        public HashSet<string> tags => _tags;

        private PluginInfo _info;
        public PluginInfo info => _info;

        public Sticker type => _value.sticker;

        public StickerFlags flags = StickerFlags.None;

        public StickerMetaData(PluginInfo info, ExtendedStickerData sticker)
        {
            _value = sticker;
            _info = info;
            if (sticker.affectsLevelGeneration)
            {
                flags |= StickerFlags.AffectsLevelGeneration;
            }
        }
    }

    public class StickerMetaStorage : MetaStorage<Sticker, StickerMetaData, ExtendedStickerData>
    {
        public static StickerMetaStorage Instance => MTM101BaldiDevAPI.stickerMeta;

        public override void Add(StickerMetaData toAdd)
        {
            metas.Add(toAdd.type, toAdd);
        }

        public StickerMetaData AddSticker(PluginInfo info, ExtendedStickerData sticker)
        {
            StickerMetaData meta = new StickerMetaData(info, sticker);
            metas.Add(sticker.sticker, meta);
            return meta;
        }

        public override StickerMetaData Get(ExtendedStickerData value)
        {
            return metas[value.sticker];
        }
    }
}
