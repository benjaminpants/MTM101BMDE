using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class Extensions
    {
        public static void MarkAsNeverUnload(this ScriptableObject me)
        {
            MTM101BaldiDevAPI.keepInMemory.Add(me);
        }
        public static void RemoveUnloadMark(this ScriptableObject me)
        {
            MTM101BaldiDevAPI.keepInMemory.Remove(me);
        }
    }
}
