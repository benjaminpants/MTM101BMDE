using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace MTM101BaldAPI.ObjectCreation
{
    // not done, comment out so poeple dont try to use it
    /*
    public class EndlessGameManagerBuilder<T> : BaseGameManagerBuilder<T> where T : EndlessGameManager
    {
        protected HappyBaldi happyBaldiPrefab;
        protected SoundObject happySoundObject;
        protected EndlessLevel endlessLevel;
        protected float angerRate = 0.01f;
        protected float angerRateRate = 0.003f;

        public EndlessGameManagerBuilder()
        {
            npcSpawnMode = GameManagerNPCAutomaticSpawn.Never;
        }


        /// <summary>
        /// Sets the endless level enum.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public EndlessGameManagerBuilder<T> SetLevelEnum(EndlessLevel type)
        {
            endlessLevel = type;
            return this;
        }

        /// <summary>
        /// Sets the happy Baldi prefab to be used for this manager.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public EndlessGameManagerBuilder<T> SetHappyBaldi(HappyBaldi prefab)
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
        public EndlessGameManagerBuilder<T> SetHappyBaldi(SoundObject greeting)
        {
            happyBaldiPrefab = null;
            happySoundObject = greeting;
            return this;
        }

        static FieldInfo _happyBaldiPre = AccessTools.Field(typeof(EndlessGameManager), "happyBaldiPre");
        static FieldInfo _audIntro = AccessTools.Field(typeof(HappyBaldi), "audIntro");
        static FieldInfo _ambience = AccessTools.Field(typeof(EndlessGameManager), "ambience");
        static FieldInfo _resultsScreen = AccessTools.Field(typeof(EndlessGameManager), "resultsScreen");
        static FieldInfo _rankText = AccessTools.Field(typeof(EndlessGameManager), "rankText");
        static FieldInfo _scoreText = AccessTools.Field(typeof(EndlessGameManager), "scoreText");
        static FieldInfo _congratsText = AccessTools.Field(typeof(EndlessGameManager), "congratsText");
        static FieldInfo _endlessLevel = AccessTools.Field(typeof(EndlessGameManager), "endlessLevel");
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
            Ambience ambienceGameObject = GameObject.Instantiate<Ambience>(MTM101BaldiDevAPI.AssetMan.Get<Ambience>("AmbienceTemplate"), comp.transform);
            ambienceGameObject.name = "Ambience";
            _ambience.SetValue(comp, ambienceGameObject);
            Canvas scoreCanvas = GameObject.Instantiate<Canvas>(MTM101BaldiDevAPI.AssetMan.Get<Canvas>("EndlessScoreTemplate"), comp.transform);
            scoreCanvas.name = "Score";
            _resultsScreen.SetValue(comp, scoreCanvas);
            _rankText.SetValue(comp, scoreCanvas.transform.Find("Rank").GetComponent<TMP_Text>());
            _scoreText.SetValue(comp, scoreCanvas.transform.Find("Score").GetComponent<TMP_Text>());
            _congratsText.SetValue(comp, scoreCanvas.transform.Find("Congrats").GetComponent<TMP_Text>());
            _endlessLevel.SetValue(comp, endlessLevel);

            return comp;
        }
    }*/
}
