using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI
{
    public static class ScriptableObjectHelpers
    {
        /// <summary>
        /// Creates a ScriptableObject of type CloneType and copies the public values of OriginalType to CloneType.
        /// <c>This clones by reference!</c>
        /// </summary>
        /// <typeparam name="OriginalType"></typeparam>
        /// <typeparam name="CloneType"></typeparam>
        /// <param name="original"></param>
        /// <returns>The clone with all public properties from the original copied over.</returns>
        public static CloneType CloneScriptableObject<OriginalType, CloneType>(OriginalType original) where OriginalType : ScriptableObject where CloneType : OriginalType
        {
            CloneType newObject = ScriptableObject.CreateInstance<CloneType>();
            // transfer the data
            FieldInfo[] foes = typeof(OriginalType).GetFields();
            foreach (FieldInfo fo in foes)
            {
                fo.SetValue(newObject, fo.GetValue(original));
            }
            return newObject;
        }
    }
}
