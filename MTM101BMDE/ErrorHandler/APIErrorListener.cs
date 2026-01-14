using BepInEx.Logging;
using System;

namespace MTM101BaldAPI.ErrorHandler
{
    public class APIErrorListener : ILogListener
    {
        public void Dispose()
        {

        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Level == LogLevel.Fatal || eventArgs.Level == LogLevel.Error)
            {
                //MTM101BaldiDevAPI.CauseCrash(MTM101BaldiDevAPI.Instance.Info, new Exception(eventArgs.Data.ToString()));
                ErrorDisplayer.allErrorDisplayers.ForEach(x => x.ShowError(eventArgs.Data.ToString(), 5f));
            }
        }
    }
}
