using System;
using System.Collections.Generic;

namespace Host
{
    /// <summary>
    /// Interface for all the events
    /// </summary>
    public interface IEvent
    {
        string GetScenario();
        byte GetEventCode();
        string ToString();
        string GetRecipient();
    }
}

