using Host;
using Host.Network;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Host.Network.HostNetworkManager;

namespace HostProject.Network
{
    public class HelpRPC : MonoBehaviour
    {
        class HideContentObject
        {
            public GameObject GameObject;
            public Vector3 WantedPosition;
            public float LastUpdateTime;
        }

        private static bool _rpcInitDone = false;

        public HintsTVController HintsTV;

        public RoomRPC RoomRPC;

        [Title("Hidden content")]

        public Transform AnchorOrigin;
        public GameObject HideContentPrefab;
        public Transform LocalTrackedObject;
        private Dictionary<int, HideContentObject> _hideContentGameObjects = new Dictionary<int, HideContentObject>();
        private float _maxTimeBeforeDeletion = 2f;
        private bool _isSendingLocalHideContentObject = false;

        [Title("Audio")]

        public AudioSource AudioSource;
        public AudioClip AnnoucementBeep;
        public AudioClip PlaneBreakdown;
        public AudioClip CommandantAnnoucement;

        [Title("Arrows")]

        public GameObject LightArrow;
        public GameObject MeteoScreenArrow;
        public GameObject SeatsArrow;
        public GameObject PannelArrow;
        public GameObject BatteryArrow;

        [Title("Image")]

        public Sprite BoardingPass;
        public Sprite DecodePass;
        public Sprite Boussole;
        public Sprite DecodeSeats;
        public Sprite SeatsLetters;

        [Title("Switches")]

        public List<SwitchController> SwitchControllers;
        public SwitchBoxController SwitchBoxController;

        [Title("Network")]

        public FMNetworkManager FMNetworkManager;
        public GameViewEncoder StreamEncoder;

        public int HostNetworkId = 1;
        public int Label = 5001;
        public int PlayerId = 0;

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

        public void Awake()
        {
            if (!_rpcInitDone)
            {
                InitRpc();
            }
        }

        public void Start()
        {
            // Disabled in start to let them perform awake
            StreamEncoder.gameObject.SetActive(false);
        }

        public void Update()
        {
            // Sync the local object
            if (_isSendingLocalHideContentObject)
            {
                SendLocalTrackedObjectPosition();
            }

            // Update the position of the tracked objects
            foreach (var valuePair in _hideContentGameObjects.ToList())
            {
                HideContentObject hideContentObject = valuePair.Value;
                hideContentObject.GameObject.transform.localPosition = Vector3.Slerp(hideContentObject.GameObject.transform.localPosition, hideContentObject.WantedPosition, Time.deltaTime * 20f);

                // Check if the position has not be updated in a while, delete the element from the scene
                if (hideContentObject.LastUpdateTime + _maxTimeBeforeDeletion < Time.realtimeSinceStartup)
                {
                    Destroy(hideContentObject.GameObject);
                    _hideContentGameObjects.Remove(valuePair.Key);
                }
            }
        }

        public void OnReceivedByteDataEvent(byte[] data)
        {
            int label = BitConverter.ToInt32(data, 0);
            if (label != Label) return;

            int playerId = BitConverter.ToInt32(data, 4);

            float x = BitConverter.ToSingle(data, 8);
            float y = BitConverter.ToSingle(data, 12);
            float z = BitConverter.ToSingle(data, 16);

            // Updates the position or instantiate in the list of tracked objects
            if (_hideContentGameObjects.ContainsKey(playerId))
            {
                _hideContentGameObjects[playerId].WantedPosition = new Vector3(x, y, z);
                _hideContentGameObjects[playerId].LastUpdateTime = Time.realtimeSinceStartup;
            }
            else
            {
                var hideContentObject = new HideContentObject();

                hideContentObject.GameObject = Instantiate(HideContentPrefab, AnchorOrigin);
                hideContentObject.GameObject.transform.localPosition = new Vector3(x, y, z);
                hideContentObject.WantedPosition = new Vector3(x, y, z);
                hideContentObject.LastUpdateTime = Time.realtimeSinceStartup;

                _hideContentGameObjects.Add(playerId, hideContentObject);
            }
        }

        public void SendLocalTrackedObjectPosition()
        {
            byte[] sendBuffer = new byte[20];

            byte[] label = BitConverter.GetBytes(Label);

            // Get the position from the frame of reference of the anchor
            Vector3 anchoredPosition = AnchorOrigin.InverseTransformPoint(LocalTrackedObject.position);

            byte[] playerId = BitConverter.GetBytes(PlayerId);

            byte[] x = BitConverter.GetBytes(anchoredPosition.x);
            byte[] y = BitConverter.GetBytes(anchoredPosition.y);
            byte[] z = BitConverter.GetBytes(anchoredPosition.z);

            Buffer.BlockCopy(label, 0, sendBuffer, 0, 4);
            Buffer.BlockCopy(playerId, 0, sendBuffer, 4, 4);
            Buffer.BlockCopy(x, 0, sendBuffer, 8, 4);
            Buffer.BlockCopy(y, 0, sendBuffer, 12, 4);
            Buffer.BlockCopy(z, 0, sendBuffer, 16, 4);

            FMNetworkManager.SendToOthers(sendBuffer);
        }

