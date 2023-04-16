using Host.DB;
using Host.Network;
using Host.Toolbox;
using UnityEngine;

namespace Host
{
    public class GlobalElements : MonoBehaviour
    {
        public static GlobalElements Instance { get; private set; }

        public ScenarioManager ScenarioManager;
        public DBManager DBManager;
        public HelpRPC HelpRPC;
        public SimulationManager SimulationManager;
        public Tools Tools;
        public HostNetworkManager HostNetworkManager;
        public FMNetworkManager FMNetworkManager;

        public void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            Application.targetFrameRate = 30;
        }
    }
}