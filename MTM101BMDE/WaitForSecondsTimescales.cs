using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    /// <summary>
    /// Wait for the defined amount of time depending on the NPCTimeScale of the environement controller. The NPC Timescale can be affected by The Test for instance.
    /// </summary>
    public class WaitForSecondsNPCTimescale : CustomYieldInstruction
    {
        public EnvironmentController ec;
        public float timeRemaining;

        public WaitForSecondsNPCTimescale(EnvironmentController envC, float seconds)
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
                return (timeRemaining >= 0);
            }
        }
    }

    /// <summary>
    /// Wait for the defined amount of time depending on the EnvironmentTimescale of the environement controller.
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

    [Obsolete("Please use WaitForSecondsEnvironmentTimescale instead!", true)]
    public class WaitForSecondsEnviromentTimescale : CustomYieldInstruction
    {
        public EnvironmentController ec;
        public float timeRemaining;

        public WaitForSecondsEnviromentTimescale(EnvironmentController envC, float seconds)
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
