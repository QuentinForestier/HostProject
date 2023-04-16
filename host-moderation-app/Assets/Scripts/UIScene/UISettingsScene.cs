using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using Host.AppSettings;
using Host.DB;
using Host.Toolbox;

namespace Host.UI
{
    using Host.Network;
    using System;

    /// <summary>
    /// Class holding the logic for the Settings scene
    /// </summary>
    public class UISettingsScene : MonoBehaviour
    {
        [Header("Buttons prefabs")]
        public GameObject btnScenarioPrefab;
        public GameObject btnMessagesPrefab;
        public GameObject btnEventsPrefab;
        public GameObject btnHelpPrefab;
        public GameObject btnDevicePrefab;

        [Header("Containers")]
        public GameObject btnScenarioContainer;
        public GameObject btnMessagesContainer;
        public GameObject btnEventsContainer;
        public GameObject btnHelpContainer;
        public GameObject btnDeviceContainer;

        [Header("Modals edit message")]
        public ModalWindowManager modalEditMessage;
        public TMP_InputField inputEditMessage;
        public Button modalBtnOK;
        public GameObject modalDeleteBtn;

        [Header("Modals add message")]
        public ModalWindowManager modalAddMessage;
        public TMP_InputField inputAddMessage;
        public Button modalBtnAdd;

        [Header("Buttons")]
        public GameObject deleteScenarioBtn;
        public GameObject btnAddMessage;
        public Button backBtn;

        [Header("Notification")]
        public NotificationManager notification;

        [Header("Fields")]
        public SliderManager cutDurationSlider;

        private HelpRPC networkManager;
        private ScenarioManager scenarioManager;
        private SimulationManager simulationManager;
        private Tools tools;
        private DBManager dBManager;
        private Settings currentSettings;
        public SceneLoader sceneLoader;


        // Start is called before the first frame update
        void Start()
        {
            networkManager = GlobalElements.Instance.HelpRPC;
            scenarioManager = GlobalElements.Instance.ScenarioManager;
            simulationManager = GlobalElements.Instance.SimulationManager;
            tools = GlobalElements.Instance.Tools;
            dBManager = GlobalElements.Instance.DBManager;

            // Add the connected devices on the GUI
            var listDevice = HostNetworkManager.HostNetwork.Clients;
            Array.ForEach(listDevice, d => tools.AddButtonToContainer(btnDevicePrefab, d.IP, btnDeviceContainer));

            // Add the different scenario on the GUI
            AddScenarioButtonOnGUI();

            // Get the current settings
            currentSettings = dBManager.GetSettings();

            if(currentSettings == null)
            {

            }

            if (currentSettings != null)
            {
                cutDurationSlider.mainSlider.value = float.Parse(currentSettings.cutDuration);

            }

            backBtn.onClick.AddListener(() => OnBackBtnClickHandler());
        }

        #region Predefined message

