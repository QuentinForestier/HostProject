using Host.Network;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Host.Toolbox;
using static Host.Network.HostNetworkManager;
using System;
using System.Linq;

namespace Host.UI
{
    /// <summary>
    /// Class holding the logic for the PickMasterDevice scene
    /// </summary>
    public class UIPickMasterDeviceScene : MonoBehaviour
    {
        private static bool _rpcInitDone = false;

        [Header("Devices")]
        public GameObject btnDevicePrefab;
        public GameObject btnDeviceContainer;

        [Header("Scene management")]
        public string nextScene;
        public SceneLoader sceneLoader;

        [Header("Instructions")]
        public Text instruText;
        public GameObject btnReady;
        public GameObject btnStart;

        [Header("Notifications")]
        public NotificationManager notification;

        public GameObject loadingBar;

        private Tools tools;
        private HelpRPC networkManager;

        public int HostNetworkId = 2;

        private void InitRpc()
        {
            HostNetworkManager.RegisterGameObject(HostNetworkId, this);
            HostNetworkManager.RegisterRPC(HostNetworkId, 0, "HololensConnected");
            HostNetworkManager.RegisterRPC(HostNetworkId, 1, "DisplayHostname");
            HostNetworkManager.RegisterRPC(HostNetworkId, 2, "StartPlacement");
            HostNetworkManager.RegisterRPC(HostNetworkId, 3, "AnchorPlaced");
            HostNetworkManager.RegisterRPC(HostNetworkId, 4, "RoomReady");
            HostNetworkManager.RegisterRPC(HostNetworkId, 5, "StartSimulation");
            _rpcInitDone = true;
        }

