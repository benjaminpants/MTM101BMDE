using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{
    /// <summary>
    /// GameObjects with this behavior makes the culling manager treat it as if it were a player. Useful if you have are using cameras that aren't the players'.
    /// Note that performance may take a hit if you use this, so use it with caution! (Maybe if the player isn't looking you could remove the component?)
    /// </summary>
    public class CullAffector : MonoBehaviour
    {
        void Awake()
        {
            allAffectors.Add(this);
        }

        void OnDestroy()
        {
            allAffectors.Remove(this);
        }

        internal static List<CullAffector> allAffectors = new List<CullAffector>();
    }
}
