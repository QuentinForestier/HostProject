using UnityEngine;
using UnityEngine.UI;
using Host.DB;
using Host.Toolbox;

namespace Host.UI
{
    /// <summary>
    /// Class holding the logic for the PickScenario scene
    /// </summary>
    public class UIPickScenarioScene : MonoBehaviour
    {
        [Header("Scenarios")]
        public GameObject btnScenarioPrefab;
        public GameObject btnScenarioContainer;

        [Header("Scene management")]
        public string nextScene;
        public SceneLoader sceneLoader;

        private ScenarioManager scenarioManager;

        private Tools tools;
        private DBManager dBManager;

        // Start is called before the first frame update
        void Start()
        {
            scenarioManager = GlobalElements.Instance.ScenarioManager;
            tools = GlobalElements.Instance.Tools;
            dBManager = GlobalElements.Instance.DBManager;

            // Display all the available scenarios on the GUI
            dBManager.GetAllScenario()?.ForEach(s =>
            {
                GameObject btn = tools.AddButtonToContainer(btnScenarioPrefab, s.name, btnScenarioContainer);
                btn.GetComponent<Button>().onClick.AddListener(() => {
                    scenarioManager.currentScenario = s;
                    sceneLoader.LoadScene(nextScene);
                });
            });
        }
    }

}

