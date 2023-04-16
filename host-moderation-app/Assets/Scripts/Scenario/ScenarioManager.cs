using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class holding the current scenario
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        public Scenario currentScenario;

        // Start is called before the first frame update
        void Start()
        {
            // Make sure that the the current gameObject won't be destroyed
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
    }

}
