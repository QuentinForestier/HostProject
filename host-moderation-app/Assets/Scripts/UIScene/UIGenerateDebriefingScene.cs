using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Host.AppSettings;
using Host.DB;
using Host.Toolbox;

namespace Host.UI
{
    using Host.Network;
    using Michsky.UI.ModernUIPack;
    using SimpleFileBrowser;

    /// <summary>
    /// Class holding the logic for the GenerateDebriefing scene
    /// </summary>
    public class UIGenerateDebriefingScene : MonoBehaviour
    {
        [Header("Buttons")]
        public Button btnPDF;
        public Button btnVideo;
        public GameObject btnBack;

        [Header("Overlay")]
        public CanvasGroup Overlay;
        public NotificationManager Notification;

        [Header("Report tools")]
        public PdfReport pdfReport;

        private SimulationManager simulationManager;
        private Tools tools;
        private DBManager dbManager;
        private Settings currentSettings;

        // Start is called before the first frame update
        void Start()
        {
            var globalElements = GlobalElements.Instance;
            if (globalElements != null)
            {
                simulationManager = GlobalElements.Instance.SimulationManager;
                dbManager = GlobalElements.Instance.DBManager;
                tools = GlobalElements.Instance.Tools;

                currentSettings = dbManager.GetSettings();
            }

            btnPDF.onClick.AddListener(() =>
            {
                GeneratePDF();
            });

            btnVideo.onClick.AddListener(() =>
            {
                GenerateVideo();
            });

            // Get the previous scene so it can come back to it when the back button is pressed
            btnBack.GetComponent<Button>().onClick.AddListener(() =>
            {
                SceneLoader sl = new SceneLoader();
                sl.LoadScene(SceneLoader.previousScene);
            });

            Overlay.alpha = 0;
            Overlay.blocksRaycasts = false;
        }

        private void GeneratePDF()
        {
            // Show an overlay and block inputs for the rest of the application
            Overlay.alpha = 0.8f;
            Overlay.blocksRaycasts = true;

            // Enable spinner

            FileBrowser.SetFilters(true, new FileBrowser.Filter("Pdf files", ".pdf"));
            FileBrowser.SetDefaultFilter(".pdf");

            StartCoroutine(ShowSaveDialogCoroutine());
        }

		IEnumerator ShowSaveDialogCoroutine()
		{
			yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, null, "Select File To Save", "Save");

			if (FileBrowser.Success)
			{
                string filename = FileBrowser.Result[0];

                if(simulationManager == null || simulationManager.simulationReviewed == null)
                {
                    // No data means we are debugging the functionnality -> use the test save
                    pdfReport.SaveTestButton(filename);
                }
                else
                {
                    var simulation = simulationManager.simulationReviewed;

                    // Save file
                    pdfReport.CreateDocumentAsTask(simulation.listComments, filename);
                }

                StartCoroutine(WaitForSaveCompletion());
            }
            else
            {
                Overlay.alpha = 0;
                Overlay.blocksRaycasts = false;
            }
        }

        IEnumerator WaitForSaveCompletion()
        {
            yield return new WaitUntil(() => !pdfReport.IsSavingInProgress());

            if(pdfReport.Success)
            {
                Notification.title = "Success";
                Notification.description = "File saved succesfully";
                Notification.UpdateUI();
                Notification.OpenNotification();
            }
            else
            {
                Notification.title = "Error";
                Notification.description = "An error occured while exporting the file, check that the document is not open.";
                Notification.UpdateUI();
                Notification.OpenNotification();
            }

            // Disable spinner and overlay
            Overlay.alpha = 0;
            Overlay.blocksRaycasts = false;
        }

        private void GenerateVideo()
        {

        }
    }

}