using TMPro;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using System;
using Host.DB;
using Host.Network;
using Host.Toolbox;

namespace Host.UI
{
    /// <summary>
    /// Class holding the logic for the AddParticipant scene
    /// </summary>
    public class UIAddParticipantScene : MonoBehaviour
    {
        [Header("Participants")]
        public GameObject btnParticipantPrefab;
        public GameObject btnParticipantContainer;

        [Header("Modal add participant")]
        public TMP_InputField inputParticipantName;
        public CustomDropdown holoLensDropdown;
        public GameObject holoLensDropdownListContainer;
        public TextMeshProUGUI selectedTextholoLensDropdown;
        public CustomDropdown roleDropdown;
        public Sprite holoLensLogo;

        [Header("Modal edit participant")]
        public ModalWindowManager modalEditParticipant;
        public Button btnDeleteParticipant;

        [Header("Notifications")]
        public NotificationManager notification;

        [Header("Visual")]
        public Button startButton;

        [Header("Scene management")]
        public string nextScene;
        public SceneLoader sceneLoader;

        private HelpRPC networkManager;

        private SimulationManager simulationManager;

        private ScenarioManager scenarioManager;

        private Tools tools;
        private DBManager dbManager;

        // Start is called before the first frame update
        void Start()
        {
            networkManager = GlobalElements.Instance.HelpRPC;
            simulationManager = GlobalElements.Instance.SimulationManager;
            scenarioManager = GlobalElements.Instance.ScenarioManager;
            dbManager = GlobalElements.Instance.DBManager;
            tools = GlobalElements.Instance.Tools;

            // Creation of the simulation
            Simulation s = new Simulation(
                scenario: scenarioManager.currentScenario,
                name: scenarioManager.currentScenario.name.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMddHHmm"),
                cutDuration: dbManager.GetSettings().cutDuration
            );

            simulationManager.currentSimulation = s;

            // Set up the start scenario button
            startButton.onClick.AddListener(() =>
            {
                // If participant have been added we start the simulation timer and load the next scene
                if (simulationManager.currentSimulation.GetNumberOfActiveParticipant() > 0)
                {
                    simulationManager.currentSimulation.StartTimer();
                    sceneLoader.LoadScene(nextScene);
                }
                else
                {
                    tools.ShowNotification(notification, "Error", "Oups, you didn't add any participant");
                }
            });

            // If some participant have already been added to this simulation we add them on the GUI
            AddAllButtonExistingParticipant();

            // Automatically adds participants to prevent having to do the setup manually
            AddDefaultParticipants();

            UpdateDeviceDropdown();
        }

        public void AddDefaultParticipants()
        {
            int i = 1;
            Array.ForEach(HostNetworkManager.HostNetwork.Clients, (d =>
            {
                if (!simulationManager.currentSimulation.ParticipantHasIP(d.IP))
                {
                    AddParticipant($"Participant {i}", d.IP, "Student");
                    i++;
                }
            }));
        }

        public void AddParticipant(string name, string hololens, string role)
        {
            // We take all the inputs and make sure they are not empty
            if (name != string.Empty && hololens != string.Empty && role != string.Empty)
            {
                HostTcpClient d = Array.Find(HostNetworkManager.HostNetwork.Clients, (p) => p.IP == hololens);

                AddButtonParticipant(name, hololens, role);
                simulationManager.currentSimulation.AddParticipant(new Participant(name, role, d.IP));

                tools.EmptyInput(inputParticipantName);
            }
            else
            {
                tools.ShowNotification(notification, "Error", "Couldn't be added, field missing");
                tools.EmptyInput(inputParticipantName);
            }
        }

        /// <summary>
        /// Handler when a participant is added
        /// </summary>
        public void AddParticipant()
        {
            string name = inputParticipantName.text;
            string hololens = holoLensDropdown.selectedText.text;
            string role = roleDropdown.selectedText.text;

            // We take all the inputs and make sure they are not empty
            if (name != string.Empty && hololens != string.Empty && role != string.Empty)
            {
                HostTcpClient d = Array.Find(HostNetworkManager.HostNetwork.Clients, (p) => p.IP == hololens);

                AddButtonParticipant(name, hololens, role);
                simulationManager.currentSimulation.AddParticipant(new Participant(name, role, d.IP));

                tools.EmptyInput(inputParticipantName);
            }
            else
            {
                tools.ShowNotification(notification, "Error", "Couldn't be added, field missing");
                tools.EmptyInput(inputParticipantName);
            }
        }

        /// <summary>
        /// Add a button with the participant informations on the GUI
        /// </summary>
        /// <param name="name">Name of the participant</param>
        /// <param name="hololens">Name of the HoloLens</param>
        /// <param name="role">Role of the participant</param>
        private void AddButtonParticipant(string name, string ip, string role)
        {
            string btnName = name + " | " + role + " | " + ip;
            GameObject btn = tools.AddButtonToContainer(btnParticipantPrefab, btnName, btnParticipantContainer);
            btn.GetComponent<Button>().onClick.AddListener(() => {
                // Handler for the deletion of a participant
                btnDeleteParticipant.onClick.AddListener(() =>
                {
                    if (simulationManager.currentSimulation.RemoveParticipant(name))
                    {
                        tools.RemoveButtonByName(btnName, btnParticipantContainer);
                    }
                    else
                    {
                        tools.ShowNotification(notification, "Error", "Couldn't remove participant");
                    }
                });

                modalEditParticipant.titleText = name;
                modalEditParticipant.descriptionText = "Do you really want to delete this participant ?";
                modalEditParticipant.UpdateUI();
                modalEditParticipant.OpenWindow();
            });
        }

        /// <summary>
        /// Add button of all the existing participant
        /// </summary>
        private void AddAllButtonExistingParticipant()
        {
            if (simulationManager.currentSimulation.GetNumberOfActiveParticipant() > 0)
            {
                simulationManager.currentSimulation.GetParticipants().ForEach(p => AddButtonParticipant(p.GetName(), p.GetIp(), p.role));
            }
        }

        /// <summary>
        /// Update the device dropdown with the current available device
        /// </summary>
        private void UpdateDeviceDropdown()
        {
            tools.RemoveAllButtonFromContainer(holoLensDropdownListContainer);

            int dropdownCount = 0;

            Array.ForEach(HostNetworkManager.HostNetwork.Clients, (d =>
            {
                if (!simulationManager.currentSimulation.ParticipantHasIP(d.IP))
                {
                    holoLensDropdown.SetItemTitle(d.IP);
                    holoLensDropdown.SetItemIcon(holoLensLogo);
                    holoLensDropdown.CreateNewItem();
                    dropdownCount += 1;
                }

            }));

            if (dropdownCount == 0)
            {
                selectedTextholoLensDropdown.text = "";
            }
        }
    }

}
