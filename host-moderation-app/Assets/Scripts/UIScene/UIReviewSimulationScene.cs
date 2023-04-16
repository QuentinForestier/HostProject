using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using Host.DB;
using Host.AppSettings;
using Host.Toolbox;

namespace Host.UI
{
    using Host.Network;

    /// <summary>
    /// Class holding the logic for the ReviewSimulation scene
    /// </summary>
    public class UIReviewSimulationScene : MonoBehaviour
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
        public Button saveSimulationBtn;

        [Header("Notification")]
        public NotificationManager notification;


        private SimulationManager simulationManager;
        public SceneLoader sceneLoader;
        private HelpRPC networkManager;
        private Tools tools;
        private DBManager dBManager;
        private Settings currentSettings;

        // Start is called before the first frame update
        void Start()
        {
            simulationManager = GlobalElements.Instance.SimulationManager;
            networkManager = GlobalElements.Instance.HelpRPC;
            tools = GlobalElements.Instance.Tools;
            dBManager = GlobalElements.Instance.DBManager;

            currentSettings = dBManager.GetSettings();

            SceneLoader.previousScene = "ReviewSimulationScene";

            // Add all the comment on the GUI
            simulationManager.simulationReviewed.listComments.ForEach(c =>
            {
                GameObject comment = Instantiate(imagePrefab) as GameObject;
                comment.GetComponentInChildren<TMP_InputField>().text = c.GetContent();
                comment.transform.SetParent(imageContainer.transform, false);

                CommentGO com = comment.GetComponent<CommentGO>();
                com.content = c.GetContent();

                comment.GetComponent<RawImage>().texture = c.GetThumbnailTexture();

                // Remove a comment on click
                comment.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    RemoveComment(c);
                });

            });

            // Add all participants on the GUI
            simulationManager.simulationReviewed.GetParticipants().ForEach(p =>
            {
                tools.AddButtonToContainer(btnParticipantPrefab, p.GetName(), btnParticipantContainer);
            });

            // Add stats about the simulation on the GUI
            UpdateStats();

            OnSaveSimulationHandler();
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
        /// Handler to save a simulation
        /// </summary>
        private void OnSaveSimulationHandler()
        {
            saveSimulationBtn.onClick.AddListener(() =>
            {
                SaveAllChangedComments();
                simulationManager.SaveCurrentSimulation();
                sceneLoader.LoadScene("PickPreviousSimulationScene");
            });
        }

        /// <summary>
        /// Get the content of each comment, compare it to what it was and change it if it changed
        /// </summary>
        private void SaveAllChangedComments()
        {

            for (int i = 0; i < imageContainer.transform.childCount; i++)
            {
                CommentGO com = imageContainer.transform.GetChild(i).GetComponentInChildren<CommentGO>();
                string content = com.content;

                string inputCommentText = imageContainer.transform.GetChild(i).GetComponentInChildren<TMP_InputField>().text;

                if (content != inputCommentText)
                {
                    Comment comment = simulationManager.currentSimulation.FindComment(c => c.id == com.id);
                    comment.SetContent(inputCommentText);
                }
            }
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        /// <param name="c">Comment to remove</param>
        private void RemoveComment(Comment c)
        {
            RemoveCommentFromGUI(c);
            simulationManager.currentSimulation.RemoveComment(c);
        }

        /// <summary>
        /// Remove a comment from the GUI
        /// </summary>
        /// <param name="c">Comment to remove</param>
        private void RemoveCommentFromGUI(Comment c)
        {

            for (int i = 0; i < imageContainer.transform.childCount; i++)
            {
                GameObject comment = imageContainer.transform.GetChild(i).gameObject;
                if (comment.GetComponent<CommentGO>().id == c.id)
                {
                    Destroy(comment);
                }
            }
        }
    }
}
