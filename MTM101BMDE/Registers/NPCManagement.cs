using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class NPCMetaStorage : IMetadataStorage<NPCMetadata, Character, NPC>
    {
        public static NPCMetaStorage Instance => MTM101BaldiDevAPI.npcMetadata;

        private Dictionary<Character, NPCMetadata> npcs = new Dictionary<Character, NPCMetadata>();

        public void Add(NPCMetadata toAdd)
        {
            npcs.Add(toAdd.character, toAdd);
        }

        public bool AddPrefab(NPC toAdd)
        {
            NPCMetadata md = Get(toAdd);
            if (md == null) return false;
            md.prefabs[toAdd.name] = toAdd;
            return true;
        }

        public NPCMetadata[] All()
        {
            return npcs.Values.ToArray();
        }

        public NPCMetadata Find(Predicate<NPCMetadata> predicate)
        {
            return FindAll(predicate).First();
        }

        public NPCMetadata[] FindAll(Predicate<NPCMetadata> predicate)
        {
            return npcs.Values.ToList().FindAll(predicate).ToArray();
        }

        public NPCMetadata[] FindAllWithTags(string[] tags, bool matchAll)
        {
            return FindAll(x =>
            {
                foreach (string tag in x.tags)
                {
                    // if it contains the tag and we don't need to match all, return true, otherwise continue past the return false
                    if (tags.Contains(tag))
                    {
                        if (!matchAll)
                        {
                            return true;
                        }
                        continue;
                    }
                    return false;
                }
                return true;
            });
        }

        public NPCMetadata Get(NPC npc)
        {
            return Get(npc.Character);
        }

        public NPCMetadata Get(Character key)
        {
            return npcs.GetValueSafe(key);
        }
    }
}
