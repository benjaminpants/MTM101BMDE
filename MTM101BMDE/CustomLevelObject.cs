using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public class CustomLevelObject : LevelObject
    {
        public List<RoomTypeGroup> additionalRoomTypes = new List<RoomTypeGroup>();
    }

    public enum RoomGroupSpawnMethod
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
        /// Uses the standard room spawning method for the first room of the group, then every room after is attached to the previous.
        /// Note that it is not guranteed that all rooms will spawn or that there won't be duplicate directions, but it will try to be avoided.
        /// This generation method is also a lot slower.
        /// </summary>
        Chain,
        /// <summary>
        /// Uses the same logic that special rooms use to spawn.
        /// The priority will be ignored and these will spawn after special rooms.
        /// stickToHallChance will be treated as stickToEdgeChance.
        /// </summary>
        SpecialRooms
    }

    public enum RoomGroupPriority
    {
        BeforeOffice,
        BeforeClassroom,
        BeforeFaculty,
        BeforeExtraRooms,
        AfterAll
    }

    [Serializable]
    public class RoomTypeGroup
    {
        public int minRooms = 1;
        public int maxRooms = 1;

        public RoomGroupPriority priority = RoomGroupPriority.AfterAll;
        
        public RoomGroupSpawnMethod spawnMethod = RoomGroupSpawnMethod.Standard;

        public WeightedRoomAsset[] potentialAssets = new WeightedRoomAsset[0];

        public float stickToHallChance = 1f;
    }
}
