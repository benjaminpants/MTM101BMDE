using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.AssetTools
{
    /*
    public static class AssetFinder
    {
        public static T[] FindAllOfType<T>(bool vanillaNonInstantiatedOnly) where T : UnityEngine.Object
        {
            return Resources.FindObjectsOfTypeAll<T>().Where(x => (!vanillaNonInstantiatedOnly) || (x.GetInstanceID() >= 0)).ToArray();
        }

        public static T[] FindAllOfTypeWithName<T>(string name, bool vanillaNonInstantiatedOnly) where T : UnityEngine.Object
        {
            return FindAllOfType<T>(vanillaNonInstantiatedOnly).Where(x => x.name == name).ToArray();
        }
    }*/
}
