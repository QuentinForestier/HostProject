using Host.DB;
using UnityEngine;
using UnityEngine.UI;
using Host.Toolbox;

namespace Host.UI
{
    /// <summary>
    /// Class holding the logic for the PickPreviousSimulation scene
    /// </summary>
    public class UIPickPreviousSimulationScene : MonoBehaviour
    {
        [Header("Previous simulation")]
        public GameObject btnPreviousSimuPrefab;
        public GameObject btnListPreviousSimuContainer;

        [Header("Scene management")]
        public string nextScene;
        public SceneLoader sceneLoader;

        private SimulationManager simulationManager;
        private Tools tools;
        private DBManager dBManager;

        // Start is called before the first frame update
        void Start()
        {
            simulationManager = GlobalElements.Instance.SimulationManager;
            tools = GlobalElements.Instance.Tools;
            dBManager = GlobalElements.Instance.DBManager;

            // Display all the previous simulations on the GUI
            dBManager.GetAllSimulations()?.ForEach(s =>
            {
                string btnName = s.GetScenario().name + " | " + s.startTime;
                GameObject btn = tools.AddButtonToContainer(btnPreviousSimuPrefab, btnName, btnListPreviousSimuContainer);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    simulationManager.simulationReviewed = s;
                    sceneLoader.LoadScene(nextScene);
                });
            });

        }
    }
}
