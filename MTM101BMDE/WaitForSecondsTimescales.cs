using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    /// <summary>
    /// Wait for the defined amount of time depending on the global NPC timescale. It is advised to use WaitForSecondsNPCTimescale instead.
    /// </summary>
    public class WaitForSecondsGlobalNPCTimescale : CustomYieldInstruction
    {
        public EnvironmentController ec;
        public float timeRemaining;

        public WaitForSecondsGlobalNPCTimescale(EnvironmentController envC, float seconds)
        {
            timeRemaining = seconds;
            ec = envC;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!ec) return false; // EC GONE! Establish CHAOS!
                timeRemaining -= Time.deltaTime * ec.NpcTimeScale;
                return (timeRemaining >= 0);
            }
        }
    }

    public class WaitForSecondsMoveMod : CustomYieldInstruction
    {
        public ActivityModifier actM;
        public float timeRemaining;

        public WaitForSecondsMoveMod(ActivityModifier actMod, float seconds)
        {
            timeRemaining = seconds;
            actM = actMod;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!actM) return false; // actM somehow null
                timeRemaining -= Time.deltaTime * actM.Multiplier;
                return timeRemaining >= 0;
            }
        }
    }

    /// <summary>
    /// Waits for the amount of time, scaled based off of the NPC's timescale.
    /// </summary>
    public class WaitForSecondsNPCTimescale : CustomYieldInstruction
    {
        public float timeRemaining;
        public NPC npc;

        public WaitForSecondsNPCTimescale(NPC npc, float seconds)
        {
            timeRemaining = seconds;
            this.npc = npc;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!npc) return false; // NPC GONE! Establish CHAOS!
                timeRemaining -= Time.deltaTime * npc.TimeScale;
                return (timeRemaining >= 0);
            }
        }
    }

    /// <summary>
    /// Wait for the defined amount of time depending on the EnvironmentTimescale of the environment controller.
    /// </summary>
    public class WaitForSecondsEnvironmentTimescale : CustomYieldInstruction
    {
        public EnvironmentController ec;
        public float timeRemaining;

        public WaitForSecondsEnvironmentTimescale(EnvironmentController envC, float seconds)
        {
            timeRemaining = seconds;
            ec = envC;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!ec) return false; // EC GONE! Establish CHAOS!
                timeRemaining -= Time.deltaTime * ec.EnvironmentTimeScale;
                return (timeRemaining >= 0);
            }
        }
    }
}
