using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Host.AppSettings;
using Host.DB;
using Michsky.UI.ModernUIPack;
using Host.Toolbox;

namespace Host.UI
{
    using Host.Network;

    /// <summary>
    /// Class holding the logic for the PreviousSimulation scene
    /// </summary>
    public class UIPreviousSimulationScene : MonoBehaviour
    {
        [Header("Image comments")]
        public GameObject imagePrefab;
        public GameObject imageContainer;

        [Header("Participants")]
        public GameObject btnParticipantPrefab;
        public GameObject btnParticipantContainer;

        [Header("Stats")]
        public Text textDuration;
        public Text textTotalComment;

        [Header("Simulation")]
        public Button btnDeleteSimulation;
        public Button generateDebriefingBtn;
        public Button watchDebriefingBtn;

        [Header("Notification")]
        public NotificationManager notification;

        private SimulationManager simulationManager;
        public SceneLoader sceneLoader;
        private Tools tools;
        private DBManager dBManager;
        private HelpRPC networkManager;
        private Settings currentSettings;


        // Start is called before the first frame update
        void Start()
        {
            simulationManager = GlobalElements.Instance.SimulationManager;
            tools = GlobalElements.Instance.Tools;
            dBManager = GlobalElements.Instance.DBManager;
            networkManager = GlobalElements.Instance.HelpRPC;

            SceneLoader.previousScene = "PreviousSimulationScene";

            currentSettings = dBManager.GetSettings();
            
            // Display all the comments on the GUI
            simulationManager.simulationReviewed.listComments?.ForEach(c =>
            {
                GameObject img = Instantiate(imagePrefab) as GameObject;
                img.GetComponentInChildren<Text>().text = c.GetContent();
                img.transform.SetParent(imageContainer.transform, false);

                img.GetComponent<RawImage>().texture = c.GetThumbnailTexture();
            });

            // Display all the participants on the GUI
            simulationManager.simulationReviewed.GetParticipants()?.ForEach(p =>
            {
                tools.AddButtonToContainer(btnParticipantPrefab, p.GetName(), btnParticipantContainer);
            });

            btnDeleteSimulation.onClick.AddListener(() =>
            {
                OnDeleteSimulationHandler(simulationManager.simulationReviewed);
            });

            // Handler for the generate debriefing button
            generateDebriefingBtn.onClick.AddListener(() =>
            {
                if (simulationManager.simulationReviewed.GetNumberOfComment() > 0)
                {
                    sceneLoader.LoadScene("GenerateDebriefingScene");
                }
                else
                {
                    tools.ShowNotification(notification, "Error", "This simulation doesn't contain any comments");
                }
            });

            // Add stats about the simulation on the GUI
            UpdateStats();

            // Handler for the watch debriefing button
            watchDebriefingBtn.onClick.AddListener(() =>
            {
                if (simulationManager.simulationReviewed.GetNumberOfComment() > 0)
                {
                    sceneLoader.LoadScene("WatchDebriefingScene");
                }
                else
                {
                    tools.ShowNotification(notification, "Error", "This simulation doesn't contain any comments");
                }
            });


        }

        /// <summary>
        /// Update the stats fields with informations about the simulation reviewed
        /// </summary>
        private void UpdateStats()
        {
            textDuration.text = "Duration : " + simulationManager.simulationReviewed.duration.ToString(@"hh\:mm\:ss");
            textTotalComment.text = "Total comments : " + simulationManager.simulationReviewed.GetNumberOfComment().ToString();
        }

        /// <summary>
        /// Handler for the deletion of a simulation
        /// </summary>
        /// <param name="s">Simulation to delete</param>
        private void OnDeleteSimulationHandler(Simulation s)
        {
            dBManager.DeleteSimulationByID(s.id);

            sceneLoader.LoadScene("PickPreviousSimulationScene");
        }
    }

}
