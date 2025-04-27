using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    /// <summary>
    /// The enum used for GameManager builders that determines when NPCs are spawned
    /// </summary>
    public enum GameManagerNPCAutomaticSpawn
    {
        /// <summary>
        /// NPCs are never automatically spawned. SpawnNPCs must be invoked manually/in an overload.
        /// </summary>
        Never,
        /// <summary>
        /// NPCs are spawned as soon as the GameManager initializes.
        /// </summary>
        OnInitialize,
        /// <summary>
        /// NPCs are spawned as soon as the player leaves the spawn elevator
        /// </summary>
        OnSpawnExit
    }


    public class BaseGameManagerBuilder<T> where T : BaseGameManager
    {
        private float gradeValue = 1f;
        private float notebookAngerVal = 1f;
        private int levelNo = 0;
        private string managerNameKey = null;
        private string name;
        private ElevatorScreen customElevatorScreen;
        protected bool beginPlayImmediately = false;
        protected GameManagerNPCAutomaticSpawn npcSpawnMode = GameManagerNPCAutomaticSpawn.OnSpawnExit;

        /// <summary>
        /// Sets the name of the created prefab.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetObjectName(string name)
        {
            this.name = name;
            return this;
        }

        /// <summary>
        /// Sets the manager name key. Currently only used by Steam highlights.
        /// Mode_HideSeek is used
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetNameKey(string name)
        {
            managerNameKey = name;
            return this;
        }

        /// <summary>
        /// Sets the automatic NPC spawn mode for the manager builder.
        /// (Behind the scenes, this sets spawnImmediately and spawnNpcsOnInit)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual BaseGameManagerBuilder<T> SetNPCSpawnMode(GameManagerNPCAutomaticSpawn value)
        {
            npcSpawnMode = value;
            return this;
        }

        /// <summary>
        /// Sets the multiplier for the grade score for the overall level.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetGradeValue(float value)
        {
            gradeValue = value;
            return this;
        }

        /// <summary>
        /// Sets
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetCustomElevatorPrefab(ElevatorScreen screen)
        {
            customElevatorScreen = screen;
            return this;
        }

        /// <summary>
        /// Sets the amount of anger Baldi gains when a notebook is collected.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetNotebookAngerValue(float value)
        {
            notebookAngerVal = value;
            return this;
        }

        /// <summary>
        /// Sets the level number for this GameManager.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BaseGameManagerBuilder<T> SetLevelNumber(int value)
        {
            levelNo = value;
            return this;
        }

        static FieldInfo _gradeValue = AccessTools.Field(typeof(BaseGameManager), "gradeValue");
        static FieldInfo _notebookAngerVal = AccessTools.Field(typeof(BaseGameManager), "notebookAngerVal");
        static FieldInfo _levelNo = AccessTools.Field(typeof(BaseGameManager), "levelNo");
        static FieldInfo _managerNameKey = AccessTools.Field(typeof(BaseGameManager), "managerNameKey");
        static FieldInfo _elevatorScreenPre = AccessTools.Field(typeof(BaseGameManager), "elevatorScreenPre");

        /// <summary>
        /// Creates the BaseGameManager prefab.
        /// </summary>
        /// <returns></returns>
        public virtual T Build()
        {
            GameObject baseObj = new GameObject();
            if (name == null)
            {
                baseObj.name = typeof(T).Name;
            }
            else
            {
                baseObj.name = name;
            }
            baseObj.ConvertToPrefab(true);
            T comp = baseObj.AddComponent<T>();
            _gradeValue.SetValue(comp, gradeValue);
            _notebookAngerVal.SetValue(comp, notebookAngerVal);
            _levelNo.SetValue(comp, levelNo);
            if (managerNameKey == null)
            {
                if ((typeof(T).IsSubclassOf(typeof(MainGameManager))) || (typeof(T) == typeof(MainGameManager)))
                {
                    _managerNameKey.SetValue(comp, "Mode_HideAndSeek");
                }
                else
                {
                    _managerNameKey.SetValue(comp, "Mode_Undefined");
                }
            }
            else
            {
                _managerNameKey.SetValue(comp, managerNameKey);
            }

            // these default to false but just incase
            comp.spawnNpcsOnInit = false;
            comp.spawnImmediately = false;
            switch (npcSpawnMode)
            {
                case GameManagerNPCAutomaticSpawn.Never:
                    break;
                case GameManagerNPCAutomaticSpawn.OnInitialize:
                    comp.spawnNpcsOnInit = true;
                    break;
                case GameManagerNPCAutomaticSpawn.OnSpawnExit:
                    comp.spawnImmediately = true;
                    break;
            }
            _elevatorScreenPre.SetValue(comp, customElevatorScreen == null ? MTM101BaldiDevAPI.AssetMan.Get<ElevatorScreen>("ElevatorScreen") : customElevatorScreen);
            comp.ReflectionSetVariable("destroyOnLoad", true);
            return comp;
        }
    }
}
