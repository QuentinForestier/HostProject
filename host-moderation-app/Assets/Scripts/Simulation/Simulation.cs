using System;
using System.Collections.Generic;
using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class representing a simulation
    /// </summary>
    [Serializable]
    public class Simulation
    {
        /// <summary>
        /// ID of the simulation in the database
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// ID of the simulation in the media server database
        /// </summary>
        public string remoteID { get; private set; }

        /// <summary>
        /// Simulation's name
        /// </summary>
        [SerializeField] private string name;

        /// <summary>
        /// Starting time of the simulation
        /// </summary>
        public DateTime startTime { get; private set; }

        /// <summary>
        /// Ending time of the simulation
        /// </summary>
        public DateTime endTime { get; private set; }

        /// <summary>
        /// Duration of the simulation
        /// </summary>
        public TimeSpan duration { get; private set; }

        /// <summary>
        /// Scenario used for the simulation
        /// </summary>
        public Scenario scenario { get; private set; }

        /// <summary>
        /// List of participant who attend the simulation
        /// </summary>
        [SerializeField] private List<Participant> participants;

        /// <summary>
        /// Duration of the video cuts for the video debriefing
        /// </summary>
        [SerializeField] private string cutDuration;

        /// <summary>
        /// List of comments added during the simulation
        /// </summary>
        public List<Comment> listComments { get; private set; }

        public string videoDebrief { get; private set; }
        public string pdfDebrief { get; private set; }

        public Simulation(Scenario scenario, string name, string cutDuration)
        {
            this.scenario = scenario;
            this.name = name;
            this.cutDuration = cutDuration;
            listComments = new List<Comment>();
            participants = new List<Participant>();
        }

        /// <summary>
        /// Get the simulation's name
        /// </summary>
        /// <returns>Simulation's name</returns>
        public string GetName() { return this.name; }

        public void SetVideoDebrief(string videoDebrief) { this.videoDebrief = videoDebrief; }

        public void SetPdfDebrief(string pdfDebrief) { this.pdfDebrief = pdfDebrief; }

        /// <summary>
        /// Set the database primary key simulation entry
        /// </summary>
        /// <param name="id">Primary key of the simulation entry in the database</param>
        public void SetID(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Set the ID of the simulation in the media server database
        /// </summary>
        /// <param name="id">ID of the simulation in the media server database</param>
        public void SetRemoteID(string id) { this.remoteID = id; }

        #region Comments

        /// <summary>
        /// Find a comment in the simulation based on a predicate
        /// </summary>
        /// <param name="predicate">Predicate matching the comment</param>
        /// <returns>Comment found</returns>
        public Comment FindComment(Predicate<Comment> predicate)
        {
            return listComments.Find(predicate);
        }

        /// <summary>
        /// Add a comment to the simulation
        /// </summary>
        /// <param name="c">Comment object to add</param>
        public void AddComment(Comment c)
        {
            int nextIndex = listComments.Count;
            c.SetID(nextIndex);
            this.listComments.Add(c);
        }

        /// <summary>
        /// Remove a comment from the simulation
        /// </summary>
        /// <param name="c">Comment object to remove</param>
        public void RemoveComment(Comment c) { this.listComments.Remove(c); }

        /// <summary>
        /// Get the number of comment of the simulation
        /// </summary>
        /// <returns>Number of comment of the simulation</returns>
        public int GetNumberOfComment()
        {
            if (this.listComments == null)
            {
                return 0;
            }
            return this.listComments.Count;
        }

        /// <summary>
        /// Set all the comments of the simulation
        /// </summary>
        /// <param name="c">List of comment object</param>
        public void SetListComment(List<Comment> c) { this.listComments = c; }

        #endregion

        #region Scenario

        /// <summary>
        /// Get the scenario used for the simulation
        /// </summary>
        /// <returns>A Scenario object</returns>
        public Scenario GetScenario() { return this.scenario; }

        #endregion

        #region Timer

        /// <summary>
        /// Set the starting time of the simulation to now
        /// </summary>
        public void StartTimer()
        {
            startTime = DateTime.Now;
        }

        /// <summary>
        /// Set the ending time of the simulation and compute the duration
        /// </summary>
        public void StopTimer()
        {
            this.endTime = DateTime.Now;
            duration = this.endTime.Subtract(startTime);
        }

        /// <summary>
        /// Get the duration elapsed since the beginning of the simulation
        /// </summary>
        /// <returns>A string with the time elapsed since the beginning of the simulation</returns>
        public TimeSpan GetTimeElapsed()
        {
            return DateTime.Now.Subtract(startTime);
        }

        /// <summary>
        /// Set the starting time of the simulation
        /// </summary>
        /// <param name="startTime">Time where the simulation started</param>
        public void SetStartTime(DateTime startTime)
        {
            this.startTime = startTime;
        }

        /// <summary>
        /// Set the ending time of the simulation
        /// </summary>
        /// <param name="endTime">Time where the simulation ended</param>
        public void SetEndTime(DateTime endTime)
        {
            this.endTime = endTime;
        }

        /// <summary>
        /// Set the duration of the simulation
        /// </summary>
        /// <param name="duration">Duration of the simulation</param>
        public void SetDuration(TimeSpan duration)
        {
            this.duration = duration;
        }

        #endregion

        #region Participants

        /// <summary>
        /// Get the list of all the participant of the simulation
        /// </summary>
        /// <returns>List of Participant object</returns>
        public List<Participant> GetParticipants() { return this.participants; }

        /// <summary>
        /// Add a participant to the simulation
        /// </summary>
        /// <param name="p">Participant object to add</param>
        public void AddParticipant(Participant p)
        {
            participants.Add(p);
        }

        /// <summary>
        /// Remove a participant from the simulation
        /// </summary>
        /// <param name="name">Name of the participant</param>
        /// <returns>True if it worked, else False</returns>
        public bool RemoveParticipant(string name)
        {
            Participant participant = participants.Find(p => p.GetName() == name);
            return participants.Remove(participant);
        }

        /// <summary>
        /// Find a participant based on a predicate
        /// </summary>
        /// <param name="predicate">Predicate matching the participant</param>
        /// <returns>Participant object found</returns>
        public Participant FindParticipant(Predicate<Participant> predicate)
        {
            return participants.Find(predicate);
        }

        /// <summary>
        /// Get the number of participant in the simulation
        /// </summary>
        /// <returns>Number of participant in the simulation</returns>
        public int GetNumberOfActiveParticipant()
        {
            return participants.Count;
        }

        /// <summary>
        /// Check if a participant has a specific device
        /// </summary>
        /// <param name="ip">Hostname of the device</param>
        /// <returns>True if it does, else False</returns>
        public bool ParticipantHasIP(string ip)
        {
            return participants.Find(p => p.GetIp() == ip) != null;
        }

        /// <summary>
        /// Set all the participants of the simulation
        /// </summary>
        /// <param name="p">List of participant object</param>
        public void SetListParticipant(List<Participant> p) { this.participants = p; }

        #endregion

        /// <summary>
        /// Returns a string representing the simulation
        /// </summary>
        /// <returns>String with the name, starting time and duration of the simulation</returns>
        public override string ToString()
        {
            return this.scenario.name + " | "
                + this.startTime.ToString() + " | "
                + " Duration : " + this.duration.ToString();
        }

    }
}

