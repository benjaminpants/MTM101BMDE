using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using MTM101BaldAPI.Registers;

namespace MTM101BaldAPI
{
    public static class ObjectFinders
    {
        [Obsolete("Please use ItemMetaStorage.Instance.FindByEnum(en).value instead!")]
        public static ItemObject GetFirstInstance(this Items en)
        {
            return ItemMetaStorage.Instance.FindByEnum(en).value;
        }

        [Obsolete("Please use NPCMetaStorage.Instance.Get(en).value instead!")]
        public static NPC GetFirstInstance(this Character en)
        {
            return NPCMetaStorage.Instance.Get(en).value;
        }
    }
}
