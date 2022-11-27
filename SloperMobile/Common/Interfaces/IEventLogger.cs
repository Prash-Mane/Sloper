using System;
using System.Collections.Generic;

namespace SloperMobile
{
    public interface IEventLogger
    {
        void LogEvent(string name);
        void LogEvent(string name, double amount);
        void LogEvent(string name, Dictionary<string, string> parameters);
    }
}
