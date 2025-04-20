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
    public static class ButtonColorExtensions
    {
        public static void ChangeColor(this GameButtonBase me)
        {
            //ButtonColorManager.ApplyButtonMaterials(me, bm);
        }
    }


    /// <summary>
    /// Stores the color handler info for the respective button.
    /// </summary>
    public class ButtonColorHandlerInfo
    {
        /// <summary>
        /// The Material, FieldInfo(for the variables that store the materials), and a formatting string used for naming the newly generated materials.
        /// </summary>
        public (Material, FieldInfo, string)[] materialInfoPairs;
        public FieldInfo rendererField;

        public FieldInfo onField;
        /// <summary>
        /// The color that won't have a new material generated for it.
        /// </summary>
        public string defaultColor;

        public ButtonColorHandlerInfo((Material, FieldInfo, string)[] materialInfoPairs, FieldInfo rendererField, FieldInfo onField, string defaultColor = "Red")
        {
            this.materialInfoPairs = materialInfoPairs;
            this.rendererField = rendererField;
            this.defaultColor = defaultColor;
            this.onField = onField;
        }
    }


    public static class ButtonColorManager
    {
        private static Dictionary<Type, ButtonColorHandlerInfo> buttonColorHandlers = new Dictionary<Type, ButtonColorHandlerInfo>();
        private static Dictionary<Type, Dictionary<string, Material[]>> buttonColorMaterials = new Dictionary<Type, Dictionary<string, Material[]>>();
        private static List<string> createdColors = new List<string>() { "Red" };

        public static bool ApplyButtonMaterials(GameButtonBase button, string colorKey)
        {
            if (!createdColors.Contains(colorKey)) return false;
            return ApplyButtonMaterials(button, buttonColorMaterials[button.GetType()][colorKey]);
        }

        public static bool ApplyButtonMaterials(GameButtonBase button, Material[] customMaterials)
        {
            if (!buttonColorHandlers.ContainsKey(button.GetType())) return false;
            ButtonColorHandlerInfo info = buttonColorHandlers[button.GetType()];
            Renderer renderer = (Renderer)info.rendererField.GetValue(button);
            for (int i = 0; i < info.materialInfoPairs.Length; i++)
            {
                info.materialInfoPairs[i].Item2.SetValue(button, customMaterials[i]); // adjust the variable
            }
            if (info.onField != null)
            {
                renderer.sharedMaterial = (bool)info.onField.GetValue(button) ? customMaterials[0] : customMaterials[1];
                return true;
            }
            renderer.sharedMaterial = customMaterials[0];
            return true;
        }

        public static void CreateButtonColor(string name, Color color)
        {
            if (createdColors.Contains(name))
            {
                return;
            }
            createdColors.Add(name);
            // now... create the color.
            foreach (KeyValuePair<Type, ButtonColorHandlerInfo> info in buttonColorHandlers)
            {
                Material[] newMaterials = new Material[info.Value.materialInfoPairs.Length];
                for (int i = 0; i < info.Value.materialInfoPairs.Length; i++)
                {
                    Material newMat = new Material(info.Value.materialInfoPairs[i].Item1);
                    newMat.SetColor("_TextureColor", color);
                    newMat.name = String.Format(info.Value.materialInfoPairs[i].Item3, name);
                    newMaterials[i] = newMat;
                }
                buttonColorMaterials[info.Key].Add(name, newMaterials);
            }
        }

        // TODO: publicize this later, have it automatically add all already added colors when a new one gets added
        private static void AddButtonColorHandler(Type buttonType, ButtonColorHandlerInfo info)
        {
            buttonColorHandlers.Add(buttonType, info);
            buttonColorMaterials.Add(buttonType, new Dictionary<string, Material[]>());
            Material[] defaultMaterials = new Material[info.materialInfoPairs.Length];
            for (int i = 0; i < info.materialInfoPairs.Length; i++)
            {
                defaultMaterials[i] = info.materialInfoPairs[i].Item1;
            }
            buttonColorMaterials[buttonType].Add(info.defaultColor, defaultMaterials);
        }

        internal static void InitializeButtonColors()
        {
            List<Material> materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToList();

            // buttons
            AddButtonColorHandler(typeof(GameButton), new ButtonColorHandlerInfo(new (Material, FieldInfo, string)[] {
                (materials.Find(x => x.name == "Button_Red_Unpressed"), AccessTools.Field(typeof(GameButton), "unPressed"), "Button_{0}_Unpressed"),
                (materials.Find(x => x.name == "Button_Red_Pressed"), AccessTools.Field(typeof(GameButton), "pressed"), "Button_{0}_Pressed"),
                (materials.Find(x => x.name == "Button_RedOff_Unpressed"), AccessTools.Field(typeof(GameButton), "unPressedOff"), "Button_{0}Off_Unpressed"),
                (materials.Find(x => x.name == "Button_RedOff_Pressed"), AccessTools.Field(typeof(GameButton), "pressedOff"), "Button_{0}Off_Pressed")
            },
            AccessTools.Field(typeof(GameButton), "meshRenderer"), // the accesstool for getting the target mesh renderer
            null));
            // levers
            AddButtonColorHandler(typeof(GameLever), new ButtonColorHandlerInfo(new (Material, FieldInfo, string)[] {
                (materials.Find(x => x.name == "Lever_Red_Down"), AccessTools.Field(typeof(GameLever), "offMat"), "Lever_{0}_Down"),
                (materials.Find(x => x.name == "Lever_Red_Up"), AccessTools.Field(typeof(GameLever), "onMat"), "Lever_{0}_Up")
            },
            AccessTools.Field(typeof(GameLever), "meshRenderer"), 
            AccessTools.Field(typeof(GameLever), "on"))); // the accesstool for getting whether or not this lever/switch/whatever is active

            CreateButtonColor("Orange", new Color(1f, 1f, 0f, 0f));
            CreateButtonColor("Yellow", new Color(1f, 1f, 0f, 0f));
            CreateButtonColor("Green", new Color(0f,1f,0f,0f));
            CreateButtonColor("Lime", new Color(0.713f, 1f, 0f, 0f));
            CreateButtonColor("Cyan", new Color(0f, 1f, 1f,0f));
            CreateButtonColor("Blue", new Color(0f,0f,1f,0f));
            CreateButtonColor("Purple", new Color(0.5f, 0f, 1f, 0f));
            CreateButtonColor("Magenta", new Color(1f, 0f, 1f,0f));
            CreateButtonColor("Pink", new Color(1f, 0.5f, 1f,0f));
            CreateButtonColor("White", new Color(1f,1f,1f, 0f));
            CreateButtonColor("Gray", new Color(0.5f,0.5f,0.5f, 0f));
            CreateButtonColor("Black", new Color(0.11f, 0.11f, 0.11f, 0f));
        }
    }
}
