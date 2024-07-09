using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MTM101BaldAPI.Registers
{
    public static class LoadingEvents
    {
        internal struct LoadingEvent
        {
            public PluginInfo info;
            public IEnumerator loadingNumerator;
        }

        internal static List<LoadingEvent> LoadingEventsStart = new List<LoadingEvent>();
        internal static List<LoadingEvent> LoadingEventsPre = new List<LoadingEvent>();
        internal static List<LoadingEvent> LoadingEventsPost = new List<LoadingEvent>();

        /// <summary>
        /// Registers a loading IEnumerator that gets called when every asset has been loaded into memory and can be sorted through with Resources.FindObjectsOfTypeAll.
        /// The first yield return should be the amount of total amount of yield returns in the function as an int.
        /// The second yield return should be the initial loading text.
        /// Every yield afterwards should be a string that displays the next loading step.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="enumerator"></param>
        /// <param name="post">If true, this will be called after the initial call, this is useful if you need to replace all references to something.</param>
        public static void RegisterOnAssetsLoaded(PluginInfo info, IEnumerator enumerator, bool post)
        {
            LoadingEvent evnt = new LoadingEvent()
            {
                info = info,
                loadingNumerator = enumerator,
            };
            if (post)
            {
                LoadingEventsPost.Add(evnt);
            }
            else
            {
                LoadingEventsPre.Add(evnt);
            }
        }

        static IEnumerator DummyIEnumerator(Action toCall)
        {
            yield return 1;
            yield return "Loading...";
            toCall.Invoke();
            yield break;
        }

        /// <summary>
        /// Registers an action that gets called when every asset has been loaded into memory and can be sorted through with Resources.FindObjectsOfTypeAll.
        /// Internally, it just creates an IEnumerator that calls toRegister.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="toRegister"></param>
        /// <param name="post">If true, this will be called after the initial call, this is useful if you need to replace all references to something.</param>
        public static void RegisterOnAssetsLoaded(PluginInfo info, Action toRegister, bool post)
        {
            RegisterOnAssetsLoaded(info, DummyIEnumerator(toRegister), post);
        }

        /// <summary>
        /// Registers an event that gets called at the beginning of the Loading screen, before any assets are loaded.
        /// Typically, you'd want to just use your plugins Awake event, but if your mod tends to freeze the game at start up, this might be helpful to more properly convey what is going on.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="enumerator"></param>
        public static void RegisterOnLoadingScreenStart(PluginInfo info, IEnumerator enumerator)
        {
            LoadingEvent evnt = new LoadingEvent()
            {
                info = info,
                loadingNumerator = enumerator,
            };
            LoadingEventsStart.Add(evnt);
        }
    }
}
