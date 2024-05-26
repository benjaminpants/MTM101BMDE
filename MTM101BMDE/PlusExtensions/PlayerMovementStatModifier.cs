using HarmonyLib;
using MTM101BaldAPI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.PlusExtensions
{

    /// <summary>
    /// A stat modifier for the PlayerMovement's variables.
    /// </summary>
    public class PlayerMovementStatModifier : MonoBehaviour
    {
        PlayerMovement pm;

        public Dictionary<string, List<ValueModifier>> modifiers = new Dictionary<string, List<ValueModifier>>();

        public Dictionary<string, float> baseStats = new Dictionary<string, float>();

        float GetModifiedStat(string name)
        {
            return (baseStats[name] * (modifiers[name].Multiplier())) + (modifiers[name].Addend());
        }

        void AddBaseStat(string name, float value)
        {
            baseStats.Add(name, value);
            modifiers.Add(name, new List<ValueModifier>());
        }

        public void AddModifier(string stat, ValueModifier vm)
        {
            modifiers[stat].Add(vm);
        }

        void Update()
        {
            pm.staminaRise = GetModifiedStat("staminaRise");
            pm.staminaMax = GetModifiedStat("staminaMax");
            pm.staminaDrop = GetModifiedStat("staminaDrop");
            pm.walkSpeed = GetModifiedStat("walkSpeed");
            pm.runSpeed = GetModifiedStat("runSpeed");
        }

        public void ChangeBaseStat(string statToChange, float value)
        {
            baseStats[statToChange] = value;
        }

        void Awake()
        {
            pm = GetComponent<PlayerMovement>();
            AddBaseStat("staminaRise", pm.staminaRise);
            AddBaseStat("staminaMax", pm.staminaMax);
            AddBaseStat("staminaDrop", pm.staminaDrop);
            AddBaseStat("walkSpeed", pm.walkSpeed);
            AddBaseStat("runSpeed", pm.runSpeed);
        }

        public void RemoveModifier(ValueModifier vm)
        {
            modifiers.Do(x =>
            {
                x.Value.Remove(vm);
            });
        }
    }
}
