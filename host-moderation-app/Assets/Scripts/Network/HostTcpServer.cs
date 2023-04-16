using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Host.Network
{
	public class HostNetworkClientsChangedEvent : EventArgs
	{
		public HostNetworkClientsChangedEvent(HostTcpClient[] clients)
		{
			TcpClients = clients;
		}

		public HostTcpClient[] TcpClients { get; }
	}

	public class HostTcpServer
	{
		public int TcpPort { get; } = 28057;

		private TcpListener _tcpListener;

		private Thread _tcpListenerThread;

		private readonly List<HostTcpClient> _connectedTcpClients = new List<HostTcpClient>();

		public HostTcpClient[] Clients { get { return _connectedTcpClients.ToArray(); } }

		/// <summary>
		/// Triggers whenever a message is received destined to the server.
		/// Careful, remember that these messages are received from another thread and need to be synchronized to interface with Unity logic
		/// </summary>
		public EventHandler<HostNetworkMessageEvent> MessageReceived;

		public EventHandler<HostNetworkClientsChangedEvent> ClientsChanged;

		~HostTcpServer()
		{
			Destroy();
		}

		public void Destroy()
        {
			if (_tcpListener != null)
			{
				_tcpListener.Stop();
			}

			if (_tcpListenerThread != null)
			{
				_tcpListenerThread.Abort();
			}
		}

		public void StartServer()
		{
			// Start TcpServer background thread 		
			_tcpListenerThread = new Thread(new ThreadStart(ListenForIncomingRequests));
			_tcpListenerThread.IsBackground = true;
			_tcpListenerThread.Start();
		}

		/// <summary> 	
		/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
		/// </summary> 	
		private void ListenForIncomingRequests()
		{
			try
			{
				_tcpListener = new TcpListener(IPAddress.Any, TcpPort);
				_tcpListener.Start();
				Debug.Log("[HostTcpServer] Server is listening");
				byte[] bytes = new byte[1024];
				while (true)
				{
					var connectingClient = _tcpListener.AcceptTcpClient();
					
					HostTcpClient client = new HostTcpClient();
					client.ConnectToClient(connectingClient);
					_connectedTcpClients.Add(client);
					client.MessageReceived += ReceivedMessageFromClient;
					ClientsChanged?.Invoke(this, new HostNetworkClientsChangedEvent(_connectedTcpClients.ToArray()));
					client.StreamClosed += (sender, args) =>
					{
						_connectedTcpClients.Remove(client);
						ClientsChanged?.Invoke(this, new HostNetworkClientsChangedEvent(_connectedTcpClients.ToArray()));
						client.Destroy();
					};
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("[HostTcpServer] SocketException " + socketException.ToString());
			}

			Debug.Log("[HostTcpServer] Server has stopped listening");
		}

		private void ReceivedMessageFromClient(object sender, HostNetworkMessageEvent messageEvent)
		{
			switch (messageEvent.Message.NetworkTarget)
			{
				case HostNetworkTarget.All:
				// Same as others, client will have taken care of receiving the data locally
				case HostNetworkTarget.Others:
					foreach (var client in _connectedTcpClients)
					{
						if (!client.IP.Equals(messageEvent.Message.SourceIP))
						{
							client.SendData(messageEvent.Message);
						}
					}
					MessageReceived(this, messageEvent);
					break;
				case HostNetworkTarget.Server:
					MessageReceived(this, messageEvent);
					break;
				case HostNetworkTarget.Target:
					// Only send to specific target contained in the message event
					foreach (var client in _connectedTcpClients)
					{
						if (client.IP.Equals(messageEvent.Message.TargetIP))
						{
							client.SendData(messageEvent.Message);
						}
					}
					break;
			}
		}

		public void SendToAll(byte[] data)
		{
			HostNetworkMessage message = new HostNetworkMessage
			{
				MessageType = HostNetworkMessageType.RPC,
				Data = data,
				NetworkTarget = HostNetworkTarget.All,
				SourceIP = "0.0.0.0"
			};

			foreach (var client in _connectedTcpClients)
			{
				client.SendData(message);
			}

			MessageReceived(this, new HostNetworkMessageEvent(message));
		}

		public void SendToServer(byte[] data)
		{
			// local event only
			HostNetworkMessage message = new HostNetworkMessage
			{
				MessageType = HostNetworkMessageType.RPC,
				Data = data,
				NetworkTarget = HostNetworkTarget.Server,
				SourceIP = "0.0.0.0"
			};

			MessageReceived(this, new HostNetworkMessageEvent(message));
		}

		public void SendToOthers(byte[] data)
		{
			HostNetworkMessage message = new HostNetworkMessage
			{
				MessageType = HostNetworkMessageType.RPC,
				Data = data,
				NetworkTarget = HostNetworkTarget.Others,
				SourceIP = "0.0.0.0"
			};

			foreach (var client in _connectedTcpClients)
			{
				client.SendData(message);
			}
		}

		public void SendToTarget(byte[] data, string target)
		{
			HostNetworkMessage message = new HostNetworkMessage
			{
				MessageType = HostNetworkMessageType.RPC,
				Data = data,
				NetworkTarget = HostNetworkTarget.Target,
				SourceIP = "0.0.0.0",
				TargetIP = target
			};

			foreach (var client in _connectedTcpClients)
			{
				if (client.IP.Equals(target))
				{
					client.SendData(message);
				}
			}
		}
	}
}