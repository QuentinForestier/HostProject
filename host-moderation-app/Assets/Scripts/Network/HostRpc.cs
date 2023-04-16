using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Host.Network
{
    [Serializable]
    public class HostRpc
    {
        public int ObjectId;
        public int RpcId;
        public object[] Payload;

        public void ConvertInt64ToInt32Payload()
        {
            if (Payload != null && Payload.Length != 0)
            {
                object[] convertedPayload = new object[Payload.Length];

                for (int i = 0; i < Payload.Length; i++)
                {
                    if (Payload[i].GetType() == typeof(long))
                    {
                        convertedPayload[i] = Convert.ToInt32(Payload[i]);
                    }
                    else
                    {
                        convertedPayload[i] = Payload[i];
                    }
                }

                Payload = convertedPayload;
            }
        }
    }
}
