using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static partial class ObjectCreators
    {
        static FieldInfo _character = AccessTools.Field(typeof(NPC), "character");
        static FieldInfo _navigator = AccessTools.Field(typeof(NPC), "navigator");
        static FieldInfo _iEntityTrigger = AccessTools.Field(typeof(Entity), "iEntityTrigger");
        static FieldInfo _minDistance = AccessTools.Field(typeof(PropagatedAudioManager), "minDistance");
        static FieldInfo _maxDistance = AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance");
        static FieldInfo _poster = AccessTools.Field(typeof(NPC), "poster");
        static FieldInfo _entity = AccessTools.Field(typeof(Navigator), "entity");
        static FieldInfo _collider = AccessTools.Field(typeof(Navigator), "collider");
        static FieldInfo _npc = AccessTools.Field(typeof(Looker), "npc");
        static FieldInfo _useHeatMap = AccessTools.Field(typeof(Navigator), "useHeatMap");
        public static T CreateNPC<T>(string name, Character character, PosterObject poster, bool hasLooker = true, bool usesHeatMap = false, bool hasTrigger = true, float minAudioDistance = 10f, float maxAudioDistance = 250f, RoomCategory[] spawnableRooms = null) where T : NPC
        {
            T newNpc = GameObject.Instantiate(MTM101BaldiDevAPI.AssetMan.Get<GameObject>("TemplateNPC")).AddComponent<T>();
            Entity npcEntity = newNpc.GetComponent<Entity>();
            newNpc.name = name;
            Navigator nav = newNpc.GetComponent<Navigator>();
            // initialize private fields for the npc
            _character.SetValue(newNpc,character);
            _navigator.SetValue(newNpc, nav);
            _poster.SetValue(newNpc, poster); // placeholder
            // set up proper sprite data
            newNpc.spriteBase = newNpc.transform.Find("SpriteBase").gameObject; //technically, yes, we could just use .GetChild(0), but I refer to it by name incase something changes to avoid grabbing the wrong thing
            newNpc.spriteRenderer = new SpriteRenderer[] { newNpc.spriteBase.transform.GetChild(0).GetComponent<SpriteRenderer>() };
            newNpc.baseTrigger = newNpc.GetComponents<CapsuleCollider>().Where(x => x.isTrigger).ToArray();
            newNpc.looker = newNpc.GetComponent<Looker>();
            newNpc.looker.enabled = hasLooker;
            _useHeatMap.SetValue(nav, usesHeatMap);
            _npc.SetValue(newNpc.looker, newNpc);
            if (spawnableRooms == null)
            {
                newNpc.spawnableRooms = new List<RoomCategory>() { RoomCategory.Hall };
            }
            else
            {
                newNpc.spawnableRooms = spawnableRooms.ToList();
            }
            newNpc.potentialRoomAssets = new WeightedRoomAsset[0];
            if (hasTrigger)
            {
                _iEntityTrigger.SetValue(npcEntity, new IEntityTrigger[] { newNpc });
            }
            PropagatedAudioManager audMan = newNpc.GetComponent<PropagatedAudioManager>();
            _minDistance.SetValue(audMan, minAudioDistance);
            _maxDistance.SetValue(audMan, maxAudioDistance);
            nav.npc = newNpc;
            _entity.SetValue(nav, npcEntity);
            _collider.SetValue(nav, newNpc.baseTrigger[0]);

            GameObject.DontDestroyOnLoad(newNpc.gameObject);
            return newNpc;
        }
    }
}
