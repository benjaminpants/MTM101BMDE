using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class HarmonyExtensions
    {
        /// <summary>
        /// Patches all conditional patches with the specified assembly
        /// </summary>
        public static void PatchAllConditionals(this Harmony _harmony, Assembly assembly, bool assumeUnmarkedAsTrue = true)
        {
            AccessTools.GetTypesFromAssembly(assembly).Do(type =>
            {
                foreach (CustomAttributeData cad in type.CustomAttributes)
                {
                    if (typeof(ConditionalPatch).IsAssignableFrom(cad.AttributeType))
                    {
                        List<CustomAttributeTypedArgument> list = cad.ConstructorArguments.ToList();
                        List<object> paramList = new List<object>();
                        list.ForEach(arg =>
                        {
                            paramList.Add(arg.Value);
                        });
                        ConditionalPatch condP = (ConditionalPatch)Activator.CreateInstance(cad.AttributeType, paramList.ToArray());
                        if (condP.ShouldPatch())
                        {
                            _harmony.CreateClassProcessor(type).Patch();
                        }
                        return;
                    }
                }
                if (assumeUnmarkedAsTrue)
                {
                    _harmony.CreateClassProcessor(type).Patch();
                }
            });
        }

        /// <summary>
        /// Patches all conditional patches in the current assembly. A direct replacement for PatchAll.
        /// </summary>
        /// <param name="_harmony"></param>
        public static void PatchAllConditionals(this Harmony _harmony)
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = method.ReflectedType.Assembly;
            _harmony.PatchAllConditionals(assembly);
        }
    }

    /// <summary>
    /// Base class for ConditionalPatches.
    /// </summary>
    public abstract class ConditionalPatch : Attribute
    {
        public abstract bool ShouldPatch();
    }

    /// <summary>
    /// Always patches, same as not having a ConditionalPatch at all.
    /// </summary>
    public class ConditionalPatchAlways : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return true;
        }
    }

    /// <summary>
    /// Patches if the specified config is true.
    /// </summary>
    public class ConditionalPatchConfig : ConditionalPatch
    {
        string _mod;
        string _category;
        string _name;
        public ConditionalPatchConfig(string mod, string category, string name)
        {
            _mod = mod;
            _category = category;
            _name = name;
        }

        public override bool ShouldPatch()
        {
            if (!Chainloader.PluginInfos.ContainsKey(_mod))
            {
                UnityEngine.Debug.LogWarning("ConditionalPatchConfig can NOT find mod with name:" + _mod);
                return false;
            }
            BaseUnityPlugin instance = Resources.FindObjectsOfTypeAll<BaseUnityPlugin>().First(x => x.Info == Chainloader.PluginInfos[_mod]);
            instance.Config.TryGetEntry(new ConfigDefinition(_category, _name), out ConfigEntry<bool> entry);
            if (entry == null)
            {
                UnityEngine.Debug.LogWarning(String.Format("Cannot find config with: ({0}) {1}, {2}", _mod, _category, _name));
                return false;
            }
            return entry.Value;
        }
    }

    /// <summary>
    /// Never patches.
    /// </summary>
    public class ConditionalPatchNever : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return false;
        }
    }

    /// <summary>
    /// Patch if the specified mod is installed.
    /// </summary>
    public class ConditionalPatchMod : ConditionalPatch
    {
        public string modKey;

        public ConditionalPatchMod(string mod)
        {
            modKey = mod;
        }

        public override bool ShouldPatch()
        {
            return Chainloader.PluginInfos.ContainsKey(modKey);
        }
    }

    /// <summary>
    /// Patch if the specified mod is not installed.
    /// </summary>
    public class ConditionalPatchNoMod : ConditionalPatch
    {
        public string modKey;

        public ConditionalPatchNoMod(string mod)
        {
            modKey = mod;
        }

        public override bool ShouldPatch()
        {
            return !Chainloader.PluginInfos.ContainsKey(modKey);
        }
    }
}
