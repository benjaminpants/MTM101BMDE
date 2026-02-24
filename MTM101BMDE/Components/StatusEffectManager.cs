using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Components
{
    public class StatusEffectManager : MonoBehaviour
    {
        public Entity myEntity;
        protected List<StatusEffect> statuses = new List<StatusEffect>();

        public void AddStatusEffect(StatusEffect effect)
        {
            statuses.Add(effect);
        }
    }
}
