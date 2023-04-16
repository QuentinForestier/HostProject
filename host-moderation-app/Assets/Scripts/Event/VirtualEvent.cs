namespace Host
{
    /// <summary>
    /// Class representing a Virtual event
    /// </summary>
    public class VirtualEvent : IEvent
    {
        /// <summary>
        /// ID of the event in the database
        /// </summary>
        public int id { get; private set; }
        public byte eventCode { get; private set; } = 2;

        /// <summary>
        /// ID of the scenario in the database
        /// </summary>
        public int scenario { get; private set; }

        /// <summary>
        /// Number describing which action should be triggered
        /// </summary>
        public string action_number { get; private set; }

        /// <summary>
        /// Recipient of the event
        /// </summary>
        public string recipient { get; private set; }

        /// <summary>
        /// Name of the event
        /// </summary>
        public string name { get; private set; }

        public VirtualEvent( int scenario, string action_number, string recipient, string name)
        {
            this.scenario = scenario;
            this.action_number = action_number;
            this.recipient = recipient;
            this.name = name;
        }

        /// <summary>
        /// Get the event recipent
        /// </summary>
        /// <returns>A string with the IP address of the recipient</returns>
        public string GetRecipient() { return this.recipient; }

        /// <summary>
        /// Set the event recipient
        /// </summary>
        /// <param name="ip">IP address of the recipient</param>
        public void SetRecipient(string ip) { this.recipient = ip; }

        /// <summary>
        /// Set the databse ID of the event
        /// </summary>
        /// <param name="id">ID of the event in the database</param>
        public void SetID(int id) { this.id = id; }

        /// <summary>
        /// Get the scenario ID of the event
        /// </summary>
        /// <returns>A string with the scenario ID of the event</returns>
        public string GetScenario(){ return this.scenario.ToString();  }

        /// <summary>
        /// Set the database ID of the scenario associated to the event
        /// </summary>
        /// <param name="scenarioID">Database ID of the scenario associated to the event</param>
        public void SetScenario(int scenarioID) { this.scenario = scenarioID; }

        public byte GetEventCode() { return this.eventCode; }

        /// <summary>
        /// Representation of the Virtual Event as a string
        /// </summary>
        /// <returns>A string with the name of the event and the recipient</returns>
        public override string ToString()
        {
            return "Name : " + this.name + " | Recipient : " + this.recipient;
        }
    }
}
