using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace MTM101BaldAPI
{
    public static class ObjectFinders
    {
        public static ItemObject GetFirstInstance(this Items en)
        {
            return Resources.FindObjectsOfTypeAll<ItemObject>().ToList().Find(x => x.itemType == en);
        }

        public static NPC GetFirstInstance(this Character en)
        {
            return Resources.FindObjectsOfTypeAll<NPC>().ToList().Find(x => x.Character == en);
        }
    }
}
