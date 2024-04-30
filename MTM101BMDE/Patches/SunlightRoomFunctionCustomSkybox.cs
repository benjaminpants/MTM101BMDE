using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [ConditionalPatchConfig("mtm101.rulerp.bbplus.baldidevapi", "Generator", "Enable Skybox Patches")]
    [HarmonyPatch(typeof(SunlightRoomFunction))]
    [HarmonyPatch("Initialize")]
    class SunlightRoomFunctionCustomSkybox
    {
        static bool Prefix(RoomController room, ref RoomController ___room)
        {
            ___room = room; //cant call inheritence because c# reflection lolol
            Color colorForSkybox = Color.white;
            SkyboxMetadata meta = SkyboxMetaStorage.Instance.Get(Singleton<CoreGameManager>.Instance.sceneObject.skybox);
            if (meta != null)
            {
                colorForSkybox = meta.lightColor;
            }
            else
            {
                MTM101BaldiDevAPI.Log.LogWarning("Skybox/Cubemap with name:" + Singleton<CoreGameManager>.Instance.sceneObject.skybox.name + " doesn't have metadata defined!");
            }
            for (int i = 0; i < room.TileCount; i++)
            {
                room.TileAtIndex(i).permanentLight = true;
                room.ec.GenerateLight(room.TileAtIndex(i), colorForSkybox, 1);
            }
            return false;
        }
    }
}
