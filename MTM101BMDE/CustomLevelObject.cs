using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{

    /// <summary>
    /// A custom version of the LevelObject class, currently doesn't contain much else but it serves as a good base to make extending level generator functionality easy in the future.
    /// </summary>
    public class CustomLevelObject : LevelObject
    {
        [Obsolete("BB+ no longer uses .items, use .forcedItems or .potentialItems instead!", true)]
        public new WeightedItemObject[] items; //hacky way of adding the Obsolete tag, but it works?

        [Obsolete("BB+ no longer uses .classWallTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] classWallTexs;

        [Obsolete("BB+ no longer uses .classFloorTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] classFloorTexs;

        [Obsolete("BB+ no longer uses .classCeilingTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] classCeilingTexs;

        [Obsolete("BB+ no longer uses .facultyWallTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] facultyWallTexs;

        [Obsolete("BB+ no longer uses .facultyFloorTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] facultyFloorTexs;

        [Obsolete("BB+ no longer uses .facultyCeilingTexs, change the textures in the associated room group instead!", true)]
        public new WeightedTexture2D[] facultyCeilingTexs;

        [Obsolete("BB+ no longer uses .classLights, change the lights in the associated room group instead!", true)]
        public new WeightedTransform[] classLights;

        [Obsolete("BB+ no longer uses .facultyLights, change the lights in the associated room group instead!", true)]
        public new WeightedTransform[] facultyLights;

        [Obsolete("BB+ no longer uses .officeLights, change the lights in the associated room group instead!", true)]
        public new WeightedTransform[] officeLights;
    }
}
