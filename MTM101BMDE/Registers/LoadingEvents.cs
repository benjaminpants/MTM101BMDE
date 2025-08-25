using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTM101BaldAPI.Registers
{

    public enum LoadingEventOrder
    {
        /// <summary>
        /// Occurs before LevelObjects get converted to CustomLevelObjects
        /// </summary>
        Start,
        /// <summary>
        /// Occurs before level generator changes are performed.
        /// </summary>
        Pre,
        /// <summary>
        /// Occurs after level generator changes are performed.
        /// </summary>
        Post,
        /// <summary>
        /// Occurs after localization has been loaded and the API's highscore mode has been determined.
        /// Use sparingly, in most cases you will want to use Post instead.
        /// </summary>
        Final
    }

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
        internal static List<LoadingEvent> LoadingEventsFinal = new List<LoadingEvent>();

        internal static void SortLoadingEvents()
        {
            LoadingEventsStart = Utility.TopologicalSort(LoadingEventsStart, (LoadingEvent x) => LoadingEventsStart.Where(z => x.info.Dependencies.Where(y => y.DependencyGUID == z.info.Metadata.GUID).Count() >= 1)).ToList();
            LoadingEventsPre = Utility.TopologicalSort(LoadingEventsPre, (LoadingEvent x) => LoadingEventsPre.Where(z => x.info.Dependencies.Where(y => y.DependencyGUID == z.info.Metadata.GUID).Count() >= 1)).ToList();
            LoadingEventsPost = Utility.TopologicalSort(LoadingEventsPost, (LoadingEvent x) => LoadingEventsPost.Where(z => x.info.Dependencies.Where(y => y.DependencyGUID == z.info.Metadata.GUID).Count() >= 1)).ToList();
            LoadingEventsFinal = Utility.TopologicalSort(LoadingEventsFinal, (LoadingEvent x) => LoadingEventsFinal.Where(z => x.info.Dependencies.Where(y => y.DependencyGUID == z.info.Metadata.GUID).Count() >= 1)).ToList();
        }

        /// <summary>
        /// Registers a loading IEnumerator that gets called when every asset has been loaded into memory and can be sorted through with Resources.FindObjectsOfTypeAll.
        /// The first yield return should be the amount of total amount of yield returns in the function as an int.
        /// The second yield return should be the initial loading text.
        /// Every yield afterwards should be a string that displays the next loading step.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="enumerator"></param>
        /// <param name="order">The event in which assets will be loaded. Refer to LoadingEventOrder for more information on when each enum is invoked.</param>
        public static void RegisterOnAssetsLoaded(PluginInfo info, IEnumerator enumerator, LoadingEventOrder order)
        {
            LoadingEvent evnt = new LoadingEvent()
            {
                info = info,
                loadingNumerator = enumerator,
            };
            switch (order)
            {
                case LoadingEventOrder.Start:
                    LoadingEventsStart.Add(evnt);
                    break;
                case LoadingEventOrder.Pre:
                    LoadingEventsPre.Add(evnt);
                    break;
                case LoadingEventOrder.Post:
                    LoadingEventsPost.Add(evnt);
                    break;
                case LoadingEventOrder.Final:
                    LoadingEventsFinal.Add(evnt);
                    break;
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
        /// <param name="order">The event in which assets will be loaded. Refer to LoadingEventOrder for more information on when each enum is invoked.</param>
        public static void RegisterOnAssetsLoaded(PluginInfo info, Action toRegister, LoadingEventOrder order)
        {
            RegisterOnAssetsLoaded(info, DummyIEnumerator(toRegister), order);
        }
    }
}
