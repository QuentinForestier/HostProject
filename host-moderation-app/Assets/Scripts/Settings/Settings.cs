using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Host.AppSettings
{
    /// <summary>
    /// Class representing the App settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Duration of the video cuts for the debriefing
        /// </summary>
        public string cutDuration { get; private set; }

        public Settings(string cutDuration)
        {
            this.cutDuration = cutDuration;
        }
    }
}
