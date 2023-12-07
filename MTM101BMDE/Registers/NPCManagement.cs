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
        None = 0, // This NPC has no necessary flags.
        CanHear = 1, // This NPC can respond to sound
        CanMove = 2, // This NPC can move linearly (Bully doesn't count)
        CanSee = 4, // This NPC can see/spot things
        MakeNoise = 8, // This NPC can make noise that Baldi can hear (First Prize)
        HasTrigger = 16, // This NPC's collider is a trigger (this excludes Chalkles and Bully)
        HasPhysicalAppearence = 32, // This NPC has a physical appearence in the schoolhouse. Currently everybody has this flag
        IsBlockade = 64, // This NPC can block other NPC's LOS or Navigation
        Standard =  CanMove | CanSee | HasPhysicalAppearence | HasTrigger,
        StandardNoCollide = Standard & ~HasTrigger,
        StandardAndHear = Standard | CanHear
    }

    public class NPCMetadata : IMetadata<NPC>
    {
        public NPCFlags flags;
        public Character character => value.Character;

        public Dictionary<string, NPC> prefabs;
        private string defaultKey;

        public NPC value => prefabs[defaultKey];

        public List<string> tags => _tags;
        private List<string> _tags = new List<string>();

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

        public override NPCMetadata Get(NPC npc)
        {
            return Get(npc.Character);
        }
    }
}
