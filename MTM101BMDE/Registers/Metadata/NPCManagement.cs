using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace MTM101BaldAPI.Registers
{
    [Flags]
    public enum NPCFlags //these are heavily inspired by the system the mod menu uses, I'm not sure how many of these flags will be useful.
    {
        /// <summary>
        /// This NPC has no applicable flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// This NPC can respond to sound
        /// </summary>
        CanHear = 1,
        /// <summary>
        /// This NPC can move linearly (Bully doesn't count)
        /// </summary>
        CanMove = 2,
        /// <summary>
        /// This NPC can see/spot things
        /// </summary>
        CanSee = 4,
        /// <summary>
        /// This NPC can make noise that Baldi can hear (First Prize)
        /// </summary>
        MakeNoise = 8,
        /// <summary>
        /// This NPC's collider is a trigger (this excludes Chalkles and Bully)
        /// </summary>
        HasTrigger = 16,
        /// <summary>
        /// This NPC has a sprite in the schoolhouse, every NPC including Chalkles currently has this flag.
        /// </summary>
        HasSprite = 32,
        /// <summary>
        /// This NPC can block other NPC's LOS or Navigation
        /// </summary>
        IsBlockade = 64,
        /// <summary>
        /// This NPC is spawned via other, non level generator/builder means (such as a StructureBuilder).
        /// EX: Student
        /// </summary>
        NonStandardSpawn = 128,
        Standard =  CanMove | CanSee | HasSprite | HasTrigger,
        StandardNoCollide = Standard & ~HasTrigger,
        StandardAndHear = Standard | CanHear,
    }

    public class NPCMetadata : IMetadata<NPC>
    {
        public NPCFlags flags;
        public string nameLocalizationKey;
        public Character character => value.Character;

        public Dictionary<string, NPC> prefabs;
        private string defaultKey;

        public NPC value => prefabs[defaultKey];

        public HashSet<string> tags => _tags;
        private HashSet<string> _tags = new HashSet<string>();

        public PluginInfo info => _info;
        private PluginInfo _info;

        public NPCMetadata(PluginInfo info, NPC[] prefabs, string defKey, NPCFlags flags = NPCFlags.Standard)
        {
            _info = info;
            this.prefabs = new Dictionary<string, NPC>();
            for (int i = 0; i < prefabs.Length; i++)
            {
                this.prefabs[prefabs[i].name] = prefabs[i];
            }
            defaultKey = defKey;
            this.flags = flags;
            string e = EnumExtensions.GetExtendedName<Character>((int)value.Character);
            try
            {
                if (value.Poster != null)
                {
                    e = Singleton<LocalizationManager>.Instance.GetLocalizedText(value.Poster.textData[0].textKey);
                }
            }
            catch (Exception E)
            {
                MTM101BaldiDevAPI.Log.LogWarning("Unable to get localized text for: " + defKey + "\n" + E.Message);
            }
            nameLocalizationKey = e;
        }

        public NPCMetadata(PluginInfo info, NPC[] prefabs, string defKey, NPCFlags flags, string[] tags) : this(info, prefabs, defKey, flags)
        {
            _tags.UnionWith(tags);
        }
    }

    public class NPCMetaStorage : MetaStorage<Character, NPCMetadata, NPC>
    {
        public static NPCMetaStorage Instance => MTM101BaldiDevAPI.npcMetadata;

        /// <summary>
        /// Replaces all references to a specific NPC with a script attached of the same type. Useful if you replace an NPC with a custom type.
        /// </summary>
        /// <typeparam name="TOld"></typeparam>
        /// <typeparam name="TReplace"></typeparam>
        /// <param name="character"></param>
        public void ReplaceAllReferencesForCharacter<TOld, TReplace>(Character character) where TOld : NPC
            where TReplace : TOld
        {
            string[] keys = metas[character].prefabs.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                NPC val = metas[character].prefabs[keys[i]];
                TReplace neww = val.GetComponent<TReplace>();
                if (neww == null) continue;
                metas[character].prefabs[keys[i]] = neww;
                GameObject.Destroy(val);
            }
        }

        public override void Add(NPCMetadata toAdd)
        {
            metas.Add(toAdd.character, toAdd);
        }

        public bool AddPrefab(NPC toAdd)
        {
            NPCMetadata md = Get(toAdd);
            if (md == null) return false;
            md.prefabs[toAdd.name] = toAdd;
            return true;
        }

        public bool RemovePrefab(NPC toRemove)
        {
            NPCMetadata md = Get(toRemove);
            if (md == null) return false;
            return md.prefabs.Remove(md.prefabs.FirstOrDefault(x => x.Value == toRemove).Key);
        }

        public override NPCMetadata Get(NPC npc)
        {
            return Get(npc.Character);
        }
    }
}
