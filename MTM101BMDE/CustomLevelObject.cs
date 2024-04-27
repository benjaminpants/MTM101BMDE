using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public class CustomLevelObject : LevelObject
    {
        public List<RoomTypeList> additionalRoomTypes = new List<RoomTypeList>();
    }

    public enum RoomTypeListSpawnMethod
    {
        /// <summary>
        /// The standard room spawning method used by classrooms, offices, and faculty rooms.
        /// </summary>
        Standard,
        /// <summary>
        /// A modified room spawning method that priotizes having one room in each direction, similar to exits.
        /// </summary>
        Exits,
        /// <summary>
        /// Uses the standard room spawning method for the first room, then every room after is attached to the previous.
        /// </summary>
        Chain
    }

    public class RoomTypeList : ScriptableObject
    {
        public List<WeightedRoomAsset> potentialAssets = new List<WeightedRoomAsset>();

        public float stickToHallChance = 1f;
    }
}
