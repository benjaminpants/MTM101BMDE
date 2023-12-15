using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace MTM101BaldAPI.Registers.Buttons
{
    public struct ButtonMaterials
    {
        public Material buttonPressed;
        public Material buttonUnpressed;
        public Material leverUp;
        public Material leverDown;
        public Color color;
        public string name;
    }

    public static class ButtonColorExtensions
    {
        public static void ChangeColor(this GameButton me, ButtonMaterials bm)
        {
            ButtonColorManager.ApplyButtonMaterials(me, bm);
        }

        public static void ChangeColor(this GameLever me, ButtonMaterials bm)
        {
            ButtonColorManager.ApplyLeverMaterials(me, bm);
        }

        public static void ChangeColor(this GameButton me, string colorKey)
        {
            ButtonColorManager.ApplyButtonMaterials(me, colorKey);
        }

        public static void ChangeColor(this GameLever me, string colorKey)
        {
            ButtonColorManager.ApplyLeverMaterials(me, colorKey);
        }
    }

    public static class ButtonColorManager
    {
        private static Dictionary<string, ButtonMaterials> _buttonColors = new Dictionary<string, ButtonMaterials>();

        internal static Material BaseButtonMaterial_Unpressed;
        internal static Material BaseButtonMaterial_Pressed;
        internal static Material BaseLeverMaterial_Up;
        internal static Material BaseLeverMaterial_Down;

        static FieldInfo buttonPressedF = AccessTools.Field(typeof(GameButton), "pressed");
        static FieldInfo buttonUnpressedF = AccessTools.Field(typeof(GameButton), "unPressed");
        static FieldInfo buttonMeshRenderer = AccessTools.Field(typeof(GameButton), "meshRenderer");

        static FieldInfo leverMeshRenderer = AccessTools.Field(typeof(GameLever), "meshRenderer");
        static FieldInfo leverOffMat = AccessTools.Field(typeof(GameLever), "offMat");
        static FieldInfo leverOnMat = AccessTools.Field(typeof(GameLever), "onMat");

        public static void ApplyButtonMaterials(GameButton applyTo, ButtonMaterials toApply)
        {
            MeshRenderer mr = ((MeshRenderer)buttonMeshRenderer.GetValue(applyTo));
            Material oldPressed = (Material)buttonPressedF.GetValue(applyTo);
            buttonPressedF.SetValue(applyTo, toApply.buttonPressed);
            buttonUnpressedF.SetValue(applyTo, toApply.buttonUnpressed);
            mr.sharedMaterial = (mr.sharedMaterial == oldPressed ? toApply.buttonPressed : toApply.buttonUnpressed);
        }

        public static void ApplyButtonMaterials(GameButton applyTo, string colorName)
        {
            ApplyButtonMaterials(applyTo, buttonColors[colorName]);
        }

        public static void ApplyLeverMaterials(GameLever applyTo, ButtonMaterials toApply)
        {
            // why is the lever down mat the off mat? that's weird but. whatever
            MeshRenderer mr = ((MeshRenderer)leverMeshRenderer.GetValue(applyTo));
            Material oldOff = (Material)leverOffMat.GetValue(applyTo);
            leverOnMat.SetValue(applyTo, toApply.leverUp);
            leverOffMat.SetValue(applyTo, toApply.leverDown);
            mr.sharedMaterial = (mr.sharedMaterial == oldOff ? toApply.leverDown : toApply.leverUp);
        }

        internal static void AddRed()
        {
            if (_buttonColors.Count != 0) throw new Exception("AddRed called twice!");
            _buttonColors.Add("Red", new ButtonMaterials()
            {
                buttonUnpressed = BaseButtonMaterial_Unpressed,
                buttonPressed = BaseButtonMaterial_Pressed,
                leverUp = BaseLeverMaterial_Up,
                leverDown = BaseLeverMaterial_Down,
                color = new Color(1f, 0f, 0f, 0f),
                name = "Red"
            });
        }

        public static void ApplyLeverMaterials(GameLever applyTo, string colorName)
        {
            ApplyLeverMaterials(applyTo, buttonColors[colorName]);
        }
        public static ButtonMaterials CreateButtonMaterial(string key, Color color)
        {
            if (buttonColors.ContainsKey(key))
            {
                Debug.LogWarningFormat("Attempted to add already existing button color: {0}!", key);
                return buttonColors[key];
            }
            Material leverUpMaterial = new Material(BaseLeverMaterial_Up);
            Material leverDownMaterial = new Material(BaseLeverMaterial_Down);
            Material pressedMaterial = new Material(BaseButtonMaterial_Pressed);
            Material unpressedMaterial = new Material(BaseButtonMaterial_Unpressed);
            color = new Color(color.r, color.g, color.b, 0f); //make sure alpha is 0
            // button material creation
            pressedMaterial.name = String.Format("Button_{0}_Pressed", key);
            pressedMaterial.SetColor("_TextureColor", color);
            unpressedMaterial.name = String.Format("Button_{0}_Unpressed", key);
            unpressedMaterial.SetColor("_TextureColor", color);
            // lever material creation
            leverUpMaterial.name = String.Format("Lever_{0}_Up", key);
            leverUpMaterial.SetColor("_TextureColor", color);
            leverDownMaterial.name = String.Format("Lever_{0}_Down", key);
            leverDownMaterial.SetColor("_TextureColor", color);
            ButtonMaterials newBut = new ButtonMaterials()
            {
                buttonPressed = pressedMaterial,
                buttonUnpressed = unpressedMaterial,
                color = color,
                name = key,
                leverUp = leverUpMaterial,
                leverDown = leverDownMaterial
            };
            buttonColors.Add(key, newBut);
            return newBut;
        }

        internal static void InitializeButtonColors()
        {
            List<Material> materials = Resources.FindObjectsOfTypeAll<Material>().ToList();
            BaseButtonMaterial_Unpressed = materials.Find(x => x.name == "Button_Red_Unpressed");
            BaseButtonMaterial_Pressed = materials.Find(x => x.name == "Button_Red_Pressed");
            BaseLeverMaterial_Down = materials.Find(x => x.name == "Lever_Red_Down");
            BaseLeverMaterial_Up = materials.Find(x => x.name == "Lever_Red_Up");
            // make sure we crash HERE if any of these are null(makes things easier to debug if mystman12 renames these)
            Assert.IsNotNull(BaseButtonMaterial_Unpressed);
            Assert.IsNotNull(BaseButtonMaterial_Pressed);
            Assert.IsNotNull(BaseLeverMaterial_Down);
            Assert.IsNotNull(BaseLeverMaterial_Up);

            // handle all the basic colors people may want so we dont have a million mods trying to create the same colors
            AddRed();
            CreateButtonMaterial("Orange", new Color(1f, 1f, 0f));
            CreateButtonMaterial("Yellow", new Color(1f, 1f, 0f));
            CreateButtonMaterial("Green", Color.green);
            CreateButtonMaterial("Cyan", new Color(0f, 1f, 1f));
            CreateButtonMaterial("Blue", Color.blue);
            CreateButtonMaterial("Purple", new Color(0.5f, 1f, 0f));
            CreateButtonMaterial("Magenta", new Color(1f, 0f, 1f));
            CreateButtonMaterial("Pink", new Color(1f, 0.5f, 1f));
            CreateButtonMaterial("White", Color.white);

            GeneratorHelpers.leverPrefab = Resources.FindObjectsOfTypeAll<GameLever>().First();
            Assert.IsNotNull(GeneratorHelpers.leverPrefab);
        }

        public static Dictionary<string, ButtonMaterials> buttonColors
        {
            get
            {
                if (_buttonColors.Count == 0)
                {
                    throw new NullReferenceException("Attempted to access buttonColors before it is defined!"); //give mods an error if they try to access button colors before they are ready
                }
                return _buttonColors;
            }
        }
    }
}
