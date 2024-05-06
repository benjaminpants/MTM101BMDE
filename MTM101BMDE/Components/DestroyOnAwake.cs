using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{
    public class DestroyOnAwakeInstantWithWarning : MonoBehaviour
    {
        void Awake()
        {
            UnityEngine.Debug.LogWarning("DestroyOnAwake called!");
            GameObject.DestroyImmediate(gameObject);
        }
    }
}
