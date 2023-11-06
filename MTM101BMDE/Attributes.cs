using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public static void PatchAllConditionals(this Harmony _harmony)
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = method.ReflectedType.Assembly;
            _harmony.PatchAllConditionals(assembly);
        }
    }


    public abstract class ConditionalPatch : Attribute
    {
        public abstract bool ShouldPatch();
    }

    public class ConditionalPatchAlways : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return true;
        }
    }

    public class ConditionalPatchNever : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return false;
        }
    }

    public class ConditionalPatchBBCROnly : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return MTM101BaldiDevAPI.IsClassicRemastered;
        }
    }

    public class ConditionalPatchBBPOnly : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return !MTM101BaldiDevAPI.IsClassicRemastered;
        }
    }

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
