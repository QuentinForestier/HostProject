using System;
using System.Collections.Generic;
using UnityEngine;
using Host.DB;

namespace Host
{
    /// <summary>
    /// Class storing informations about the current simulation and the simulation reviewed
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        public Simulation currentSimulation;
        public Simulation simulationReviewed;
        private DBManager dBManager;

        // Start is called before the first frame update
        void Start()
        {
            try
            {
                dBManager = GlobalElements.Instance.DBManager;
            }
            catch
            {
                Debug.LogError("[SimulationManager] - Couldn't initialize DBManager");
                return;
            }


            // Make sure that the the current gameObject won't be destroyed
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Save the current simulation in the database
        /// </summary>
        public void SaveCurrentSimulation()
        {
            if (dBManager.PutSimulation(currentSimulation) != -1)
            {
                currentSimulation = null;
            }
            else
            {
                Debug.LogError("[SimulationManager] - Couldn't save current simulation in DB");
            }

        }
    }

}
