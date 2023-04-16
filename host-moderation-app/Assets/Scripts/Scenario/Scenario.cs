using System;
using System.Collections.Generic;

namespace Host
{
    /// <summary>
    /// Class representing a scenario
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// Scenario's name
        /// </summary>
        public string name { get; private set; }

        /// <summary>
        /// ID of the scenario in the database
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// List of virtual events associated to the scenario
        /// </summary>
        public List<VirtualEvent> virtualEvents { get; private set; }

        /// <summary>
        /// List of message events associated to the scenario
        /// </summary>
        public List<MessageEvent> messageEvents { get; private set; }

        /// <summary>
        /// List of help events associated to the scenario
        /// </summary>
        public List<HelpEvent> helpEvents { get; private set; }

        public Scenario(string name)
        {
            this.name = name;
            virtualEvents = new List<VirtualEvent>();
            messageEvents = new List<MessageEvent>();
            helpEvents = new List<HelpEvent>();

        }

        /// <summary>
        /// Set the database scenario ID
        /// </summary>
        /// <param name="id">ID of the scenario in the database</param>
        public void SetID(int id) { this.id = id; }

        /// <summary>
        /// Set all the virtual events of the scenario
        /// </summary>
        /// <param name="li">A list of VirtualEvent object</param>
        public void SetVirtualEvents(List<VirtualEvent> li) { this.virtualEvents = li; }

        /// <summary>
        /// Set all the message events of the scenario
        /// </summary>
        /// <param name="li">A list of MessageEvent object</param>
        public void SetMessageEvents(List<MessageEvent> li) { this.messageEvents = li; }

        /// <summary>
        /// Set all the help events of the scenario
        /// </summary>
        /// <param name="li">A list of HelpEvent object</param>
        public void SetHelpEvents(List<HelpEvent> li) { this.helpEvents = li; }

        /// <summary>
        /// Add a virtual event to the scenario
        /// </summary>
        /// <param name="v">VirtualEvent object to add</param>
        public void AddVirtualEvent(VirtualEvent v) { virtualEvents.Add(v); }

        /// <summary>
        /// Add a message event to the scenario
        /// </summary>
        /// <param name="v">MessageEvent object to add</param>
        public void AddMessageEvent(MessageEvent m) { messageEvents.Add(m); }

        /// <summary>
        /// Add a help event to the scenario
        /// </summary>
        /// <param name="v">HelpEvent object to add</param>
        public void AddHelpEvent(HelpEvent h) { helpEvents.Add(h); }

    }

}