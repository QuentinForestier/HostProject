using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class used to represent a device as a GameObject and store informations
    /// </summary>
    public class Device : MonoBehaviour
    {
        /// <summary>
        /// Hostname of the device
        /// </summary>
        public string hostname;

        /// <summary>
        /// IP address of the device
        /// </summary>
        public string ip;
    }
}

