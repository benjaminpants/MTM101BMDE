using MTM101BaldAPI.Registers.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class GeneratorHelpers
    {
        public static GameLever leverPrefab;

        public static GameLever BuildLeverInArea(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameLever leverPre, System.Random cRng)
        {
            GameLever lever;
            GameButton fakePrefab = new GameObject().AddComponent<GameButton>();
            GameButton gb = GameButton.BuildInArea(ec, posA, posB, buttonRange, receiver, fakePrefab, cRng); //create a new gamebutton object because we'll be deleting it anyway
            GameObject.Destroy(fakePrefab.gameObject); //don't need this anymore
            if (gb == null) return null; //the button didn't succesfully spawn so we have nowhere to put the lever
            lever = UnityEngine.Object.Instantiate(leverPre, gb.transform.parent); // parent to the tile controller
            lever.transform.rotation = gb.transform.rotation; // rotate to whatever the button was rotated to
            lever.SetUp(receiver.GetComponent<IButtonReceiver>());
            GameObject.Destroy(gb.gameObject);
            return lever;

        }

        public static GameButton BuildInAreaWithColor(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameButton buttonPre, string colorKey, System.Random cRng)
        {
            GameButton gb = GameButton.BuildInArea(ec, posA, posB, buttonRange, receiver, buttonPre, cRng);
            ButtonColorManager.ApplyButtonMaterials(gb, colorKey);
            return gb;
        }
    }
}
