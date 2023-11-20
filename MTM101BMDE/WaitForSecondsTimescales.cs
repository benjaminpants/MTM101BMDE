using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
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
