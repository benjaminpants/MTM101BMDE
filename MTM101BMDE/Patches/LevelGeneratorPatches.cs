using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace MTM101BaldAPI.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("Generate", MethodType.Enumerator)]
    class LevelGeneratorPatches
    {

        struct SelectedGenTextures
        {
            public Texture2D wall;
            public Texture2D floor;
            public Texture2D ceiling;
        }

        const string generateSubclassName = "LevelGenerator+<Generate>d__2, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        static FieldInfo _GhallWallTex = AccessTools.Field(Type.GetType(generateSubclassName), "<hallWallTex>5__11");
        static FieldInfo _GhallFloorTex = AccessTools.Field(Type.GetType(generateSubclassName), "<hallFloorTex>5__12");
        static FieldInfo _GhallCeilTex = AccessTools.Field(Type.GetType(generateSubclassName), "<hallCeilingTex>5__13");
        static FieldInfo _GclassWallTex = AccessTools.Field(Type.GetType(generateSubclassName), "<classWallTex>5__15");
        static FieldInfo _GclassFloorTex = AccessTools.Field(Type.GetType(generateSubclassName), "<classFloorTex>5__16");
        static FieldInfo _GclassCeilTex = AccessTools.Field(Type.GetType(generateSubclassName), "<classCeilingTex>5__17");
        static FieldInfo _GfacultyWallTex = AccessTools.Field(Type.GetType(generateSubclassName), "<facultyWallTex>5__18");
        static FieldInfo _GfacultyFloorTex = AccessTools.Field(Type.GetType(generateSubclassName), "<facultyFloorTex>5__19");
        static FieldInfo _GfacultyCeilTex = AccessTools.Field(Type.GetType(generateSubclassName), "<facultyCeilingTex>5__20");

        static MethodInfo _FrameShouldEnd = AccessTools.Method(typeof(LevelBuilder), "FrameShouldEnd");

        static FieldInfo _levelInProgress = AccessTools.Field(typeof(LevelBuilder), "levelInProgress");

        static Dictionary<string, SelectedGenTextures> currentRoomTextureGroup = new Dictionary<string, SelectedGenTextures>();

        static void ResetExtraData()
        {
            currentRoomTextureGroup = new Dictionary<string, SelectedGenTextures>();
            // note: actually loading in the custom data happens in callPostSpecial since that is the earliest that any room generation code is done.
        }

        static void CallPreOffice(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeOffice);
        }

        static void CallPreClassroom(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeClassroom);
        }

        static void CallPreFaculty(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeFaculty);
        }

        static void CallPreExtra(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.BeforeExtraRooms);
        }

        static void CallPostAll(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            CallGenerateGroups(generator, RoomGroupPriority.AfterAll);
        }

        static void CallPostSpecial(LevelGenerator generator, object weirdGenInstance)
        {
            if (!(generator.ld is CustomLevelObject)) return;
            _FrameShouldEnd.Invoke(generator, null);
            // add all the texture stuff
            foreach (RoomTextureGroup group in ((CustomLevelObject)generator.ld).additionalTextureGroups)
            {
                if (group.name == "hall" || group.name == "faculty" || group.name == "class") return;
                currentRoomTextureGroup.Add(group.name, new SelectedGenTextures()
                {
                    wall=WeightedTexture2D.ControlledRandomSelection(group.potentialWallTextures, generator.controlledRNG),
                    ceiling = WeightedTexture2D.ControlledRandomSelection(group.potentialCeilTextures, generator.controlledRNG),
                    floor = WeightedTexture2D.ControlledRandomSelection(group.potentialFloorTextures, generator.controlledRNG)
                });
            }
            currentRoomTextureGroup.Add("hall", new SelectedGenTextures()
            {
                wall=(Texture2D)_GhallWallTex.GetValue(weirdGenInstance),
                ceiling=(Texture2D)_GhallCeilTex.GetValue(weirdGenInstance),
                floor=(Texture2D)_GhallFloorTex.GetValue(weirdGenInstance)
            });
            currentRoomTextureGroup.Add("class", new SelectedGenTextures()
            {
                wall = (Texture2D)_GclassWallTex.GetValue(weirdGenInstance),
                ceiling = (Texture2D)_GclassCeilTex.GetValue(weirdGenInstance),
                floor = (Texture2D)_GclassFloorTex.GetValue(weirdGenInstance)
            });
            currentRoomTextureGroup.Add("faculty", new SelectedGenTextures()
            {
                wall = (Texture2D)_GfacultyWallTex.GetValue(weirdGenInstance),
                ceiling = (Texture2D)_GfacultyCeilTex.GetValue(weirdGenInstance),
                floor = (Texture2D)_GfacultyFloorTex.GetValue(weirdGenInstance)
            });
            ((CustomLevelObject)generator.ld).additionalRoomTypes.Where(x => x.spawnMethod == RoomGroupSpawnMethod.SpecialRooms).Do(x => GenerateSpecialRoomGroup(generator, x));
        }

        static void CallGenerateGroups(LevelGenerator generator, RoomGroupPriority priority)
        {
            ((CustomLevelObject)generator.ld).additionalRoomTypes.Where(x => x.priority == priority).Do(x => GenerateGroup(generator, x));
        }

        readonly static MethodInfo _RandomlyPlaceRoom = typeof(LevelGenerator).GetMethod("RandomlyPlaceRoom", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(RoomAsset), typeof(bool), typeof(RoomController).MakeByRefType() }, null);

        readonly static FieldInfo _potentialRoomSpawns = AccessTools.Field(typeof(LevelBuilder), "potentialRoomSpawns");

        static void GenerateSpecialRoomGroup(LevelGenerator generator, RoomTypeGroup group)
        {
            int roomCount = generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1); // Get the room count
            for (int i = 0; i < roomCount; i++)
            {
                generator.UpdatePotentialSpawnsForPlots(generator.controlledRNG.NextDouble() < group.stickToHallChance);
                WeightedSelection<RoomAsset>[] potentialRooms = group.potentialAssets;
                object[] parameters = new object[] { WeightedSelection<RoomAsset>.ControlledRandomSelection(potentialRooms, generator.controlledRNG), false, null };
                _RandomlyPlaceRoom.Invoke(generator, parameters);
                ((List<RoomController>)_standardRooms.GetValue(generator)).Add((RoomController)parameters[2]);
            }
        }

        //thank you pixelguy

        readonly static MethodInfo _gen_weightfrompos = AccessTools.Method(typeof(LevelBuilder), "WeightFromPos", new Type[] { typeof(IntVector2), typeof(IntVector2) });
        readonly static MethodInfo _gen_weightfromroom = AccessTools.Method(typeof(LevelBuilder), "WeightFromRoom", new Type[] { typeof(RoomController) });
        static void GenerateChainGroup(LevelGenerator generator, RoomTypeGroup group) // CREDIT PIXEL GUY FOR THIS
        {
            var invalidRooms = group.potentialAssets.Where(x => x.selection.potentialDoorPositions.Count == 0).ToArray();
            if (invalidRooms.Length > 0)
            {
                StringBuilder bl = new StringBuilder();
                for (int i = 0; i < invalidRooms.Length; i++)
                    bl.Append(invalidRooms[i].selection.name + (i >= invalidRooms.Length - 1 ? string.Empty : " -- "));
                throw new ArgumentException("Failed to generate a Room group due to empty potential door positions. \nThe following rooms have empty potentialDoorPositions: " + bl.ToString());
            }


            generator.UpdatePotentialSpawnsForRooms(generator.controlledRNG.NextDouble() < group.stickToHallChance);
            int count = generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1); // Get the room count
            var assets = new List<WeightedRoomAsset>(group.potentialAssets); // Get the room potential assets
            bool isFirstRoom = true;
            var rooms = new List<RoomController>();

            for (int i = 0; i < count; i++)
            {
                if (assets.Count == 0) break;


                int num14 = WeightedSelection<RoomAsset>.ControlledRandomIndex(assets.ToArray(), generator.controlledRNG);
                RoomController roomcontrol = null;

                var parameters = new object[] { assets[num14].selection, group.generateDoors, roomcontrol }; // An interesting workaround for out parameters
                var potentialRooms = (List<WeightedRoomSpawn>)_potentialRoomSpawns.GetValue(generator);
                if (!isFirstRoom)
                {

                    potentialRooms.Clear();
                    foreach (var room in rooms)
                    {
                        foreach (IntVector2 doorPositions in room.potentialDoorPositions)
                        {
                            foreach (RoomSpawn roomSpawn2 in generator.Ec.GetPotentialRoomSpawnsAtCell(doorPositions))
                            {
                                WeightedRoomSpawn weightedRoomSpawn2 = new WeightedRoomSpawn
                                {
                                    selection = roomSpawn2,
                                    weight = (int)_gen_weightfrompos.Invoke(generator, new object[] { roomSpawn2.position, doorPositions })
                                };
                                weightedRoomSpawn2.weight += (int)_gen_weightfromroom.Invoke(generator, new object[] { room });
                                weightedRoomSpawn2.weight = Mathf.Max(1, weightedRoomSpawn2.weight); // OH MY FUCKING GOD, WHY THIS CAN BE BELOW 0
                                potentialRooms.Add(weightedRoomSpawn2);
                            }
                        }
                        _FrameShouldEnd.Invoke(generator, null);
                    }

                }


                if ((bool)_RandomlyPlaceRoom.Invoke(generator, parameters))
                {
                    roomcontrol = (RoomController)parameters[2];
                    ((List<RoomController>)_standardRooms.GetValue(generator)).Add(roomcontrol);
                    isFirstRoom = false;
                    rooms.Add(roomcontrol);
                }
                else
                {
                    assets.RemoveAt(num14);
                    i--;
                }
                _FrameShouldEnd.Invoke(generator, null);

            }
        }

        //thank you pixelguy
        static void GenerateExitGroup(LevelGenerator generator, RoomTypeGroup group)
        {
            List<Direction> availableDirs = Directions.All(); // All dirs

            int count = generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1); // Get the room count
            List<WeightedRoomAsset> potentialAssets = new List<WeightedRoomAsset>(group.potentialAssets); // Get the room potential assets
            bool usedDir = false;
            bool stick = generator.controlledRNG.NextDouble() < group.stickToHallChance;
            generator.UpdatePotentialSpawnsForRooms(stick);
            for (int i = 0; i < count; i++)
            {
                // Elevator stuff
                if (potentialAssets.Count == 0 || availableDirs.Count == 0)
                    break;


                int dIdx = generator.controlledRNG.Next(availableDirs.Count);
                Direction dir = availableDirs[dIdx];

                // Generate rooms here
                int index = WeightedSelection<RoomAsset>.ControlledRandomIndex(potentialAssets.ToArray(), generator.controlledRNG);
                RoomController roomcontrol = null;

                var parameters = new object[] { potentialAssets[index].selection, group.generateDoors, roomcontrol }; // An interesting workaround for out parameters

            repeatExitProcess: // repeat the exit with the same dir
                ((List<WeightedRoomSpawn>)_potentialRoomSpawns.GetValue(generator)).RemoveAll(x => x.selection.direction != dir);
                if ((bool)_RandomlyPlaceRoom.Invoke(generator, parameters))
                {
                    stick = generator.controlledRNG.NextDouble() < group.stickToHallChance;
                    generator.UpdatePotentialSpawnsForRooms(stick);
                    RoomController rm = (RoomController)parameters[2];
                    ((List<RoomController>)_standardRooms.GetValue(generator)).Add(rm);
                    if (currentRoomTextureGroup.ContainsKey(group.textureGroupName) && !potentialAssets[index].selection.keepTextures)
                    {
                        rm.florTex = currentRoomTextureGroup[group.textureGroupName].floor;
                        rm.wallTex = currentRoomTextureGroup[group.textureGroupName].wall;
                        rm.ceilTex = currentRoomTextureGroup[group.textureGroupName].ceiling;
                    }
                    availableDirs.RemoveAt(dIdx); // ONLY remove the direction when successfully spawn the room
                    usedDir = false;
                }
                else
                {
                    if (usedDir || group.stickToHallChance >= 1f)
                    {
                        potentialAssets.RemoveAt(index);
                        usedDir = false;
                    }
                    else
                    {
                        usedDir = true;
                        generator.UpdatePotentialSpawnsForRooms(!stick);
                        goto repeatExitProcess;
                    }
                    i--;
                }
                _FrameShouldEnd.Invoke(generator, null);
            }
        }

        static void GenerateGroup(LevelGenerator generator, RoomTypeGroup group)
        {
            if (group.spawnMethod == RoomGroupSpawnMethod.SpecialRooms) return;
            if (group.spawnMethod == RoomGroupSpawnMethod.Exits)
            {
                GenerateExitGroup(generator, group);
                return;
            }
            if (group.spawnMethod == RoomGroupSpawnMethod.Chain)
            {
                GenerateChainGroup(generator, group);
                return;
            }
            generator.UpdatePotentialSpawnsForRooms(generator.controlledRNG.NextDouble() < (double)group.stickToHallChance);
            List<WeightedRoomAsset> potentialRoomsList = new List<WeightedRoomAsset>(group.potentialAssets);
            int roomCount = Mathf.Max(generator.controlledRNG.Next(group.minRooms, group.maxRooms + 1), 0);
            for (int i = 0; i < roomCount; i++)
            {
                if (potentialRoomsList.Count == 0) break;
                // do the typical spawn behavior code, select an asset, try it, and then remove it if it fails.
                WeightedSelection<RoomAsset>[] potentialAssets = group.potentialAssets.ToArray();
                int index = WeightedSelection<RoomAsset>.ControlledRandomIndex(potentialAssets, generator.controlledRNG);
                object[] parameters = new object[] { potentialAssets[index].selection, group.generateDoors, null};
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
                    if (currentRoomTextureGroup.ContainsKey(group.textureGroupName) && !potentialAssets[index].selection.keepTextures)
                    {
                        rm.florTex = currentRoomTextureGroup[group.textureGroupName].floor;
                        rm.wallTex = currentRoomTextureGroup[group.textureGroupName].wall;
                        rm.ceilTex = currentRoomTextureGroup[group.textureGroupName].ceiling;
                    }
                    generator.UpdatePotentialSpawnsForRooms(generator.controlledRNG.NextDouble() < (double)group.stickToHallChance);
                }
            }
        }

        static MethodInfo _CallPostAll = AccessTools.Method(typeof(LevelGeneratorPatches), "CallPostAll");

        static MethodInfo _CallPostSpecial = AccessTools.Method(typeof(LevelGeneratorPatches), "CallPostSpecial");
        static MethodInfo _ResetExtraData = AccessTools.Method(typeof(LevelGeneratorPatches), "ResetExtraData");

        static FieldInfo _standardRooms = AccessTools.Field(typeof(LevelGenerator), "standardRooms");
        static FieldInfo _halls = AccessTools.Field(typeof(LevelBuilder), "halls");

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
            bool patchedSpecialPostgen = false;
            bool patchedTextureReset = false;

            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                yield return instruction;
                if (!patchedTextureReset)
                {
                    if (i + 3 > codeInstructions.Length - 1) continue;
                    if (
                        (codeInstructions[i + 0].opcode == OpCodes.Ldloc_2) &&
                        (codeInstructions[i + 1].opcode == OpCodes.Ldc_I4_1) &&
                        (codeInstructions[i + 2].opcode == OpCodes.Stfld) &&
                        ((FieldInfo)codeInstructions[i + 2].operand == _levelInProgress)
                        )
                    {
                        yield return new CodeInstruction(OpCodes.Call, _ResetExtraData); //yeah
                        patchedTextureReset = true;
                    }
                }
                if (!patchedSpecialPostgen)
                {
                    if (i + 4 > codeInstructions.Length - 1) continue;
                    if (
                        (codeInstructions[i + 0].opcode == OpCodes.Ldloc_2) &&
                        (codeInstructions[i + 1].opcode == OpCodes.Ldfld) &&
                        (codeInstructions[i + 2].opcode == OpCodes.Ldnull) &&
                        (codeInstructions[i + 3].opcode == OpCodes.Callvirt) &&
                        ((FieldInfo)codeInstructions[i + 1].operand == _halls)
                        )
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_2); //this
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //weird generator subclass
                        yield return new CodeInstruction(OpCodes.Call, _CallPostSpecial);
                        patchedSpecialPostgen = true;
                    }
                }
                if (notPatched.Count == 0)
                {
                    if (patchedLast) continue;
                    // all the preGen stuff is done, now search only for the last two things we need
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
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //weird generator subclass
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
                        yield return new CodeInstruction(OpCodes.Ldarg_0); //weird generator subclass
                        yield return new CodeInstruction(OpCodes.Call, preGenDict[notPatched[z]]);
                        notPatched.RemoveAt(z);
                    }
                }
            }
            if (notPatched.Count != 0) throw new Exception("Unable to patch LevelGenerator.Generate preGenDict!");
            if (!patchedLast) throw new Exception("Unable to patch LevelGenerator.Generate CallPostAll");
            if (!patchedSpecialPostgen) throw new Exception("Unable to patch LevelGenerator.Generate CallPostSpecial");
            if (!patchedTextureReset) throw new Exception("Unable to patch LevelGenerator.Generate ResetExtraData");
            yield break;
        }
    }
}
