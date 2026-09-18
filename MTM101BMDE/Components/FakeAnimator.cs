using HarmonyLib;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Components.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MTM101BaldAPI.Components
{

    public static class FakeAnimatorExtensions
    {
        public static T AddFakeAnimatorComponent<T>(this GameObject me, bool disableAnimator = true) where T : FakeAnimator
        {
            T fakeAnim = me.gameObject.AddComponent<T>();
            fakeAnim.animator = me.gameObject.GetComponent<Animator>();
            if (fakeAnim.animator == null)
            {
                fakeAnim.animator = me.gameObject.AddComponent<Animator>();
            }
            fakeAnim.animator.enabled &= !disableAnimator;
            return fakeAnim;
        }
    }

    [Serializable]
    public abstract class FakeAnimator : MonoBehaviour
    {
        public static Dictionary<Animator, FakeAnimator> globalTrackers = new Dictionary<Animator, FakeAnimator>();

        [SerializeField]
        Animator animatorToTrack;

        public Animator animator
        {
            get
            {
                return animatorToTrack;
            }
            set
            {
                if (animatorToTrack == null)
                {
                    animatorToTrack = value;
                    return;
                }
                // probably unintialized yet
                if (!globalTrackers.ContainsKey(animatorToTrack))
                {
                    animatorToTrack = value;
                    return;
                }
                globalTrackers.Remove(animatorToTrack);
                animatorToTrack = value;
                globalTrackers.Add(value, this);
            }
        }

        void Awake()
        {
            globalTrackers.Add(animatorToTrack, this);
        }


        void OnDestroy()
        {
            globalTrackers.Remove(animatorToTrack);
        }

        public abstract float speed { get; set; }

        public abstract void Play(string stateName, int layer, float normalizedTime);

        public abstract void SetBool(string id, bool value);
        public abstract bool GetBool(string id);

        public abstract void SetInteger(string id, int value);
        public abstract int GetInteger(string id);

        public abstract void SetFloat(string id, float value);
        public abstract float GetFloat(string id);

        public abstract void SetTrigger(string id);
        public abstract void ResetTrigger(string id);
    }

    public class FakeAnimatorSimple : FakeAnimator
    {
        public ISimpleAnimator simpleAnimator;

        public override float speed { get => simpleAnimator.AnimationSpeed; set => simpleAnimator.AnimationSpeed = value; }

        public override bool GetBool(string id)
        {
            return false;
        }

        public override float GetFloat(string id)
        {
            return 0f;
        }

        public override int GetInteger(string id)
        {
            return 0;
        }

        public override void Play(string stateName, int layer, float normalizedTime)
        {
            simpleAnimator.Play(stateName, normalizedTime);
        }

        public override void ResetTrigger(string id)
        {
        }

        public override void SetBool(string id, bool value)
        {
        }

        public override void SetFloat(string id, float value)
        {
        }

        public override void SetInteger(string id, int value)
        {
        }

        public override void SetTrigger(string id)
        {
            simpleAnimator.Play(id, 1f);
        }
    }

#if DEBUG
    public class FakeAnimatorDebug : FakeAnimator
    {
        public override float speed
        {
            get
            {
                Debug.Log("Get FakeAnimator!");
                return 1f;
            }
            set
            {
                Debug.Log("Set FakeAnimator!");
            }
        }

        public override bool GetBool(string id)
        {
            Debug.Log("Get bool");
            return false;
        }

        public override float GetFloat(string id)
        {
            Debug.Log("Get float");
            return 0f;
        }

        public override int GetInteger(string id)
        {
            Debug.Log("Get int");
            return 0;
        }

        public override void Play(string stateName, int layer, float normalizedTime)
        {
            Debug.Log("FakePlay with: " + stateName + ", " + layer + ", " + normalizedTime + "!");
        }

        public override void ResetTrigger(string id)
        {
            Debug.Log("Reset trigger: " + id + "!");
        }

        public override void SetBool(string id, bool value)
        {
            Debug.Log("Set bool");
        }

        public override void SetFloat(string id, float value)
        {
            Debug.Log("Set float");
        }

        public override void SetInteger(string id, int value)
        {
            Debug.Log("Set int");
        }

        public override void SetTrigger(string id)
        {
            Debug.Log("Set trigger: " + id + "!");
        }
    }

#endif
}


namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch]
    class AnimatorPatches
    {
        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("Play", new Type[] { typeof(string), typeof(int), typeof(float) })]
        [HarmonyPrefix]
        static bool PlayBasicPrefix(Animator __instance, string stateName, int layer, float normalizedTime)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].Play(stateName, layer, normalizedTime);
                return false;
            }
            return true;
        }


        // less useful, honestly unsure if we need to patch this but better safe then sorry
        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("Play", new Type[] { typeof(int), typeof(int), typeof(float) })]
        [HarmonyPrefix]
        static bool PlayHashPrefix(Animator __instance, int stateNameHash, int layer, float normalizedTime)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].Play(stateNameHash.ToString(), layer, normalizedTime);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("speed", MethodType.Getter)]
        [HarmonyPrefix]
        static bool SpeedGetterPrefix(Animator __instance, ref float __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].speed;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("speed", MethodType.Setter)]
        [HarmonyPrefix]
        static bool SpeedSetterPrefix(Animator __instance, float __0)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].speed = __0;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetBoolString")]
        [HarmonyPrefix]
        static bool GetBoolStringPrefix(Animator __instance, string name, ref bool __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetBool(name);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetBoolID")]
        [HarmonyPrefix]
        static bool GetBoolIdPrefix(Animator __instance, int id, ref bool __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetBool(id.ToString());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetFloatString")]
        [HarmonyPrefix]
        static bool GetFloatStringPrefix(Animator __instance, string name, ref float __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetFloat(name);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetFloatID")]
        [HarmonyPrefix]
        static bool GetFloatIdPrefix(Animator __instance, int id, ref float __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetFloat(id.ToString());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetIntegerString")]
        [HarmonyPrefix]
        static bool GetIntStringPrefix(Animator __instance, string name, ref int __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetInteger(name);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("GetIntegerID")]
        [HarmonyPrefix]
        static bool GetIntIdPrefix(Animator __instance, int id, ref int __result)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                __result = FakeAnimator.globalTrackers[__instance].GetInteger(id.ToString());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetBoolString")]
        [HarmonyPrefix]
        static bool SetBoolStringPrefix(Animator __instance, string name, bool value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetBool(name, value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetBoolID")]
        [HarmonyPrefix]
        static bool SetBoolIDPrefix(Animator __instance, int id, bool value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetBool(id.ToString(), value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetFloatString")]
        [HarmonyPrefix]
        static bool SetFloatStringPrefix(Animator __instance, string name, float value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetFloat(name, value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetFloatID")]
        [HarmonyPrefix]
        static bool SetFloatIDPrefix(Animator __instance, int id, float value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetFloat(id.ToString(), value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetIntegerString")]
        [HarmonyPrefix]
        static bool SetIntegerStringPrefix(Animator __instance, string name, int value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetInteger(name, value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetIntegerID")]
        [HarmonyPrefix]
        static bool SetIntegerIDPrefix(Animator __instance, int id, int value)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetInteger(id.ToString(), value);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetTriggerString")]
        [HarmonyPrefix]
        static bool SetTriggerStringPrefix(Animator __instance, string name)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetTrigger(name);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("SetTriggerID")]
        [HarmonyPrefix]
        static bool SetTriggerIDPrefix(Animator __instance, int id)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].SetTrigger(id.ToString());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("ResetTriggerString")]
        [HarmonyPrefix]
        static bool ResetTriggerStringPrefix(Animator __instance, string name)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].ResetTrigger(name);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Animator))]
        [HarmonyPatch("ResetTriggerID")]
        [HarmonyPrefix]
        static bool ResetTriggerIDPrefix(Animator __instance, int id)
        {
            if (FakeAnimator.globalTrackers.ContainsKey(__instance))
            {
                FakeAnimator.globalTrackers[__instance].ResetTrigger(id.ToString());
                return false;
            }
            return true;
        }
    }
}