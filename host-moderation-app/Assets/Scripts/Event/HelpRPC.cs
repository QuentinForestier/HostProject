using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Michsky.UI.ModernUIPack;
using System;
using Host.UI;
using Host.Toolbox;
using static Host.Network.HostNetworkManager;


namespace Host.Network
{
    /// <summary>
    /// Establish the connection with the Photon PUN2 server and trigger remote events
    /// </summary>
    public class HelpRPC : MonoBehaviour
    {
        private static bool _rpcInitDone = false;

        // Subscribers to notification events
        [HideInInspector]
        public UIMainScene uiMainScene;

        [HideInInspector]
        public UIPickMasterDeviceScene uiPickMasterDeviceScene;

        public int HostNetworkId = 1;

        private void InitRpc()
        {
            HostNetworkManager.RegisterGameObject(HostNetworkId, this);
            HostNetworkManager.RegisterRPC(HostNetworkId, 0, "TriggerMessageEvent");
            HostNetworkManager.RegisterRPC(HostNetworkId, 1, "TriggerVirtualEvent");
            HostNetworkManager.RegisterRPC(HostNetworkId, 2, "TriggerHelpEvent");
            HostNetworkManager.RegisterRPC(HostNetworkId, 3, "StopSimulation");
            HostNetworkManager.RegisterRPC(HostNetworkId, 4, "StartStreaming");
            HostNetworkManager.RegisterRPC(HostNetworkId, 5, "SeatsFreeNotification");
            HostNetworkManager.RegisterRPC(HostNetworkId, 6, "SetSwitchState");
            HostNetworkManager.RegisterRPC(HostNetworkId, 7, "BreakerPanelOpen");
            HostNetworkManager.RegisterRPC(HostNetworkId, 8, "CryptedMessage");
            HostNetworkManager.RegisterRPC(HostNetworkId, 9, "MonitoringButton");
            _rpcInitDone = true;
        }

        private void Awake()
        {
            if (!_rpcInitDone)
            {
                InitRpc();
            }

            // Make sure that the the current gameObject won't be destroyed
            DontDestroyOnLoad(this);
        }

        // On enter room -> Trigger Hololens Button
        // uiPickMasterDeviceScene.OnPlayerEnteredRoom(pl);
        // On left room
        // uiPickMasterDeviceScene.OnPlayerLeftRoom(pl);

        // Get connected players
        // Get other players
        // GetNumberOfOtherDevice


        /// <summary>
        /// Send virtual, help and message events to devices
        /// </summary>
        /// <param name="ev">The event to send</param>
        /// <returns>True if the event was triggered, else False</returns>
        public bool SendEvent(IEvent ev)
        {
            if (ev.GetType() == typeof(MessageEvent))
            {
                MessageEvent m = (MessageEvent)ev;

                if (ev.GetRecipient() == "All")
                {
                    HostNetwork.RPC(HostNetworkId, "TriggerMessageEvent", HostNetworkTarget.Others, m.scenario, m.type, m.content);
                    return true;
                }
                else
                {
                    HostNetwork.RPC(HostNetworkId, "TriggerMessageEvent", ev.GetRecipient(), m.scenario, m.type, m.content);
                    return true;
                }
            }
            else if (ev.GetType() == typeof(VirtualEvent))
            {
                VirtualEvent v = (VirtualEvent)ev;
                HostNetwork.RPC(HostNetworkId, "TriggerVirtualEvent", HostNetworkTarget.Others, v.scenario, v.action_number);
                return true;
            }
            else if (ev.GetType() == typeof(HelpEvent))
            {
                HelpEvent h = (HelpEvent)ev;
                HostNetwork.RPC(HostNetworkId, "TriggerHelpEvent", HostNetworkTarget.Others, h.scenario, h.action_number);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Send a RPC to stop the remote simulation
        /// </summary>
        public void StopRemoteSimulation()
        {
            HostNetwork.RPC(HostNetworkId, "StopSimulation", HostNetworkTarget.Others, null);
        }

        public void TriggerMessageEvent(int scenario, string type, string content)
        {
            Debug.Log("[NetworkManager] - Triggering message event");
        }

        public void TriggerVirtualEvent(int scenario, string actionNumber) { }

        public void TriggerHelpEvent(int scenario, string actionNumber) { }

        public void StopSimulation() { }


        public void SeatsFreeNotification()
        {
            uiMainScene.ShowNotification("Déplacer les sièges", "Les sièges peuvent être déplacés maintenant!");
        }

        public void SetSwitchState(int switchIndex, bool state)
        {
            // Not used here
        }

        public void BreakerPanelOpen(bool state)
        {
            // Not used here
            if(manager != null)
            {
                manager.CryptedMessage();
            }
        }

        

        public void TriggerCryptedMessage(string message, string pairs)
        {

            HostNetwork.RPC(HostNetworkId, "CryptedMessage", HostNetworkTarget.Others, message, pairs);
        }
        public void CryptedMessage(string message, string pairs)
        {
            // Not used here
        }

        private RiddleManager manager;

        public void TriggerMonitoringButton(RiddleManager manager, int id)
        {
            this.manager = manager;
            HostNetwork.RPC(HostNetworkId, "MonitoringButton", HostNetworkTarget.Others, id);
        }

        public void MonitoringButton(int id)
        {
            if(manager != null)
            {
                manager.ButtonClicked(id);
            }
        }
    }

}
