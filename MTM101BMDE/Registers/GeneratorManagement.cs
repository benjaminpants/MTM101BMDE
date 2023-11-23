using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public enum GenerationModType
    {
        Base, // runs first, expected to override everything
        Override, // overrides specific variables, such as items and whatnot
        Addend, // adds to already existing fields, such as adding new items
        Finalizer // runs last, useful for subtracting/removing things or if you REALLY need the last say, use with caution!
    }


    public static class GeneratorManagement
    {
        private static Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, LevelObject>>> generationStuff = new Dictionary<BaseUnityPlugin, Dictionary<GenerationModType, Action<string, LevelObject>>>();

        public static void Register(BaseUnityPlugin plug, GenerationModType type, Action<string, LevelObject> action)
        {
            generationStuff.Add(plug, new Dictionary<GenerationModType, Action<string, LevelObject>>());
            generationStuff[plug].Add(type, action);
        }

        public static void Invoke(string name, LevelObject obj)
        {
            Dictionary<GenerationModType, List<Action<string, LevelObject>>> actionsList = new Dictionary<GenerationModType, List<Action<string, LevelObject>>>();
            foreach (var kvp in generationStuff) //i hate using var but i also dont want to type this out
            {
                Dictionary<GenerationModType, Action<string, LevelObject>> kvp2 = kvp.Value;
                foreach (var kvp3 in kvp2)
                {
                    if (!actionsList.ContainsKey(kvp3.Key))
                    {
                        actionsList.Add(kvp3.Key, new List<Action<string, LevelObject>>());
                    }
                    actionsList[kvp3.Key].Add(kvp3.Value);
                }
            }
            if (actionsList.ContainsKey(GenerationModType.Base))
            {
                actionsList[GenerationModType.Base].Do(x => x.Invoke(name, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Override))
            {
                actionsList[GenerationModType.Override].Do(x => x.Invoke(name, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Addend))
            {
                actionsList[GenerationModType.Addend].Do(x => x.Invoke(name, obj));
            }
            if (actionsList.ContainsKey(GenerationModType.Finalizer))
            {
                actionsList[GenerationModType.Finalizer].Do(x => x.Invoke(name, obj));
            }
        }
    }
}
