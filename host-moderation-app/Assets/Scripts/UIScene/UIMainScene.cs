using Host.AppSettings;
using Host.DB;
using Host.Toolbox;
using Michsky.UI.ModernUIPack;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Host.UI
{
    using FfmpegUnity;
    using Host.Network;

    /// <summary>
    /// Class holding the logic for the Main scene
    /// </summary>
    public class UIMainScene : MonoBehaviour
    {
        [Header("Messages")]
        public GameObject btnMsgPrefab;
        public GameObject msgListContainer;

        public Button btnCustomMsg;
        public TMP_InputField inputCustomMessage;

        [Header("Virtual events")]
        public GameObject btnVirtualEventPrefab;
        public GameObject virtualEventListContainer;

        [Header("Help events")]
        public GameObject btnHelpEventPrefab;
        public GameObject helpEventContainer;

        [Header("Comments")]
        public Button btnAddComment;
        public TMP_InputField inputComment;

        [Header("Notifications")]
        public NotificationManager notification;

        [Header("Simulation control")]
        public GameObject stopSimulationBtn;
        public ModalWindowManager stopSimulationModal;
        public Button stopSimulationOKBtn;
        public Text textTimeElapsed;

        [Header("Videos")]
        public GameObject videoPrefab;
        public GameObject videoListContainer;
        public GameObject mainVideo;
        public WebcamSelector WebcamSelector;
        public VideoManager VideoManager;

        private FMNetworkManager FMNetworkManager;

        // Network
        private HelpRPC networkManager;

        // Scenario
        private ScenarioManager scenarioManager;

        // Simulation
        private SimulationManager simulationManager;

        public SceneLoader sceneLoader;

        private DBManager dbManager;
        private Settings currentSettings;

        // Tools
        private Tools tools;

        private string currentRecipient = "All";

        // Start is called before the first frame update
        void Start()
        {
            FMNetworkManager = GlobalElements.Instance.FMNetworkManager;
            networkManager = GlobalElements.Instance.HelpRPC;
            networkManager.uiMainScene = this;
            scenarioManager = GlobalElements.Instance.ScenarioManager;
            simulationManager = GlobalElements.Instance.SimulationManager;
            tools = GlobalElements.Instance.Tools;
            dbManager = GlobalElements.Instance.DBManager;

            currentSettings = dbManager.GetSettings();

            // Create the video streams for each hololens
            int baseID = 2000;
            var players = HostNetworkManager.HostNetwork.Clients;

            foreach(var player in players)
            {
                baseID++;
                Participant p = simulationManager.currentSimulation.FindParticipant((parti) => parti.GetIp() == player.IP);

                if(p != null)
                {
                    GameObject video = Instantiate(videoPrefab);
                    video.GetComponentInChildren<Text>().text = p.GetName();
                    video.transform.SetParent(videoListContainer.transform, false);
                    video.GetComponent<Button>().onClick.AddListener(() => SwitchMainVideo(video));
                    var gameDecoder = video.GetComponent<GameViewDecoder>();
                    gameDecoder.label = baseID;

                    FMNetworkManager.OnReceivedByteDataEvent.AddListener(gameDecoder.Action_ProcessImageData);

                    // Send start stream command
                    HostNetworkManager.HostNetwork.RPC(networkManager.HostNetworkId, "StartStreaming", player.IP, new object[] { baseID });
                }
                else
                {
                    Debug.LogError("[UIMainScene] - Couldn't find participant with device ip " + player.IP);
                }
            }

            var webcamStream = mainVideo.transform.GetChild(0).gameObject;

            // Add switch for main webcam too
            webcamStream.GetComponent<Button>().onClick.AddListener(() => SwitchMainVideo(webcamStream));

            // Add predefined messages on GUI
            /*scenarioManager.currentScenario.messageEvents.ForEach(m =>
            {
                GameObject btn = tools.AddButtonToContainer(btnMsgPrefab, m.content, msgListContainer);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    m.SetRecipient(currentRecipient);
                    if (networkManager.SendEvent(m))
                    {
                        tools.ShowNotification(notification, "Success", "Message sent !");
                    }
                });
            });*/

            // Add virtual event on GUI
            scenarioManager.currentScenario.virtualEvents.ForEach(e =>
            {
                GameObject btn = tools.AddButtonToContainer(btnVirtualEventPrefab, e.name, virtualEventListContainer);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (networkManager.SendEvent(e))
                    {
                        tools.ShowNotification(notification, "Success", "Event triggered !");
                    }
                });
            });

            // Add help event on GUI
            /*scenarioManager.currentScenario.helpEvents.ForEach(h =>
            {
                GameObject btn = tools.AddButtonToContainer(btnHelpEventPrefab, h.name, helpEventContainer);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (networkManager.SendEvent(h))
                    {
                        tools.ShowNotification(notification, "Success", "Help triggered !");
                    }
                });
            });*/

            // Set handler for custom message button
            OnSendCustomMessageHandler(currentRecipient);

            // Set handler for comment button
            OnAddCommentHandler();

            // Set handler for stop simulation button
            OnStopSimulationHandler();

            // Start the video capture
            VideoManager.StartRecordingVideo(scenarioManager.currentScenario);
        }


        // Update is called once per frame
        void Update()
        {
            if(simulationManager != null)
            {
                // Update the time elapsed on the GUI
                textTimeElapsed.text = simulationManager.currentSimulation.GetTimeElapsed().ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Callback when the app stops
        /// </summary>
        void OnApplicationQuit()
        {
            networkManager.StopRemoteSimulation();
        }

        /// <summary>
        /// Switch the main video with an other one
        /// </summary>
        /// <param name="video">The video to switch</param>
        public void SwitchMainVideo(GameObject video)
        {
            // Check if it's not already the main video
            if(video.transform.parent == mainVideo.transform)
            {
                return;
            }

            // Otherwise we need to swap
            var child = mainVideo.transform.GetChild(0);
            int siblingIndex = mainVideo.transform.GetSiblingIndex();

            child.transform.SetParent(video.transform.parent, false);
            video.transform.SetParent(mainVideo.transform, false);

            child.transform.SetSiblingIndex(siblingIndex);
        }

        /// <summary>
        /// Handler for the send custom message button
        /// </summary>
        /// <param name="recipientIP">Recipient's IP address</param>
        private void OnSendCustomMessageHandler(string recipientIP)
        {
            btnCustomMsg.onClick.RemoveAllListeners();
            btnCustomMsg.onClick.AddListener(() =>
            {
                string message = inputCustomMessage.text;

                if (message.Length < 1)
                {
                    tools.ShowNotification(notification, "Error", "Couldn't send message, input empty !");
                    return;
                }

                if (networkManager.SendEvent(new MessageEvent(scenario: scenarioManager.currentScenario.id, type: "Alert", content: message, recipient: recipientIP)))
                {
                    tools.ShowNotification(notification, "Success", "Message sent !");
                    inputCustomMessage.text = "";
                }
            });
        }

        /// <summary>
        /// Handler for the add comment button
        /// </summary>
        public void OnAddCommentHandler()
        {
            btnAddComment.onClick.RemoveAllListeners();
            btnAddComment.GetComponent<Button>().onClick.AddListener(() =>
            {
                string content = inputComment.text;
                if (content.Length < 1)
                {
                    tools.ShowNotification(notification, "Error", "Couldn't add comment, input empty !");
                    return;
                }
                double timeInSimulation = simulationManager.currentSimulation.GetTimeElapsed().TotalMilliseconds;
                Comment c = new Comment(content, timeInSimulation);

                // Get the current video image
                var image = mainVideo.GetComponentInChildren<RawImage>();

                if(image != null && image.texture != null)
                {
                    if(image.texture is Texture2D)
                    {
                        Texture2D tex = image.texture as Texture2D;
                        c.SetThumbnail(tex);
                    }
                    // Webcam texture needs to be converted to encode as png in the database
                    else if(image.texture is WebCamTexture)
                    {
                        WebCamTexture tex = image.texture as WebCamTexture;
                        Texture2D texture = new Texture2D(tex.width, tex.height);
                        texture.SetPixels(tex.GetPixels());
                        texture.Apply();
                        c.SetThumbnail(texture);
                    }
                }

                simulationManager.currentSimulation.AddComment(c);
                inputComment.text = "";

                VideoManager.AddCommentOnVideo(c);

                Debug.Log($"content : {c.GetContent()}, timeInSimulation : {c.GetTimeInSimulation()}");

                tools.ShowNotification(notification, "Success", "Comment added !");
            });
        }

        /// <summary>
        /// Handler for the stop simulation button
        /// </summary>
        public void OnStopSimulationHandler()
        {
            stopSimulationBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                stopSimulationOKBtn.onClick.RemoveAllListeners();
                stopSimulationOKBtn.onClick.AddListener(() =>
                {
                    networkManager.StopRemoteSimulation();
                    simulationManager.simulationReviewed = simulationManager.currentSimulation;

                    VideoManager.StopRecordingVideo();

                    WebcamSelector.Action_StopWebcam();

                    sceneLoader.LoadScene("ReviewSimulationScene");
                });
                stopSimulationModal.OpenWindow();
                simulationManager.currentSimulation.StopTimer();
            });
        }

        public void ShowNotification(string title, string content)
        {
            tools.ShowNotification(notification, title, content);
        }
    }
}

