using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{

    public static class ValueModifierExtensions
    {
        public static float Multiplier(this List<ValueModifier> modifiers)
        {
            float num = 1f;
            foreach (ValueModifier vm in modifiers)
            {
                num *= vm.multiplier;
            }
            return num;
        }

        public static float Addend(this List<ValueModifier> modifiers)
        {
            float num = 0f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                num += modifiers[i].addend;
            }
            return num;
        }
    }

    public abstract class ValueModifierManager<T> : MonoBehaviour
    {

        void Awake()
        {
            Initialize();
        }

        public abstract void Initialize();

        public abstract ValueModifier[] Modifiers { get; }

        public abstract void RemoveModifier(ValueModifier vm);

        public abstract void AbstractUpdate();

        void Update()
        {
            AbstractUpdate();
        }
    }

    public class ValueModifier
    {

        public ValueModifier(float multiplier = 1f, float addend = 0f)
        {
            this.multiplier = multiplier;
            this.addend = addend;
        }

        public float multiplier;
        public float addend;
    }
}
