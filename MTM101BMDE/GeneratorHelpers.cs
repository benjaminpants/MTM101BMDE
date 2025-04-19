using MTM101BaldAPI.Registers.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class GeneratorHelpers
    {
        public static GameButton BuildInAreaWithColor(EnvironmentController ec, IntVector2 rangeStartPosition, int buttonRange, GameObject receiver, GameButtonBase buttonPre, string colorKey, System.Random cRng)
        {
            GameButton gb = (GameButton)GameButton.BuildInArea(ec, rangeStartPosition, buttonRange, receiver, buttonPre, cRng);
            ButtonColorManager.ApplyButtonMaterials(gb, colorKey);
            return gb;
        }
    }
    
}
