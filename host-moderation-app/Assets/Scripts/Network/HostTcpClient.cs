using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Host.Network
{
    public class HostTcpClient
    {
        private static int BUFFER_SIZE = 1024;

        public string IP { get; private set; }
        public int TcpPort { get; } = 28057;

        private TcpClient _socketConnection;
        private Thread _clientReceiveThread;

        public EventHandler<HostNetworkMessageEvent> MessageReceived;

        public EventHandler StreamClosed;

        private bool _shouldConnect = false;

        /// <summary>
        /// Setup the connection using the existing TcpClient
        /// </summary>
        /// <param name="client"></param>
        public void ConnectToClient(TcpClient client)
        {
            _socketConnection = client;
            IP = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString();

            StartListen();
        }

        ~HostTcpClient()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (_socketConnection != null)
            {
                if (_socketConnection.Connected)
                {
                    _socketConnection.Close();
                }
            }

            if (_clientReceiveThread != null)
            {
                _clientReceiveThread.Abort();
            }
        }

        /// <summary>
        /// Connect to the TCP socket usingn the specified IP address
        /// </summary>
        public void ConnectToIP(string ip)
        {
            IP = ip;
            _shouldConnect = true;

            StartListen();
        }

        /// <summary>
        /// Start the background thread to listen for incoming data
        /// </summary>
        private void StartListen()
        {
            try
            {
                _clientReceiveThread = new Thread(new ThreadStart(ListenForData));
                _clientReceiveThread.IsBackground = true;
                _clientReceiveThread.Start();
            }
            catch (Exception e)
            {
                Debug.Log("[HostTcpClient] On client connect exception " + e);
            }
        }

        /// <summary>
        /// Runs in background clientReceiveThread; Listens for incomming data. 	
        /// </summary>
        private void ListenForData()
        {
            try
            {
                if (_shouldConnect)
                {
                    _socketConnection = new TcpClient(IP, TcpPort);
                    _shouldConnect = false;
                }

                Debug.Log("[HostTcpClient] Started client for ip: " + IP);

                byte[] bufferData = new byte[BUFFER_SIZE];
                while (_socketConnection.Connected)
                {
                    // Get a stream object for reading 				
                    using (NetworkStream stream = _socketConnection.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. Read is blocking				
                        while ((length = stream.Read(bufferData, 0, bufferData.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bufferData, 0, incomingData, 0, length);

                            // Trigger event with incomming data
                            Debug.Log($"[HostTcpClient] Data received");

                            try
                            {
                                HostNetworkMessage message = JsonConvert.DeserializeObject<HostNetworkMessage>(Encoding.UTF8.GetString(incomingData));
                                MessageReceived?.Invoke(this, new HostNetworkMessageEvent(message));
                            }
                            catch(JsonReaderException)
                            {
                                Debug.Log($"Corrupted message received: {Encoding.UTF8.GetString(incomingData)}");
                                // Discard corrupted message, but keep going
                                break;
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("[HostTcpClient] Socket exception: " + socketException);
            }

            // Sometimes the object is destroyed and StreamClosed doesn't exist anymore
            // This can happen because we are in another thread
            if (StreamClosed != null)
            {
                StreamClosed.Invoke(this, null);
            }

            Debug.Log("[HostTcpClient] Stopped client for ip: " + IP);
        }

        public void SendToTarget(byte[] data, string target)
        {
            HostNetworkMessage message = new HostNetworkMessage
            {
                MessageType = HostNetworkMessageType.RPC,
                Data = data,
                NetworkTarget = HostNetworkTarget.Target,
                SourceIP = IP,
                TargetIP = target
            };

            SendData(message);
        }

        public void SendToAll(byte[] data)
        {
            HostNetworkMessage message = new HostNetworkMessage
            {
                MessageType = HostNetworkMessageType.RPC,
                Data = data,
                NetworkTarget = HostNetworkTarget.All,
                SourceIP = IP
            };

            SendData(message);

            // Invoke locally as if we received the message
            MessageReceived?.Invoke(this, new HostNetworkMessageEvent(message));
        }

        public void SendToServer(byte[] data)
        {
            HostNetworkMessage message = new HostNetworkMessage
            {
                MessageType = HostNetworkMessageType.RPC,
                Data = data,
                NetworkTarget = HostNetworkTarget.Server,
                SourceIP = IP
            };

            SendData(message);
        }

        public void SendToOthers(byte[] data)
        {
            HostNetworkMessage message = new HostNetworkMessage
            {
                MessageType = HostNetworkMessageType.RPC,
                Data = data,
                NetworkTarget = HostNetworkTarget.Others,
                SourceIP = IP
            };

            SendData(message);
        }

        /// <summary>
        /// Send message to server using socket connection. 	
        /// </summary>
        public void SendData(HostNetworkMessage message)
        {
            string strData = JsonConvert.SerializeObject(message);
            byte[] data = Encoding.UTF8.GetBytes(strData);

            if (_socketConnection == null)
            {
                Debug.Log("[HostTcpClient] Socket is null");
                return;
            }

            try
            {
                // Get a stream object for writing.
                NetworkStream stream = _socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("[HostTcpClient] Socket exception: " + socketException);
            }
            catch (InvalidOperationException invalidException)
            {

            }
        }
    }
}
