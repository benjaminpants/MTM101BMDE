using HarmonyLib;
using MTM101BaldAPI.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    public class MainGameManagerBuilder<T> : BaseGameManagerBuilder<T> where T : MainGameManager
    {
        protected HappyBaldi happyBaldiPrefab;
        protected SoundObject happySoundObject;
        protected SceneObject customPitstop;
        protected SoundObject allNotebooks;

        public MainGameManagerBuilder()
        {
            npcSpawnMode = GameManagerNPCAutomaticSpawn.Never;
        }

        /// <summary>
        /// Sets the happy Baldi prefab to be used for this manager.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public MainGameManagerBuilder<T> SetHappyBaldi(HappyBaldi prefab)
        {
            happyBaldiPrefab = prefab;
            return this;
        }

        /// <summary>
        /// Sets a soundobject to be used for Happy Baldi.
        /// Will automatically create the appropiate prefab.
        /// </summary>
        /// <param name="greeting"></param>
        /// <returns></returns>
        public MainGameManagerBuilder<T> SetHappyBaldi(SoundObject greeting)
        {
            happyBaldiPrefab = null;
            happySoundObject = greeting;
            return this;
        }

        /// <summary>
        /// Sets a custom pitstop SceneObject, otherwise use the standard pitstop SceneObject.
        /// </summary>
        /// <param name="pitstop"></param>
        /// <returns></returns>
        public MainGameManagerBuilder<T> SetCustomPitstop(SceneObject pitstop)
        {
            customPitstop = pitstop;
            return this;
        }


        /// <summary>
        /// Sets the sound that is played when all notebooks are collected.
        /// </summary>
        /// <param name="allNotebooks"></param>
        /// <returns></returns>
        public MainGameManagerBuilder<T> SetAllNotebooksSound(SoundObject allNotebooks)
        {
            this.allNotebooks = allNotebooks;
            return this;
        }


        static FieldInfo _happyBaldiPre = AccessTools.Field(typeof(MainGameManager), "happyBaldiPre");
        static FieldInfo _audIntro = AccessTools.Field(typeof(HappyBaldi), "audIntro");
        static FieldInfo _pitstop = AccessTools.Field(typeof(MainGameManager), "pitstop");
        static FieldInfo _ambience = AccessTools.Field(typeof(MainGameManager), "ambience");
        static FieldInfo _allNotebooksNotification = AccessTools.Field(typeof(MainGameManager), "allNotebooksNotification");
        public override T Build()
        {
            T comp = base.Build();
            if (happyBaldiPrefab != null)
            {
                _happyBaldiPre.SetValue(comp, happyBaldiPrefab);
            }
            else
            {
                if (happySoundObject != null)
                {
                    HappyBaldi newPrefab = GameObject.Instantiate<HappyBaldi>(MTM101BaldiDevAPI.AssetMan.Get<HappyBaldi>("HappyBaldi3"), MTM101BaldiDevAPI.prefabTransform);
                    newPrefab.name = "HappyBaldi_" + comp.name;
                    _audIntro.SetValue(newPrefab, happySoundObject);
                    _happyBaldiPre.SetValue(comp, newPrefab);
                }
                else
                {
                    _happyBaldiPre.SetValue(comp, MTM101BaldiDevAPI.AssetMan.Get<HappyBaldi>("HappyBaldi3"));
                }
            }
            if (customPitstop != null)
            {
                _pitstop.SetValue(comp, customPitstop);
            }
            else
            {
                _pitstop.SetValue(comp, MTM101BaldiDevAPI.AssetMan.Get<SceneObject>("Pitstop"));
            }
            _allNotebooksNotification.SetValue(comp, allNotebooks);
            Ambience ambienceGameObject = GameObject.Instantiate<Ambience>(MTM101BaldiDevAPI.AssetMan.Get<Ambience>("AmbienceTemplate"), comp.transform);
            ambienceGameObject.name = "Ambience";
            _ambience.SetValue(comp, ambienceGameObject);
            return comp;
        }
    }
}
