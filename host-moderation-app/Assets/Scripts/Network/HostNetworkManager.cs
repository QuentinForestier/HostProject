using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Host.Network
{
    public class HostNetworkManager : MonoBehaviour
    {
        private class RegisteredObject
        {
            public MonoBehaviour TargetObject;
            public Dictionary<int, string> Rpc;
        }

        private static Dictionary<int, RegisteredObject> _registeredObjects = new Dictionary<int, RegisteredObject>();

        /// <summary>
        /// UDP data network manager
        /// </summary>
        private FMNetworkManager _fmNetworkManager;

        /// <summary>
        /// TCP data network manager
        /// </summary>
        private HostTcpServer _hostTcpServer;

        private HostTcpClient _hostTcpClient;

        private bool _serverFound = false;

        public bool IsServer = false;

        public bool IsClient { get { return !IsServer; } set { IsServer = !value; } }

        public static HostNetworkManager HostNetwork { get; private set; }

        public int NumberOfClients => _hostTcpServer.Clients.Count();

        public HostTcpClient[] Clients => _hostTcpServer.Clients;

        public string ClientLocalIP => _hostTcpClient.IP;

        /// <summary>
        /// Only available as a server
        /// Indicate when the number of clients has changed and return the current list
        /// </summary>
        public UnityEvent<HostTcpClient[]> ClientsListChanged;

        public void Awake()
        {
            HostNetwork = this;
            _fmNetworkManager = GetComponent<FMNetworkManager>();

            if (IsServer)
            {
                _hostTcpServer = new HostTcpServer();

                // Start to listen for incoming events
                _hostTcpServer.StartServer();
                _hostTcpServer.MessageReceived += MessageReceived;
                _hostTcpServer.ClientsChanged += ClientsChangedOnServer;
            }
            else
            {
                _hostTcpClient = new HostTcpClient();
                _hostTcpClient.MessageReceived += MessageReceived;
            }
        }

        private void ClientsChangedOnServer(object sender, HostNetworkClientsChangedEvent clientsChangedEvent)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ClientsListChanged.Invoke(clientsChangedEvent.TcpClients);
            });
        }

        public void Start()
        {
            if (!_serverFound && IsClient)
            {
                Invoke("CheckConnectedToUdpServer", 1f);
            }
        }

        public void OnDestroy()
        {
            if(_hostTcpServer != null)
            {
                _hostTcpServer.Destroy();
            }

            if(_hostTcpClient != null)
            {
                _hostTcpClient.Destroy();
            }
        }

        public void CheckConnectedToUdpServer()
        {
            // FMNetworkManager gives 0 feedback when it connects, so we need to poll it's status to know when the IP address is available
            if (!_serverFound && IsClient)
            {
                if (_fmNetworkManager.Client.IsConnected && !_fmNetworkManager.Client.ServerIP.Equals("0,0,0,0"))
                {
                    Debug.Log($"[HostNetworkManager] - Server found {_fmNetworkManager.Client.ServerIP}");
                    _serverFound = true;
                    _hostTcpClient.ConnectToIP(_fmNetworkManager.Client.ServerIP);
                }
                else
                {
                    Invoke("CheckConnectedToUdpServer", 1f);
                }
            }
        }

        public void ConnectClient(string ip)
        {
            _hostTcpClient.ConnectToIP(ip);
        }

        private void MessageReceived(object sender, HostNetworkMessageEvent messageEvent)
        {
            if (messageEvent.Message.MessageType == HostNetworkMessageType.RPC)
            {
                HostRpc message = JsonConvert.DeserializeObject<HostRpc>(Encoding.UTF8.GetString(messageEvent.Message.Data));
                message.ConvertInt64ToInt32Payload();

                MonoBehaviour targetObject = _registeredObjects[message.ObjectId].TargetObject;
                string methodname = _registeredObjects[message.ObjectId].Rpc[message.RpcId];

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log($"Received RPC: {message.ObjectId}, {methodname}, {message.Payload}");

                    // Call the method
                    var method = targetObject.GetType().GetMethod(methodname);
                    method.Invoke(targetObject, message.Payload);
                });
            }
        }

        public static void RegisterGameObject(int objectId, MonoBehaviour targetObject)
        {
            if (_registeredObjects.ContainsKey(objectId) || _registeredObjects.ContainsKey(objectId))
            {
                Debug.LogError($"Registered same object ID twice: {objectId}");
                return;
            }
            var objectToRegister = new RegisteredObject()
            {
                TargetObject = targetObject,
                Rpc = new Dictionary<int, string>()
            };

            _registeredObjects.Add(objectId, objectToRegister);
        }

        public static void RegisterRPC(int objectId, int rpcId, string method)
        {
            if (!_registeredObjects.ContainsKey(objectId))
            {
                Debug.LogError($"Unregistered game object ID: {objectId}");
            }

            var objectDic = _registeredObjects[objectId].Rpc;

            if (objectDic.ContainsKey(rpcId))
            {
                Debug.LogError($"Registered same RPC ID twice: {objectId}");
            }

            objectDic.Add(rpcId, method);
        }

        public void RPC(int objectId, string methodName, HostNetworkTarget target, params object[] parameters)
        {
            HostRpc rpc = new HostRpc();
            rpc.ObjectId = objectId;
            rpc.RpcId = _registeredObjects[objectId].Rpc.FirstOrDefault(x => x.Value.Equals(methodName)).Key;
            rpc.Payload = parameters;

            string message = JsonConvert.SerializeObject(rpc);

            Debug.Log($"RPC sent: {objectId}, {methodName}\n\n{message}\n");

            switch (target)
            {
                case HostNetworkTarget.Others:
                    if (IsServer)
                    {
                        _hostTcpServer.SendToOthers(Encoding.UTF8.GetBytes(message));
                    }
                    else
                    {
                        _hostTcpClient.SendToOthers(Encoding.UTF8.GetBytes(message));
                    }
                    break;
                case HostNetworkTarget.All:
                    if (IsServer)
                    {
                        _hostTcpServer.SendToAll(Encoding.UTF8.GetBytes(message));
                    }
                    else
                    {
                        _hostTcpClient.SendToOthers(Encoding.UTF8.GetBytes(message));
                    }
                    break;
                case HostNetworkTarget.Server:
                    if (IsServer)
                    {
                        _hostTcpServer.SendToServer(Encoding.UTF8.GetBytes(message));
                    }
                    else
                    {
                        _hostTcpClient.SendToOthers(Encoding.UTF8.GetBytes(message));
                    }
                    break;
            }
        }

        public void RPC(int objectId, string methodName, string ip, params object[] parameters)
        {
            HostRpc rpc = new HostRpc();
            rpc.ObjectId = objectId;
            rpc.RpcId = _registeredObjects[objectId].Rpc.FirstOrDefault(x => x.Value.Equals(methodName)).Key;
            rpc.Payload = parameters;

            string message = JsonConvert.SerializeObject(rpc);

            Debug.Log($"RPC sent: {objectId}, {methodName}\n\n{message}\n");

            if (IsServer)
            {
                _hostTcpServer.SendToTarget(Encoding.UTF8.GetBytes(message), ip);
            }
            else
            {
                _hostTcpClient.SendToTarget(Encoding.UTF8.GetBytes(message), ip);
            }
        }
    }

}