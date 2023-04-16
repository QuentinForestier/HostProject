using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Host.Network
{
    [Serializable]
    public class HostNetworkMessage
    {
        public HostNetworkMessageType MessageType;
        public HostNetworkTarget NetworkTarget;
        public string SourceIP = "";
        public string TargetIP = "";
        public byte[] Data = null;
    }

    public class HostNetworkMessageEvent : EventArgs
    {
        public HostNetworkMessageEvent(HostNetworkMessage message)
        {
            Message = message;
        }

        public HostNetworkMessage Message { get; set; }
    }
}