        /// <summary>
        /// Update the list of predefined messages
        /// </summary>
        /// <param name="s">Scenario</param>
        private void UpdatePredefinedMessageButtons(Scenario s)
        {
            tools.RemoveAllButtonFromContainer(btnMessagesContainer);

            dBManager.GetAllMessageEventScenario(s.id)?.ForEach(m =>
            {
                GameObject btn = tools.AddButtonToContainer(btnMessagesPrefab, m.content, btnMessagesContainer);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    modalEditMessage.titleText = "Edit message";
                    modalEditMessage.UpdateUI();
                    inputEditMessage.text = m.content;

                    if (!modalDeleteBtn.activeSelf)
                    {
                        tools.ShowButton(modalDeleteBtn);
                    }

                    OnEditPredefinedMessageHandler(s, m);
                    OnDeletePredefinedMessageHandler(s, m);

                    modalEditMessage.OpenWindow();
                });
            });
        }

        /// <summary>
        /// Handler for the edition of predefined message
        /// </summary>
        /// <param name="s">Scenario attached</param>
        /// <param name="m">Message to edit</param>
        private void OnEditPredefinedMessageHandler(Scenario s, MessageEvent m)
        {
            modalBtnOK.onClick.RemoveAllListeners();
            modalBtnOK.onClick.AddListener(() =>
            {
                if (inputEditMessage.text.Length > 0)
                {
                    m.SetContent(inputEditMessage.text);
                    dBManager.UpdateMessageEvent(m);
                    UpdatePredefinedMessageButtons(s);
                }
                else
                {
                    tools.ShowNotification(notification, "Error", "Couldn't update message. Input field empty.");
                }
            });
        }

        /// <summary>
        /// Handler for the deletion of predefined message
        /// </summary>
        /// <param name="s">Scenario attached</param>
        /// <param name="m">Message to edit</param>
        private void OnDeletePredefinedMessageHandler(Scenario s, MessageEvent m)
        {
            modalDeleteBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            modalDeleteBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                dBManager.DeleteMessageEventByID(m.id);
                UpdatePredefinedMessageButtons(s);
            });
        }

        #endregion

        #region GUI

        /// <summary>
        /// Add all the virtual events on the GUI
        /// </summary>
        /// <param name="s">Scenario</param>
        private void AddVirtualEventButtonsOnGUI(Scenario s)
        {
            tools.RemoveAllButtonFromContainer(btnEventsContainer);

            s.virtualEvents?.ForEach(v =>
            {
                tools.AddButtonToContainer(btnEventsPrefab, v.name, btnEventsContainer);
            });
        }

        /// <summary>
        /// Add all the help events on the GUI
        /// </summary>
        /// <param name="s">Scenario</param>
        private void AddHelpEventButtonsOnGUI(Scenario s)
        {
            tools.RemoveAllButtonFromContainer(btnHelpContainer);

            s.helpEvents?.ForEach(h =>
            {
                tools.AddButtonToContainer(btnHelpPrefab, h.name, btnHelpContainer);
            });
        }

        /// <summary>
        /// Add the scenario buttons on the GUI
        /// </summary>
        private void AddScenarioButtonOnGUI()
        {
            dBManager.GetAllScenario()?.ForEach(s =>
            {
                GameObject btnScenario = tools.AddButtonToContainer(btnScenarioPrefab, s.name, btnScenarioContainer);
                btnScenario.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // The selected button changes of color
                    tools.ChangeButtonColorOnSelect(btnScenario, btnScenarioContainer, regular: new Color32(95, 104, 115, 255), Color.black);


                    UpdatePredefinedMessageButtons(s);
                    AddVirtualEventButtonsOnGUI(s);
                    AddHelpEventButtonsOnGUI(s);

                    if (!btnAddMessage.activeSelf)
                    {
                        tools.ShowButton(btnAddMessage);
                    }

                    btnAddMessage.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        modalAddMessage.titleText = "Add message";
                        tools.HideButton(modalDeleteBtn);
                        tools.EmptyInput(inputEditMessage);
                        modalAddMessage.UpdateUI();
                        OnAddMessageHandler(s);
                        modalAddMessage.OpenWindow();
                    });

                    tools.ShowButton(deleteScenarioBtn);

                    OnDeleteScenarioHandler(s);
                });
            });
        }

        /// <summary>
        /// Update the list of scenario
        /// </summary>
        private void UpdateScenarioList()
        {
            tools.RemoveAllButtonFromContainer(btnScenarioContainer);
            AddScenarioButtonOnGUI();
        }

        #endregion

        #region Button handlers

        /// <summary>
        /// Handler for the add message button
        /// </summary>
        /// <param name="s">Scenario attached</param>
        private void OnAddMessageHandler(Scenario s)
        {
            modalBtnAdd.onClick.RemoveAllListeners();
            modalBtnAdd.onClick.AddListener(() =>
            {
                if (inputAddMessage.text.Length > 0)
                {
                    dBManager.PutMessageEvent(new MessageEvent(s.id, "Alert", inputAddMessage.text, "All"));
                    UpdatePredefinedMessageButtons(s);
                }
                else
                {
                    tools.ShowNotification(notification, "Error", "Couldn't add message. Input field empty.");
                }

            });

        }

        /// <summary>
        /// Handler for the deletion of a scenario
        /// </summary>
        /// <param name="s">Scenario</param>
        private void OnDeleteScenarioHandler(Scenario s)
        {
            deleteScenarioBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            deleteScenarioBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                dBManager.DeleteScenario(s);
                UpdateScenarioList();
                tools.HideButton(deleteScenarioBtn);
                tools.HideButton(btnAddMessage);
                tools.RemoveAllButtonFromContainer(btnMessagesContainer);
                tools.RemoveAllButtonFromContainer(btnEventsContainer);
            });
        }

        /// <summary>
        /// Handler for the back button
        /// </summary>
        private void OnBackBtnClickHandler()
        {
            Settings s = new Settings(
                       cutDuration: cutDurationSlider.mainSlider.value.ToString()
            );

            // If no settings have been set we add new ones, else we update the current one
            if (currentSettings == null)
            {
                dBManager.PutSettings(s);
            }
            else
            {
                if (currentSettings.cutDuration != cutDurationSlider.mainSlider.value.ToString())
                {
                    dBManager.UpdateSettings(s);
                }
            }

            // Switch to the main scene
            sceneLoader.LoadScene("EntrypointScene");
        }

        #endregion
    }

}