        public void StartStreaming(int streamID)
        {
            StreamEncoder.gameObject.SetActive(true);
            StreamEncoder.label = streamID;
            PlayerId = streamID;

            _isSendingLocalHideContentObject = true;
        }

        public void StopSimulation()
        {
            Debug.Log("[RoomRPC] - StopSimulation");

            _isSendingLocalHideContentObject = false;

            // Stop sending images
            StreamEncoder.gameObject.SetActive(false);

            // Show end screen
            RoomRPC.ShowEndScreen();
        }

        public void TriggerMessageEvent(int scenario, string type, string content)
        {
            Debug.Log($"[HelpRPC] - Message Event Received: {scenario}, {type}, {content}");

            switch (type)
            {
                case "Alert":
                    HintsTV.ShowMessage(HintsTVController.IconType.Warning, content);
                    break;
                default:
                    break;
            }
        }

        public void TriggerVirtualEvent(int scenario, string actionNumber)
        {
            Debug.Log($"[HelpRPC] - Virtual Event Received: {scenario}, {actionNumber}");

            switch (actionNumber)
            {
                case "1":
                    break;
                case "2":
                    AudioSource.clip = CommandantAnnoucement;
                    AudioSource.Play();
                    break;
                case "3":
                    AudioSource.clip = PlaneBreakdown;
                    AudioSource.Play();
                    break;
                case "4":
                    AudioSource.clip = AnnoucementBeep;
                    AudioSource.Play();
                    break;
                default:
                    break;
            }
        }

        public void TriggerHelpEvent(int scenario, string actionNumber)
        {
            Debug.Log($"Action index: {actionNumber}");
            switch (actionNumber)
            {
                case "1":
                    break;
                case "51": // image boarding pass
                    HintsTV.ShowImage(BoardingPass);
                    break;
                case "52": // arrow lights
                    LightArrow.SetActive(true);
                    LightArrow.GetComponent<AudioSource>().Play();
                    _currentArrow = LightArrow;
                    CancelInvoke("HideArrow");
                    Invoke("HideArrow", 10f);
                    break;
                case "53": // image decode lights
                    HintsTV.ShowImage(DecodePass);
                    break;
                case "54": // image boussole
                    HintsTV.ShowImage(Boussole);
                    break;
                case "55": // arrow meteo screen
                    MeteoScreenArrow.SetActive(true);
                    MeteoScreenArrow.GetComponent<AudioSource>().Play();
                    _currentArrow = MeteoScreenArrow;
                    CancelInvoke("HideArrow");
                    Invoke("HideArrow", 10f);
                    break;
                case "56": // arrow seats
                    SeatsArrow.SetActive(true);
                    SeatsArrow.GetComponent<AudioSource>().Play();
                    _currentArrow = SeatsArrow;
                    CancelInvoke("HideArrow");
                    Invoke("HideArrow", 10f);
                    break;
                case "57": // example decode seats
                    HintsTV.ShowImage(DecodeSeats);
                    break;
                case "58": // arrow pannel
                    PannelArrow.SetActive(true);
                    PannelArrow.GetComponent<AudioSource>().Play();
                    _currentArrow = PannelArrow;
                    CancelInvoke("HideArrow");
                    Invoke("HideArrow", 10f);
                    break;
                case "59": // arrow battery
                    BatteryArrow.SetActive(true);
                    BatteryArrow.GetComponent<AudioSource>().Play();
                    _currentArrow = BatteryArrow;
                    CancelInvoke("HideArrow");
                    Invoke("HideArrow", 10f);
                    break;
                case "60": // image seats numbers
                    HintsTV.ShowImage(SeatsLetters);
                    break;
                default:
                    break;
            }
        }

        public void TriggerSetSwitchState(int switchIndex, bool state)
        {
            HostNetwork.RPC(HostNetworkId, "SetSwitchState", HostNetworkTarget.All, switchIndex, state);
        }

        public void SetSwitchState(int switchIndex, bool state)
        {
            SwitchControllers[switchIndex].SetState(state);
            SwitchBoxController.CheckPassword();
            Debug.Log("Switch State received");
        }

        public void BreakerPanelOpen(bool state)
        {
            SwitchBoxController.OpenDoor();
            Debug.Log("Breaker panel open triggered");
        }

        public void TriggerBreakerPanelOpen(bool state)
        {
            HostNetwork.RPC(HostNetworkId, "BreakerPanelOpen", HostNetworkTarget.All, state);
        }

        public void SeatsFreeNotification()
        {
            // not used here
        }

        public void TriggerSeatsFreeNotification()
        {
            HostNetwork.RPC(HostNetworkId, "SeatsFreeNotification", HostNetworkTarget.Server);
        }


        private GameObject _currentArrow;

        public void HideArrow()
        {
            _currentArrow.SetActive(false);
        }

        public void TriggerMonitoringButton(int id)
        {
            HostNetwork.RPC(HostNetworkId, "MonitoringButton", HostNetworkTarget.Server, id);
        }

        public void MonitoringButton(int id)
        {
            Debug.Log(id);
        }
        public void CryptedMessage(string message, string pairs)
        {
            Debug.Log(message + " / " + pairs);
        }
    }
}