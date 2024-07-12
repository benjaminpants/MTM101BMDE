using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public class CustomLevelObject : LevelObject
    {
        [Obsolete("BB+ no longer uses .items, use .forcedItems or .potentialItems instead!", true)]
        public new WeightedItemObject[] items; //hacky way of adding the Obsolete tag, but it works?
    }
}
