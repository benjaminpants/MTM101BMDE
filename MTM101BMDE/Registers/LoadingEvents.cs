using System;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public static class LoadingEvents
    {
        internal static Action OnAllAssetsLoaded;
        internal static Action OnAllAssetsLoadedPost;

        /// <summary>
        /// Registers an event that gets called when every asset has been loaded into memory and can be sorted through with Resources.FindObjectsOfTypeAll.
        /// </summary>
        /// <param name="toRegister"></param>
        /// <param name="post">If true, this will be called after the initial call, this is useful if you need to replace all references to something</param>
        public static void RegisterOnAssetsLoaded(Action toRegister, bool post)
        {
            if (post)
            {
                OnAllAssetsLoadedPost += toRegister;
            }
            else
            {
                OnAllAssetsLoaded += toRegister;
            }
        }
    }
}
