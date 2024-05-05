using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public class CustomLevelObject : LevelObject
    {
        public List<RoomTypeGroup> additionalRoomTypes = new List<RoomTypeGroup>();
        public List<RoomTextureGroup> additionalTextureGroups = new List<RoomTextureGroup>();

        [Obsolete("BB+ no longer uses .items, use .forcedItems or .potentialItems instead!", true)]
        public new WeightedItemObject[] items; //hacky way of adding the Obsolete tag, but it works?
    }

    public enum RoomGroupSpawnMethod
    {
        /// <summary>
        /// The standard room spawning method used by classrooms, offices, and faculty rooms.
        /// </summary>
        Standard,
        /// <summary>
        /// A modified room spawning method that priotizes having one room in each direction, similar to exits.
        /// Note that it is not guranteed that all rooms will spawn or that there won't be duplicate directions, but it will try to be avoided.
        /// This generation method is also a lot slower.
        /// </summary>
        Exits,
        /// <summary>
        /// A modified room spawning method that spawns a room normally, then every room after has to be connected to the previous rooms.
        /// Requires atleast one potentialDoorPosition in each room asset, but more are recommended to ensure no OOB rooms generate.
        /// </summary>
        Chain,
        /// <summary>
        /// Uses the same logic that special rooms use to spawn.
        /// The priority will be ignored and these will spawn after special rooms.
        /// stickToHallChance will be treated as stickToEdgeChance.
        /// Doors will NOT be generated, you will have to use a RoomFunction(special rooms use SpecialRoomSwingingDoorsBuilder) assigned to the room for those.
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
    public class RoomTextureGroup
    {
        public string name;
        public WeightedTexture2D[] potentialFloorTextures;
        public WeightedTexture2D[] potentialWallTextures;
        public WeightedTexture2D[] potentialCeilTextures;
    }

    [Serializable]
    public class RoomTypeGroup
    {
        public int minRooms = 1;
        public int maxRooms = 1;

        public bool generateDoors = true;

        public RoomGroupPriority priority = RoomGroupPriority.AfterAll;
       
        public RoomGroupSpawnMethod spawnMethod = RoomGroupSpawnMethod.Standard;

        public WeightedRoomAsset[] potentialAssets = new WeightedRoomAsset[0];
        public float stickToHallChance = 1f;

        /// <summary>
        /// The name of the RoomTextureGroup to use.
        /// "hall", "class", and "faculty" are always valid.
        /// </summary>
        public string textureGroupName = "";
    }
}
