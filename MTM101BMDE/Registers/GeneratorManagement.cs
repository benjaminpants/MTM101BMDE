using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public enum GenerationModType
    {
        /// <summary>
        /// This should be used for methods that override the majority of the generator properties, almost completely transforming the level.
        /// </summary>
        Base, // runs first, expected to override everything

        /// <summary>
        /// This should be used for overriding only certain properties of the generator, such as changing exit counts.
        /// </summary>
        Override, // overrides specific variables, such as items and whatnot

        /// <summary>
        /// This should be used for adding onto already existing properties, such as adding characters or items. Most mods will be using this.
        /// </summary>
        Addend, // adds to already existing fields, such as adding new items

        /// <summary>
        /// Useful for removing things that might've been added or changed by other mods, or if for one reason or another you need the final say on something. 
        /// <c>Use with caution.</c>
        /// </summary>
        Finalizer // runs last, useful for subtracting/removing things or if you REALLY need the last say, use with caution!
    }


    public static class GeneratorManagement
    {
        private static Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, int, SceneObject>>> generationStuff = new Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, int, SceneObject>>>();

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
            if (actionsList.ContainsKey(GenerationModType.Base))
            {
                actionsList[GenerationModType.Base].Do(x => x.Invoke(name, floorNumber, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Override))
            {
                actionsList[GenerationModType.Override].Do(x => x.Invoke(name, floorNumber, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Addend))
            {
                actionsList[GenerationModType.Addend].Do(x => x.Invoke(name, floorNumber, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Finalizer))
            {
                actionsList[GenerationModType.Finalizer].Do(x => x.Invoke(name, floorNumber, obj));
            }
        }
    }
}
