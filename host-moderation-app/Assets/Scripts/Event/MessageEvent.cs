namespace Host
{
    /// <summary>
    /// Class representing a Message event
    /// </summary>
    public class MessageEvent : IEvent
    {
        /// <summary>
        /// ID of the event in the database
        /// </summary>
        public int id { get; private set; }
        public byte eventCode { get; private set; } = 1;

        /// <summary>
        /// ID of the scenario in the database
        /// </summary>
        public int scenario { get; private set; }

        /// <summary>
        /// Type of message event
        /// </summary>
        public string type { get; private set; }

        /// <summary>
        /// Content of the message event
        /// </summary>
        public string content { get; private set; }

        /// <summary>
        /// Recipient of the event
        /// </summary>
        public string recipient { get; private set; }

        public MessageEvent(int scenario, string type, string content, string recipient)
        {
            this.scenario = scenario;
            this.type = type;
            this.content = content;
            this.recipient = recipient;
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
        /// Set the content of the message event
        /// </summary>
        /// <param name="content">Content of the message</param>
        public void SetContent(string content) { this.content = content; }

        /// <summary>
        /// Get the scenario ID of the event
        /// </summary>
        /// <returns>A string with the scenario ID of the event</returns>
        public string GetScenario() { return this.scenario.ToString(); }

        /// <summary>
        /// Set the database ID of the scenario associated to the event
        /// </summary>
        /// <param name="scenarioID">Database ID of the scenario associated to the event</param>
        public void SetScenario(int scenarioID) { this.scenario = scenarioID; }

        public void SetEventCode(byte eventCode) { this.eventCode = eventCode; }

        public byte GetEventCode() { return this.eventCode; }

        /// <summary>
        /// Representation of the Message Event as a string
        /// </summary>
        /// <returns>A string with the content and the recipient of the event</returns>
        public override string ToString()
        {
            return "Content : " + this.content + " | Recipient : " + this.recipient;
        }
    }
}
