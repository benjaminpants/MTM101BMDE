using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("Generate", MethodType.Enumerator)]
    class LevelGeneratorPatches
    {

        static MethodInfo _FrameShouldEnd = AccessTools.Method(typeof(LevelBuilder), "FrameShouldEnd");

        static void CallPreOffice(LevelGenerator generator)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeOffice);
        }

        static void CallPreClassroom(LevelGenerator generator)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeClassroom);
        }

        static void CallPreFaculty(LevelGenerator generator)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeFaculty);
        }

        static void CallPreExtra(LevelGenerator generator)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeExtraRooms);
        }

        static void CallPostAll(LevelGenerator generator)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.AfterAll);
        }

        static void CallGenerateGroups(LevelGenerator generator, RoomGroupPriority priority)
        {
            ((CustomLevelObject)generator.ld).additionalRoomTypes.Where(x => x.priority == priority).Do(x => GenerateGroup(generator, x));
        }

        static MethodInfo _RandomlyPlaceRoom = typeof(LevelGenerator).GetMethod("RandomlyPlaceRoom", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(RoomAsset), typeof(bool), typeof(RoomController).MakeByRefType() }, null);

        static FieldInfo _potentialRoomSpawns = AccessTools.Field(typeof(LevelBuilder), "potentialRoomSpawns");

        static void GenerateExitGroup(LevelGenerator generator, RoomTypeGroup group)
        {
            bool stickingToHalls = generator.controlledRNG.NextDouble() < (double)group.stickToHallChance; //store as a variable so RoomGroupSpawnMethod.Exits can flip it
            bool triedCurrentDirection = false;
            generator.UpdatePotentialSpawnsForRooms(stickingToHalls);
            List<WeightedRoomAsset> potentialRoomsList = new List<WeightedRoomAsset>(group.potentialAssets);
            int roomCount = Mathf.Max(generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1), 0);
            List<Direction> directions = Directions.All();
            for (int i = 0; i < roomCount; i++)
            {
                if (potentialRoomsList.Count == 0) break;
                // exit specific logic, todo: rewrite this
                isolate_all_to_exits:
                int dirIndex = generator.controlledRNG.Next(0, directions.Count);
                List<WeightedRoomSpawn> weightedRoomSpawns = (List<WeightedRoomSpawn>)_potentialRoomSpawns.GetValue(generator);
                Direction triedDirection = directions[dirIndex]; //store this so that if we fail, we can attempt it with the opposite parity
                weightedRoomSpawns.RemoveAll(x => x.selection.direction != triedDirection);
                directions.RemoveAt(dirIndex);
                if (weightedRoomSpawns.Count == 0)
                {
                    UnityEngine.Debug.Log("Removed all valid spawns when spawning RoomGroupSpawnMethod.Exits type room, trying again...");
                    if (group.stickToHallChance < 1f && !triedCurrentDirection) //if we have the possibility of doing the opposite of what we tried, flip and try again.
                    {
                        triedCurrentDirection = true;
                        directions.Clear();
                        directions.Add(triedDirection);
                        generator.UpdatePotentialSpawnsForRooms(!stickingToHalls);
                        UnityEngine.Debug.Log("Trying the opposite inversion!");
                        goto isolate_all_to_exits;
                    }
                    generator.UpdatePotentialSpawnsForRooms(generator.controlledRNG.NextDouble() < (double)group.stickToHallChance);
                    if (directions.Count == 0)
                    {
                        directions = Directions.All();
                    }
                    goto isolate_all_to_exits;
                }
                triedCurrentDirection = false;
                // do the typical spawn behavior code, select an asset, try it, and then remove it if it fails.
                WeightedSelection<RoomAsset>[] potentialAssets = group.potentialAssets.ToArray();
                int index = WeightedSelection<RoomAsset>.ControlledRandomIndex(potentialAssets, generator.controlledRNG);
                object[] parameters = new object[] { potentialAssets[index].selection, true, null };
                bool result = (bool)_RandomlyPlaceRoom.Invoke(generator, parameters);
                RoomController rm = (RoomController)parameters[2]; // what the fuck?
                _FrameShouldEnd.Invoke(generator, null);
                if (!result)
                {
                    if (directions.Count == 0) // if we still have directions to try, do not give up yet.
                    {
                        potentialRoomsList.RemoveAt(index);
                    }
                    i--;
                }
                else
                {
                    ((List<RoomController>)_standardRooms.GetValue(generator)).Add(rm);
                    stickingToHalls = generator.controlledRNG.NextDouble() < (double)group.stickToHallChance;
                    generator.UpdatePotentialSpawnsForRooms(stickingToHalls);
                }
                if (directions.Count == 0)
                {
                    directions = Directions.All();
                }
            }
        }

        static void GenerateGroup(LevelGenerator generator, RoomTypeGroup group)
        {
            if (group.spawnMethod == RoomGroupSpawnMethod.Exits)
            {
                GenerateExitGroup(generator, group);
                return;
            }
            bool stickingToHalls = generator.controlledRNG.NextDouble() < (double)group.stickToHallChance; //store as a variable so RoomGroupSpawnMethod.Exits can flip it
            generator.UpdatePotentialSpawnsForRooms(stickingToHalls);
            List<WeightedRoomAsset> potentialRoomsList = new List<WeightedRoomAsset>(group.potentialAssets);
            int roomCount = Mathf.Max(generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1), 0);
            for (int i = 0; i < roomCount; i++)
            {
                if (potentialRoomsList.Count == 0) break;
                // do the typical spawn behavior code, select an asset, try it, and then remove it if it fails.
                WeightedSelection<RoomAsset>[] potentialAssets = group.potentialAssets.ToArray();
                int index = WeightedSelection<RoomAsset>.ControlledRandomIndex(potentialAssets, generator.controlledRNG);
                object[] parameters = new object[] { potentialAssets[index].selection, true, null};
                bool result = (bool)_RandomlyPlaceRoom.Invoke(generator, parameters);
                RoomController rm = (RoomController)parameters[2]; // what the fuck?
                _FrameShouldEnd.Invoke(generator, null);
                if (!result)
                {
                    potentialRoomsList.RemoveAt(index);
                    i--;
                }
                else
                {
                    ((List<RoomController>)_standardRooms.GetValue(generator)).Add(rm);
                    stickingToHalls = generator.controlledRNG.NextDouble() < (double)group.stickToHallChance;
                    generator.UpdatePotentialSpawnsForRooms(stickingToHalls);
                }
            }
        }

        static MethodInfo _CallPostAll = AccessTools.Method(typeof(LevelGeneratorPatches), "CallPostAll");

        static FieldInfo _standardRooms = AccessTools.Field(typeof(LevelGenerator), "standardRooms");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            // initialize the dictionary
            Dictionary<FieldInfo, MethodInfo> preGenDict = new Dictionary<FieldInfo, MethodInfo>
            {
                { AccessTools.Field(typeof(LevelObject), "potentialOffices"), AccessTools.Method(typeof(LevelGeneratorPatches), "CallPreOffice") },
                { AccessTools.Field(typeof(LevelObject), "potentialClassRooms"), AccessTools.Method(typeof(LevelGeneratorPatches), "CallPreClassroom") },
                { AccessTools.Field(typeof(LevelObject), "potentialFacultyRooms"), AccessTools.Method(typeof(LevelGeneratorPatches), "CallPreFaculty") },
                { AccessTools.Field(typeof(LevelObject), "potentialExtraRooms"), AccessTools.Method(typeof(LevelGeneratorPatches), "CallPreExtra") },
            };

            List<FieldInfo> notPatched = preGenDict.Keys.ToList();

            CodeInstruction[] codeInstructions = instructions.ToArray();

            bool patchedLast = false;

            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                yield return instruction;
                if (notPatched.Count == 0)
                {
                    if (patchedLast) continue;
                    // all the preGen stuff is done, now search only for the last thing we need
                    if (i + 5 > codeInstructions.Length - 1) continue;
                    if (
                        // using (List<RoomController>.Enumerator enumerator9 = this.standardRooms.GetEnumerator())
                        (codeInstructions[i + 0].opcode == OpCodes.Ldarg_0) &&
                        (codeInstructions[i + 1].opcode == OpCodes.Ldloc_2) &&
                        (codeInstructions[i + 2].opcode == OpCodes.Ldfld) &&
                        (codeInstructions[i + 3].opcode == OpCodes.Callvirt) &&
                        (codeInstructions[i + 4].opcode == OpCodes.Stfld) &&
                        ((FieldInfo)codeInstructions[i + 2].operand == _standardRooms)
                        )
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_2); //this
                        yield return new CodeInstruction(OpCodes.Call, _CallPostAll);
                        patchedLast = true;
                    }
                    continue;
                }
                for (int z = notPatched.Count - 1; z >= 0; z--)
                {
                    if (i + 6 > codeInstructions.Length - 1) continue;
                    if (
                        (codeInstructions[i + 0].opcode == OpCodes.Ldarg_0) &&
                        (codeInstructions[i + 1].opcode == OpCodes.Ldloc_2) &&
                        (codeInstructions[i + 2].opcode == OpCodes.Ldfld) &&
                        (codeInstructions[i + 3].opcode == OpCodes.Ldfld) &&
                        (codeInstructions[i + 4].opcode == OpCodes.Newobj) &&
                        (codeInstructions[i + 5].opcode == OpCodes.Stfld) &&
                        ((FieldInfo)codeInstructions[i + 3].operand == notPatched[z])
                        )
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_2); //this
                        yield return new CodeInstruction(OpCodes.Call, preGenDict[notPatched[z]]);
                        notPatched.RemoveAt(z);
                    }
                }
            }
            if (notPatched.Count != 0) throw new Exception("Unable to patch LevelGenerator.Generate preGenDict!");
            if (!patchedLast) throw new Exception("Unable to patch LevelGenerator.Generate CallPostAll");
            yield break;
        }
    }
}
