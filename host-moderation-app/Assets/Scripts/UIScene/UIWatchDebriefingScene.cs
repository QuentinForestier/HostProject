using Host.AppSettings;
using Host.DB;
using Michsky.UI.ModernUIPack;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Host.Toolbox;

namespace Host.UI
{
    /// <summary>
    /// Class holding the logic for the WatchDebriefing scene
    /// </summary>
    public class UIWatchDebriefingScene : MonoBehaviour
    {
        [Header("Video")]
        public GameObject videoContainer;
        public GameObject videoPrefab;

        [Header("UI elements")]
        public GameObject loop;
        public NotificationManager notification;

        private SimulationManager simulationManager;
        private Tools tools;
        private DBManager dbManager;
        private Settings currentSettings;

        // Start is called before the first frame update
        void Start()
        {
            simulationManager = GlobalElements.Instance.SimulationManager;
            tools = GlobalElements.Instance.Tools;
            dbManager = GlobalElements.Instance.DBManager;

            currentSettings = dbManager.GetSettings();
        }
    }
}