        public void Awake()
        {
            if (!_rpcInitDone)
            {
                InitRpc();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            tools = GlobalElements.Instance.Tools;
            networkManager = GlobalElements.Instance.HelpRPC;

            // Add its self to the network manager so it can be notified
            networkManager.uiPickMasterDeviceScene = this;

            btnStart.GetComponent<Button>().onClick.AddListener(() => StartSimulationButtonClick());
            btnStart.SetActive(false);

            btnReady.GetComponent<Button>().onClick.AddListener(() => OnReadyForPlacementHandler());
            if (HostNetwork.NumberOfClients == 0)
            {
                btnReady.SetActive(false);
            }

            // Display the connected devices on the GUI
            Array.ForEach(HostNetwork.Clients, p => CreateDeviceButton(p.IP));

            Debug.Log("RoomReady");
            HostNetwork.RPC(HostNetworkId, "RoomReady", HostNetworkTarget.Others);
            HostNetwork.ClientsListChanged.AddListener(NumberOfClientsChanged);
        }

        /// <summary>
        /// Handler that trigger the placement of the scene on the HoloLenses
        /// </summary>
        private void OnReadyForPlacementHandler()
        {
            btnReady.SetActive(false);
            Debug.Log("[UIPickMasterDeviceScene] - Scene placement requested to Hololens");
            HostNetwork.RPC(HostNetworkId, "StartPlacement", HostNetworkTarget.Others);
            instruText.text = "Please place the scene on the HoloLens";

            for (int i = 0; i < btnDeviceContainer.transform.childCount; i++)
            {
                btnDeviceContainer.transform.GetChild(i).Find("Ready").gameObject.SetActive(false);
                btnDeviceContainer.transform.GetChild(i).Find("Loop").gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Called when clicking on the button on the UI to start the simulation (when all hololens are ready)
        /// </summary>
        private void StartSimulationButtonClick()
        {
            sceneLoader.LoadScene(nextScene);
            HostNetwork.RPC(HostNetworkId, "StartSimulation", HostNetworkTarget.Others);
        }

        /// <summary>
        /// Creates a button on the GUI with the devices informations
        /// </summary>
        /// <param name="p">Player object associated to the device</param>
        private void CreateDeviceButton(string playerIp)
        {
            string btnName = "Device Name | " + playerIp;
            GameObject btn = tools.AddButtonToContainer(btnDevicePrefab, btnName, btnDeviceContainer);
            Device _d = btn.AddComponent<Device>();
            _d.hostname = "Device Name";
            _d.ip = playerIp;

            btn.transform.Find("Loop").gameObject.SetActive(true);
        }


        #region Photon RPCs

        /// <summary>
        /// Called when an HoloLens is connected
        /// </summary>
        /// <param name="info">Info about the device</param>
        public void HololensConnected(string sender)
        {
            Debug.Log("Player has connected with IP : " + sender);

            for (int i = 0; i < btnDeviceContainer.transform.childCount; i++)
            {
                string ip = btnDeviceContainer.transform.GetChild(i).GetComponentInChildren<Device>().ip;

                if (ip == sender)
                {
                    btnDeviceContainer.transform.GetChild(i).Find("Loop").gameObject.SetActive(false);
                    btnDeviceContainer.transform.GetChild(i).Find("Ready").gameObject.SetActive(true);
                }
            }
            HostNetwork.RPC(HostNetworkId, "DisplayHostname", sender);
        }

        /// <summary>
        /// Display the hostname on the HoloLens
        /// </summary>
        public void DisplayHostname()
        {
            Debug.Log("[UIPickMasterDeviceScene] - Display hostname requested");
        }

        /// <summary>
        /// Called by the Host when a Hololens needs to place the geometry
        /// </summary>
        public void StartPlacement()
        {
            Debug.Log("[UIPickMasterDeviceScene] - Placement has been requested");
        }

        /// <summary>
        /// Called by the HoloLens when the scene has been placed
        /// </summary>
        /// <param name="info">Info about the HoloLens which placed the scene</param>
        public void AnchorPlaced(string sender)
        {
            for (int i = 0; i < btnDeviceContainer.transform.childCount; i++)
            {
                string ip = btnDeviceContainer.transform.GetChild(i).GetComponentInChildren<Device>().ip;

                if (ip == sender)
                {
                    btnDeviceContainer.transform.GetChild(i).Find("Loop").gameObject.SetActive(false);
                    btnDeviceContainer.transform.GetChild(i).Find("Ready").gameObject.SetActive(true);
                }
            }

            btnStart.SetActive(true);
        }

        /// <summary>
        /// Triggered when the HoloLenses are ready
        /// </summary>
        public void RoomReady()
        {
            Debug.Log("[UIPickMasterDeviceScene] - Room Ready");
            instruText.text = "Everything is ready";
        }

        /// <summary>
        /// Triggered on the HoloLenses to start the simulation
        /// </summary>
        public void StartSimulation() { }

        #endregion

        #region NetworkManager callbacks

        private void NumberOfClientsChanged(HostTcpClient[] clients)
        {
            Debug.Log("[UIPickMasterDeviceScene] - Number of clients changed");

            // Destroy inexisting clients
            for (int i = 0; i < btnDeviceContainer.transform.childCount; i++)
            {
                string ip = btnDeviceContainer.transform.GetChild(i).GetComponentInChildren<Device>().ip;

                if (!clients.Any(client => client.IP.Equals(ip)))
                {
                    Destroy(btnDeviceContainer.transform.GetChild(i).gameObject);
                }
            }

            // Create new clients
            foreach (var client in clients)
            {
                bool clientFound = false;

                for (int i = 0; i < btnDeviceContainer.transform.childCount; i++)
                {
                    string ip = btnDeviceContainer.transform.GetChild(i).GetComponentInChildren<Device>().ip;
                    if (client.IP.Equals(ip))
                    {
                        clientFound = true;
                        break;
                    }
                }

                // No button for this client, create one
                if(!clientFound)
                {
                    CreateDeviceButton(client.IP);
                    btnReady.SetActive(true);

                    // Send notification to hololens to say that the we are ready to go
                    HostNetwork.RPC(HostNetworkId, "RoomReady", client.IP);
                }
            }

            if (HostNetwork.NumberOfClients == 0)
            {
                instruText.text = "Turn on all the HoloLens and scan the room, they will appear above once ready !";
                btnReady.SetActive(false);
            }
        }

        #endregion

    }

}
