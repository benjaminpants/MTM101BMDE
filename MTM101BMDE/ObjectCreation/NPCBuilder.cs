using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class NPCBuilder<T> where T : NPC
    {
        PluginInfo info;
        public NPCBuilder(PluginInfo info)
        {
            this.info = info;
        }


        static FieldInfo _character = AccessTools.Field(typeof(NPC), "character");
        static FieldInfo _ignorePlayerOnSpawn = AccessTools.Field(typeof(NPC), "ignorePlayerOnSpawn");
        static FieldInfo _ignoreBelts = AccessTools.Field(typeof(NPC), "ignoreBelts");
        static FieldInfo _navigator = AccessTools.Field(typeof(NPC), "navigator");
        static FieldInfo _iEntityTrigger = AccessTools.Field(typeof(Entity), "iEntityTrigger");
        static FieldInfo _minDistance = AccessTools.Field(typeof(PropagatedAudioManager), "minDistance");
        static FieldInfo _maxDistance = AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance");
        static FieldInfo _poster = AccessTools.Field(typeof(NPC), "poster");
        static FieldInfo _entity = AccessTools.Field(typeof(Navigator), "entity");
        static FieldInfo _collider = AccessTools.Field(typeof(Navigator), "collider");
        static FieldInfo _npc = AccessTools.Field(typeof(Looker), "npc");
        static FieldInfo _useHeatMap = AccessTools.Field(typeof(Navigator), "useHeatMap");


        string objectName = "Unnamed Character";
        Character characterEnum = Character.Null;
        string characterEnumName = "";
        PosterObject poster;
        Texture2D posterTexture = MTM101BaldiDevAPI.AssetMan.Get<PosterObject>("CharacterPosterTemplate").baseTexture;
        string[] posterData = new string[]
        {
            "Unnamed",
            "Unnamed and unloved, the developer forgot to give this character a poster."
        };
        bool hasLooker = false;
        bool hasTrigger = false;
        bool useHeatmap = false;
        bool ignorePlayerOnSpawn = false;
        bool ignoreBelts = false;
        bool grounded = true;
        float minAudioDistance = 10f;
        float maxAudioDistance = 250f;
        NPCFlags flags = NPCFlags.HasPhysicalAppearence | NPCFlags.CanMove;
        string[] tags = new string[0];
        List<RoomCategory> spawnableRooms = new List<RoomCategory>();
        List<WeightedRoomAsset> potentialRoomAssets = new List<WeightedRoomAsset>();

        public T Build()
        {
            T newNpc = GameObject.Instantiate(MTM101BaldiDevAPI.AssetMan.Get<GameObject>("TemplateNPC")).AddComponent<T>();
            Entity npcEntity = newNpc.GetComponent<Entity>();
            newNpc.name = objectName;
            Navigator nav = newNpc.GetComponent<Navigator>();
            // initialize private fields for the npc
            Character character = characterEnum;
            if (characterEnumName != "")
            {
                character = EnumExtensions.ExtendEnum<Character>(characterEnumName);
            }
            _character.SetValue(newNpc, character);
            _navigator.SetValue(newNpc, nav);
            if (!flags.HasFlag(NPCFlags.CanMove))
            {
                nav.enabled = false;
            }
            if (poster == null)
            {
                poster = ObjectCreators.CreateCharacterPoster(posterTexture, posterData[0], posterData[1]);
            }
            _poster.SetValue(newNpc, poster);
            // set up proper sprite data
            if (flags.HasFlag(NPCFlags.HasPhysicalAppearence))
            {
                newNpc.spriteBase = newNpc.transform.Find("SpriteBase").gameObject; //technically, yes, we could just use .GetChild(0), but I refer to it by name incase something changes to avoid grabbing the wrong thing
                newNpc.spriteRenderer = new SpriteRenderer[] { newNpc.spriteBase.transform.GetChild(0).GetComponent<SpriteRenderer>() };
            }
            else
            {
                newNpc.spriteBase = null;
                newNpc.spriteRenderer = new SpriteRenderer[] { };
            }
            newNpc.baseTrigger = newNpc.GetComponents<CapsuleCollider>().Where(x => x.isTrigger).ToArray();
            newNpc.looker = newNpc.GetComponent<Looker>();
            newNpc.looker.enabled = hasLooker;
            if (hasLooker)
            {
                flags |= NPCFlags.CanSee;
            }
            _useHeatMap.SetValue(nav, useHeatmap);
            _npc.SetValue(newNpc.looker, newNpc);
            if (spawnableRooms.Count == 0)
            {
                newNpc.spawnableRooms = new List<RoomCategory>() { RoomCategory.Hall };
            }
            else
            {
                newNpc.spawnableRooms = spawnableRooms.ToList();
            }
            newNpc.potentialRoomAssets = potentialRoomAssets.ToArray();
            if (hasTrigger)
            {
                _iEntityTrigger.SetValue(npcEntity, new IEntityTrigger[] { newNpc });
                flags |= NPCFlags.HasTrigger;
            }
            PropagatedAudioManager audMan = newNpc.GetComponent<PropagatedAudioManager>();
            _minDistance.SetValue(audMan, minAudioDistance);
            _maxDistance.SetValue(audMan, maxAudioDistance);
            nav.npc = newNpc;
            _entity.SetValue(nav, npcEntity);
            _collider.SetValue(nav, newNpc.baseTrigger[0]);
            _ignoreBelts.SetValue(newNpc, ignoreBelts);
            _ignorePlayerOnSpawn.SetValue(newNpc, ignorePlayerOnSpawn);
            npcEntity.SetGrounded(grounded);

            GameObject.DontDestroyOnLoad(newNpc.gameObject);

            NPCMetadata meta = newNpc.AddMeta(info.Instance, flags);
            meta.tags.AddRange(tags);

            return newNpc;
        }

        public NPCBuilder<T> SetName(string name)
        {
            objectName = name;
            return this;
        }

        public NPCBuilder<T> SetEnum(Character character)
        {
            characterEnum = character;
            return this;
        }

        public NPCBuilder<T> SetEnum(string enumName)
        {
            characterEnum = Character.Null;
            characterEnumName = enumName;
            return this;
        }

        public NPCBuilder<T> SetPoster(PosterObject poster)
        {
            this.poster = poster;
            return this;
        }

        public NPCBuilder<T> AddMetaFlag(NPCFlags flag)
        {
            flags |= flag;
            return this;
        }

        public NPCBuilder<T> SetStationary()
        {
            flags &= NPCFlags.CanMove;
            return this;
        }

        public NPCBuilder<T> SetMetaTags(string[] tags)
        {
            this.tags = tags;
            return this;
        }

        public NPCBuilder<T> RemoveSprite()
        {
            flags &= NPCFlags.HasPhysicalAppearence;
            return this;
        }

        public NPCBuilder<T> SetPoster(Texture2D texture, string posterTitle, string posterDescription)
        {
            poster = null;
            posterTexture = texture;
            posterData = new string[] { posterTitle, posterDescription };
            return this;
        }

        public NPCBuilder<T> AddLooker()
        {
            hasLooker = true;
            return this;
        }

        public NPCBuilder<T> AddTrigger()
        {
            hasTrigger = true;
            return this;
        }

        public NPCBuilder<T> IgnorePlayerOnSpawn()
        {
            ignorePlayerOnSpawn = true;
            return this;
        }

        public NPCBuilder<T> IgnoreBelts()
        {
            ignoreBelts = true;
            return this;
        }

        public NPCBuilder<T> SetAirborne()
        {
            grounded = false;
            return this;
        }

        public NPCBuilder<T> AddHeatmap()
        {
            useHeatmap = true;
            return this;
        }

        public NPCBuilder<T> AddSpawnableRoomCategories(params RoomCategory[] categories)
        {
            spawnableRooms.AddRange(categories);
            return this;
        }

        public NPCBuilder<T> SetMinMaxAudioDistance(float min, float max)
        {
            minAudioDistance = min;
            maxAudioDistance = max;
            return this;
        }

        public NPCBuilder<T> AddPotentialRoomAsset(RoomAsset asset, int weight)
        {
            potentialRoomAssets.Add(new WeightedRoomAsset()
            {
                selection=asset,
                weight=weight
            });
            return this;
        }

        public NPCBuilder<T> AddPotentialRoomAssets(params WeightedRoomAsset[] assets)
        {
            potentialRoomAssets.AddRange(assets);
            return this;
        }
    }
}
