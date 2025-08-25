using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public enum GenerationModType
    {
        /// <summary>
        /// Runs before everything else, including base. This is the only time a new level type can be added or a level type can be removed from a SceneObject.
        /// Attempting it in any other GenerationModType will throw an error.
        /// </summary>
        Preparation,

        /// <summary>
        /// This should be used for methods that override the majority of the generator properties, almost completely transforming the level.
        /// </summary>
        Base,

        /// <summary>
        /// This should be used for overriding only certain properties of the generator, such as changing exit counts.
        /// </summary>
        Override,

        /// <summary>
        /// This should be used for adding onto already existing properties, such as adding characters or items. Most mods will be using this.
        /// </summary>
        Addend,

        /// <summary>
        /// Useful for removing things that might've been added or changed by other mods, or if for one reason or another you need the final say on something. 
        /// <c>Use with caution.</c>
        /// </summary>
        Finalizer
    }


    /// <summary>
    /// A helper class used to store fieldtrip loot during the mod loading phase.
    /// </summary>
    public class FieldTripLoot
    {
        public List<WeightedItemObject> potentialItems;
        public List<ItemObject> guaranteedItems;
    }

    /// <summary>
    /// A helper class used for easy modification of WeightedItemObject arrays and lists during the mod loading phase.
    /// </summary>
    public class CustomLoot
    {
        public List<WeightedItemObject> potentialItems;
    }

    public enum DefaultCustomLoot
    {
        CrazyVendingMachines,
        MysteryRoom,
        PartyEvent
    }

    public static class GeneratorManagement
    {
        private static Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, int, SceneObject>>> generationStuff = new Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, int, SceneObject>>>();

        internal static GenerationStageFlags currentGenerationStepFlags = GenerationStageFlags.Invalid;

        internal static List<SceneObject> queuedModdedScenes = new List<SceneObject>();

        /// <summary>
        /// Register a generator action, called during mod loading.
        /// </summary>
        /// <param name="plug">The plugin adding the generator modifiers.</param>
        /// <param name="type"></param>
        /// <param name="action">The first parameter is the level name, the second one is the level id, and the last is the SceneObject itself.</param>
        public static void Register(BaseUnityPlugin plug, GenerationModType type, Action<string, int, SceneObject> action)
        {
            if (!generationStuff.ContainsKey(plug))
            {
                generationStuff.Add(plug, new Dictionary<GenerationModType, Action<string, int, SceneObject>>());
            }
            generationStuff[plug].Add(type, action);
        }

        internal static Dictionary<BaseUnityPlugin, Action<FieldTrips, FieldTripLoot>> fieldtripLootChanges = new Dictionary<BaseUnityPlugin, Action<FieldTrips, FieldTripLoot>>();

        static readonly FieldInfo _potentialItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "potentialItems");
        static readonly FieldInfo _guaranteedItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "guaranteedItems");
        /// <summary>
        /// Invoke FieldTrip loot table changes for the specified field trip.
        /// </summary>
        /// <param name="trip"></param>
        public static void InvokeFieldTripLootChange(FieldTripObject trip)
        {
            FieldTripBaseRoomFunction roomFunction = trip.tripHub.room.roomFunctionContainer.GetComponent<FieldTripBaseRoomFunction>();
            FieldTripLoot tripLoot = new FieldTripLoot();
            tripLoot.potentialItems = ((WeightedItemObject[])_potentialItems.GetValue(roomFunction)).ToList();
            tripLoot.guaranteedItems = ((List<ItemObject>)_guaranteedItems.GetValue(roomFunction)).ToList();
            foreach (KeyValuePair<BaseUnityPlugin, Action<FieldTrips, FieldTripLoot>> kvp in fieldtripLootChanges)
            {
                kvp.Value.Invoke(trip.trip, tripLoot);
            }
            if (fieldtripLootChanges.Count > 0)
            {
                trip.MarkAsNeverUnload();
                trip.tripHub.room.MarkAsNeverUnload();
                trip.tripHub.room.roomFunctionContainer.MarkAsNeverUnload();
            }
            _potentialItems.SetValue(roomFunction, tripLoot.potentialItems.ToArray());
            _guaranteedItems.SetValue(roomFunction, tripLoot.guaranteedItems.ToList());
        }

        /// <summary>
        /// Register an action that will be called to modify the field trip loot during mod loading.
        /// Unlike registering a generator action, you can only register one of these.
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="action"></param>
        public static void RegisterFieldTripLootChange(BaseUnityPlugin plug, Action<FieldTrips, FieldTripLoot> action)
        {
            if (fieldtripLootChanges.ContainsKey(plug))
            {
                throw new Exception("Attempted to add duplicate field trip loot change!");
            }
            fieldtripLootChanges.Add(plug, action);
        }

        /// <summary>
        /// This adds the specified SceneObject to the queue to be processed during the loading screen.
        /// Only use this for custom SceneObjecst you create.
        /// This will only work properly if called before the loading screen applies generator changes, aka not in post.
        /// </summary>
        public static void EnqueueGeneratorChanges(SceneObject sceneObj)
        {
            if (queuedModdedScenes.Contains(sceneObj)) return;
            queuedModdedScenes.Add(sceneObj);
        }

        /// <summary>
        /// Invoke the generator actions for the specified SceneObject.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="floorNumber"></param>
        /// <param name="obj"></param>
        public static void Invoke(string name, int floorNumber, SceneObject obj)
        {
            Dictionary<GenerationModType, List<Action<string, int, SceneObject>>> actionsList = new Dictionary<GenerationModType, List<Action<string, int, SceneObject>>>();
            foreach (var kvp in generationStuff) //i hate using var but i also dont want to type this out
            {
                Dictionary<GenerationModType, Action<string, int, SceneObject>> kvp2 = kvp.Value;
                foreach (var kvp3 in kvp2)
                {
                    if (!actionsList.ContainsKey(kvp3.Key))
                    {
                        actionsList.Add(kvp3.Key, new List<Action<string, int, SceneObject>>());
                    }
                    actionsList[kvp3.Key].Add(kvp3.Value);
                }
            }
            currentGenerationStepFlags = GenerationStageFlags.Preparation;
            if (actionsList.ContainsKey(GenerationModType.Preparation))
            {
                actionsList[GenerationModType.Preparation].Do(x => x.Invoke(name, floorNumber, obj));
            }
            currentGenerationStepFlags = GenerationStageFlags.Base;
            CustomLevelObject[] oldObjects = obj.GetCustomLevelObjects();
            if (actionsList.ContainsKey(GenerationModType.Base))
            {
                actionsList[GenerationModType.Base].Do(x => x.Invoke(name, floorNumber, obj));
            }
            currentGenerationStepFlags = GenerationStageFlags.Override;
            if (actionsList.ContainsKey(GenerationModType.Override))
            {
                actionsList[GenerationModType.Override].Do(x => x.Invoke(name, floorNumber, obj));
            }
            currentGenerationStepFlags = GenerationStageFlags.Addend;
            if (actionsList.ContainsKey(GenerationModType.Addend))
            {
                actionsList[GenerationModType.Addend].Do(x => x.Invoke(name, floorNumber, obj));
            }
            currentGenerationStepFlags = GenerationStageFlags.Finalizer;
            if (actionsList.ContainsKey(GenerationModType.Finalizer))
            {
                actionsList[GenerationModType.Finalizer].Do(x => x.Invoke(name, floorNumber, obj));
            }
            currentGenerationStepFlags = GenerationStageFlags.Invalid;
            int matchingObjects = 0;
            CustomLevelObject[] newObjects = obj.GetCustomLevelObjects();
            if (newObjects.Length != oldObjects.Length)
            {
                throw new InvalidOperationException("A mod changed LevelObject assignments outside GenerationModType.Preparation (length does not match)");
            }
            for (int i = 0; i < newObjects.Length; i++)
            {
                if (oldObjects.Contains(newObjects[i]))
                {
                    matchingObjects++;
                }
            }
            if (matchingObjects != oldObjects.Length)
            {
                throw new InvalidOperationException("A mod changed LevelObject assignments outside GenerationModType.Preparation (missing objects)");
            }
        }
    }
}
