using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class representing a participant
    /// </summary>
    [Serializable]
    public class Participant
    {
        /// <summary>
        /// ID of the participant in the database
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// Name of the participant
        /// </summary>
        [SerializeField] private string name;

        /// <summary>
        /// Role of the participant (Actor/Student)
        /// </summary>
        public string role { get; private set; }

        /// <summary>
        /// HoloLens associated to the participant
        /// </summary>
        [SerializeField] private string ip;

        /// <summary>
        /// ID of the simulation in the database
        /// </summary>
        public int simulation { get; private set; }

        public Participant(string name, string role, string ip)
        {
            this.name = name;
            this.role = role;
            this.ip = ip;
        }

        /// <summary>
        /// Set the participant database ID
        /// </summary>
        /// <param name="id">Primary key of the participant in the database</param>
        public void SetID(int id) { this.id = id; }

        /// <summary>
        /// Set the ID of the simulation in the database
        /// </summary>
        /// <param name="simulationID">Primary key of the simulation in the database</param>
        public void SetSimulation(int simulationID) { this.simulation = simulationID; }

        /// <summary>
        /// Get the participant's name
        /// </summary>
        /// <returns>Participant's name</returns>
        public string GetName() { return this.name; }

        /// <summary>
        /// Get the device associated to the participant
        /// </summary>
        /// <returns>A Device object</returns>
        public string GetIp() { return this.ip; }
    }

}
