using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.PlusExtensions
{
    public static class PlusExtension
    {
        public static PlayerMovementStatModifier GetModifier(this PlayerMovement pm)
        {
            PlayerMovementStatModifier modifier = pm.GetComponent<PlayerMovementStatModifier>();
            if (modifier) return modifier;
            return pm.gameObject.AddComponent<PlayerMovementStatModifier>();
        }

        public static PlayerMovementStatModifier GetMovementStatModifier(this PlayerManager pm)
        {
            return pm.plm.GetModifier();
        }
    }
}
