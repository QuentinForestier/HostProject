using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using Host.AppSettings;
using System.IO;

namespace Host.DB
{
    /// <summary>
    /// Store all the persistent data of the app
    /// </summary>
    public class DBManager : MonoBehaviour
    {
        /// <summary>
        /// Database name
        /// </summary>
        public string dbName;

        /// <summary>
        /// Path where the database is stored
        /// </summary>
        private string dbPath;

        /// <summary>
        /// Reference to the database
        /// </summary>
        private IDbConnection db;

        // Start is called before the first frame update
        void Start()
        {
            if(!dbName.EndsWith(".db"))
            {
                dbName += ".db";
            }

#if UNITY_WSA || UNITY_EDITOR_WIN
            string directoryPath = Application.dataPath + "/../Simulations/";

            // Create the path if it doesn't exist
            DirectoryInfo di = Directory.CreateDirectory(directoryPath);

            dbPath = "URI=file:" + directoryPath + dbName;
#else
            dbPath = "URI=file:" + Application.persistentDataPath + "/" + dbName;
#endif

            Debug.Log("[DB] - Database stored under " + dbPath);

            // Creation of the database and its tables
            CreateDB(dbPath);
            CreateTable(SCENARIO_TABLE, CREATE_SCENARIO_TABLE);
            CreateTable(VIRTUAL_EVENT_TABLE, CREATE_VIRTUAL_EVENT_TABLE);
            CreateTable(HELP_EVENT_TABLE, CREATE_HELP_EVENT_TABLE);
            CreateTable(MESSAGE_EVENT_TABLE, CREATE_MESSAGE_EVENT_TABLE);
            CreateTable(SIMULATION_TABLE, CREATE_SIMULATION_TABLE);
            CreateTable(PARTICIPANT_TABLE, CREATE_PARTICIPANT_TABLE);
            CreateTable(COMMENT_TABLE, CREATE_COMMENT_TABLE);
            CreateTable(SETTINGS_TABLE, CREATE_SETTINGS_TABLE);

            // Create the two original scenarios if they don't exist
            if (GetScenarioByID(0) == null && GetScenarioByID(1) == null)
            {
                Scenario sce1 = new Scenario("Scenario Avion");
                sce1.AddVirtualEvent(new VirtualEvent(scenario: sce1.id, "1", recipient: "All", name: "Passager se lève"));
                sce1.AddVirtualEvent(new VirtualEvent(scenario: sce1.id, "2", recipient: "All", name: "Commandant de bord crie"));
                sce1.AddVirtualEvent(new VirtualEvent(scenario: sce1.id, "3", recipient: "All", name: "Bruit stressant"));
                sce1.AddVirtualEvent(new VirtualEvent(scenario: sce1.id, "4", recipient: "All", name: "Annonce simple beep"));

                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Le code est en rapport avec le soleil, pensez à fouiller le sac", recipient: "All"));
                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Le modèle de l''avion est affiché quelque part", recipient: "All"));
                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Ces formes sont présentes à différents endroits...", recipient: "All"));
                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Certains objets peuvent être manipulés à la main", recipient: "All"));
                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Attention ! Vous oubliez votre patiente", recipient: "All"));
                sce1.AddMessageEvent(new MessageEvent(scenario: sce1.id, type: "Alert", content: "Il vous reste 10min", recipient: "All"));

                sce1.AddHelpEvent(new HelpEvent(scenario: sce1.id, action_number: "1", recipient: "All", name: "Indice flèche sac"));

                Scenario sce2 = new Scenario("Scenario Désert");
                sce2.AddVirtualEvent(new VirtualEvent(scenario: sce2.id, "1", recipient: "All", name: "Horse runs"));
                sce2.AddVirtualEvent(new VirtualEvent(scenario: sce2.id, "2", recipient: "All", name: "Sand stormd"));
                sce2.AddVirtualEvent(new VirtualEvent(scenario: sce2.id, "3", recipient: "All", name: "Bird fly over"));
                sce2.AddVirtualEvent(new VirtualEvent(scenario: sce2.id, "4", recipient: "All", name: "Wind noise"));

                sce2.AddMessageEvent(new MessageEvent(scenario: sce2.id, type: "Alert", content: "Look at the sand", recipient: "All"));
                sce2.AddMessageEvent(new MessageEvent(scenario: sce2.id, type: "Alert", content: "Ask for help", recipient: "All"));
                sce2.AddMessageEvent(new MessageEvent(scenario: sce2.id, type: "Alert", content: "Be less loud", recipient: "All"));

                sce2.AddHelpEvent(new HelpEvent(scenario: sce2.id, action_number: "1", recipient: "All", name: "Hint sand"));

                PutScenario(sce1);
                PutScenario(sce2);

                Debug.Log("[DB] - Added two predefined scenarios");
            }

            // Create default settings if they don't exist
            if(GetSettings() == null)
            {
                Settings s = new Settings(
                       cutDuration: "5"
                );

                PutSettings(s);

                Debug.Log("[DB] - Added default settings");
            }

            // Make sure that the the current gameObject won't be destroyed
            GameObject.DontDestroyOnLoad(this.gameObject);

        }

#region Utils

        /// <summary>
        /// Create the database
        /// </summary>
        /// <param name="path">Path of to the database file</param>
        public void CreateDB(string path)
        {
            db = new SqliteConnection(path);
            db.Open();
        }

        /// <summary>
        /// Close the database connection
        /// </summary>
        public void CloseDB()
        {
            Debug.Log("[DB] - Closing DB");
            db.Close();
        }

        /// <summary>
        /// Create a table in the database
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="query">Query to create the table</param>
        public void CreateTable(string name, string query)
        {
            Debug.Log("[DB] - Creating table " + name);
            IDbCommand cmd = db.CreateCommand();
            cmd.CommandText = query;
            cmd.ExecuteReader();
        }


        /// <summary>
        /// Get the next primary from a table
        /// </summary>
        /// <param name="table">Table in which to look for</param>
        /// <returns>Next primary key</returns>
        public int GetNextPrimaryKey(string table)
        {
            string query = $"SELECT * FROM {table} ORDER BY id DESC LIMIT 1";
            IDataReader reader = Get(query);
            if (reader[0] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(reader[0]) + 1;
        }

        /// <summary>
        /// Read data from the DB
        /// </summary>
        /// <param name="query">The query to perform</param>
        /// <returns>A reader</returns>
        public IDataReader Get(string query)
        {
            IDbCommand r = db.CreateCommand();
            r.CommandText = query;
            return r.ExecuteReader();
        }


        /// <summary>
        /// PUT data in the DB
        /// </summary>
        /// <param name="query">>The query to perform</param>
        public bool Put(string query, List<SqliteParameter> parameters = null)
        {
            try
            {
                IDbCommand cmnd = db.CreateCommand();
                cmnd.CommandText = query;

                if(parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmnd.Parameters.Add(param);
                    }
                }

                cmnd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Debug.LogWarning("[DB] - Couln't add data with query : " + query
                    + $"\n{e.Message}" +
                    $"\n{e.StackTrace}\n");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Delete an entry from a table based on the entry id
        /// </summary>
        /// <param name="table">Table in which the data is stored</param>
        /// <param name="id">Entry id</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteEntryByID(string table, int id)
        {
            string query = "DELETE FROM " + table
                + " WHERE "
                + "id = " + id;

            return Put(query);
        }

#endregion

#region Scenario

        // Scenario table
        private const string SCENARIO_TABLE = "scenario";
        private const string SCE_ID = "id";
        private const string SCE_NAME = "name";

        private readonly string CREATE_SCENARIO_TABLE =
            $"CREATE TABLE IF NOT EXISTS {SCENARIO_TABLE} " +
            $"({SCE_ID} INTEGER PRIMARY KEY, " +
            $"{SCE_NAME} TEXT)";

        /// <summary>
        /// Add a scenario in the DB
        /// </summary>
        /// <param name="s">Scenario to add in the DB</param>
        /// <returns>The primary key of the element if it worked, else -1</returns>
        public int PutScenario(Scenario s)
        {
            int nextPrimaryKey = GetNextPrimaryKey(SCENARIO_TABLE);
            string query = "INSERT INTO " + SCENARIO_TABLE
                + " ( "
                + SCE_ID + ", "
                + SCE_NAME + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + s.name + "' )";

            if (!Put(query)) { return -1; }

            // Add the virtual events of this scenario in the db
            s.virtualEvents.ForEach(v => {
                v.SetScenario(nextPrimaryKey);
                PutVirtualEvent(v);
            });

            // Add the message events of this scenario in the db
            s.messageEvents.ForEach(m => {
                m.SetScenario(nextPrimaryKey);
                PutMessageEvent(m);
            });

            // Add the help events of this scenario in the db
            s.helpEvents.ForEach(h =>
            {
                h.SetScenario(nextPrimaryKey);
                PutHelpEvent(h);
            });

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get a scenario from the DB based on its id 
        /// </summary>
        /// <param name="id">Scenario id</param>
        /// <returns>The scenario if found, else null</returns>
        public Scenario GetScenarioByID(int id)
        {
            string query = $"SELECT * FROM {SCENARIO_TABLE} WHERE {SCE_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get scenario with id " + id);
                return null;
            }

            Scenario s = new Scenario(name: reader[1].ToString());
            s.SetID(Convert.ToInt32(reader[0]));

            // Get all the virtual, message and help events of this scenario
            s.SetVirtualEvents(GetAllVirtualEventScenario(id));
            s.SetMessageEvents(GetAllMessageEventScenario(id));
            s.SetHelpEvents(GetAllHelpEventScenario(id));

            return s;
        }

        /// <summary>
        /// Get all the scenario present in the DB
        /// </summary>
        /// <returns>A list with all the scenario found</returns>
        public List<Scenario> GetAllScenario()
        {
            string query = $"SELECT {SCE_ID} FROM {SCENARIO_TABLE}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get all scenarios ");
                return null;
            }

            List<Scenario> scenarios = new List<Scenario>();

            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                scenarios.Add(GetScenarioByID(id));
            }

            return scenarios;
        }

        /// <summary>
        /// Delete a scenario
        /// </summary>
        /// <param name="s">The scenario object to delete</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteScenario(Scenario s)
        {
            s.messageEvents?.ForEach(m => DeleteMessageEventByID(m.id));
            s.virtualEvents?.ForEach(v => DeleteVirtualEventByID(v.id));
            s.helpEvents?.ForEach(h => DeleteHelpEventByID(h.id));

            return DeleteEntryByID(SCENARIO_TABLE, s.id);
        }

#endregion

#region Virtual event

        // Virtual Event table
        private const string VIRTUAL_EVENT_TABLE = "virtual_event";
        private const string VE_ID = "id";
        private const string VE_EVENT_CODE = "event_code";
        private const string VE_SCENARIO_ID = "scenario_id";
        private const string VE_ACTION_NUMBER = "action_number";
        private const string VE_RECIPIENT = "recipient";
        private const string VE_NAME = "name";

        private readonly string CREATE_VIRTUAL_EVENT_TABLE =
            $"CREATE TABLE IF NOT EXISTS {VIRTUAL_EVENT_TABLE} " +
            $"( {VE_ID} INTEGER PRIMARY KEY, " +
            $"{VE_EVENT_CODE} INTEGER, " +
            $"{VE_SCENARIO_ID} INTEGER, " +
            $"{VE_ACTION_NUMBER} INTEGER, " +
            $"{VE_RECIPIENT} TEXT, " +
            $"{VE_NAME} TEXT)";

        /// <summary>
        /// Add a virtual event in the DB
        /// </summary>
        /// <param name="v">Virtual event to add in the DB</param>
        /// <returns>The primary key of the element if it worked, else -1</returns>
        public int PutVirtualEvent(VirtualEvent v)
        {
            int nextPrimaryKey = GetNextPrimaryKey(VIRTUAL_EVENT_TABLE);
            string query = "INSERT INTO " + VIRTUAL_EVENT_TABLE
                + " ( "
                + VE_ID + ", "
                + VE_EVENT_CODE + ", "
                + VE_SCENARIO_ID + ", "
                + VE_ACTION_NUMBER + ", "
                + VE_RECIPIENT + ", "
                + VE_NAME + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + v.eventCode + "', '"
                + v.scenario + "', '"
                + v.action_number + "', '"
                + v.recipient + "', '"
                + v.name + "' )";

            if (!Put(query)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get a virtual event from the DB based on its id 
        /// </summary>
        /// <param name="id">Virtual event id</param>
        /// <returns>The virtual event if found, else null</returns>
        public VirtualEvent GetVirtualEventByID(int id)
        {
            string query = $"SELECT * FROM {VIRTUAL_EVENT_TABLE} WHERE {SCE_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get virtual event with id " + id);
                return null;
            }

            VirtualEvent v = new VirtualEvent(
                scenario: Convert.ToInt32(reader[2]),
                action_number: reader[3].ToString(),
                recipient: reader[4].ToString(),
                name: reader[5].ToString()
                );
            v.SetID(Convert.ToInt32(reader[0]));

            return v;

        }

        /// <summary>
        /// Get all the virtual events from a specific scenario
        /// </summary>
        /// <param name="scenarioID">The scenario id</param>
        /// <returns>A list with all the virtual events found</returns>
        public List<VirtualEvent> GetAllVirtualEventScenario(int scenarioID)
        {
            string query = $"SELECT * FROM {VIRTUAL_EVENT_TABLE} WHERE {VE_SCENARIO_ID}={scenarioID}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get all virtual event with scenario id " + scenarioID);
                return null;
            }

            List<VirtualEvent> events = new List<VirtualEvent>();

            while (reader.Read())
            {
                VirtualEvent v = new VirtualEvent(
                    scenario: scenarioID,
                    action_number: reader[3].ToString(),
                    recipient: reader[4].ToString(),
                    name: reader[5].ToString());
                v.SetID(Convert.ToInt32(reader[0]));

                events.Add(v);
            }

            return events;
        }

        /// <summary>
        /// Delete a virtual event based on its id
        /// </summary>
        /// <param name="id">The virtual event id</param>
        /// <returns>True if deleted, else False</returns>
        public bool DeleteVirtualEventByID(int id)
        {
            return DeleteEntryByID(VIRTUAL_EVENT_TABLE, id);
        }

#endregion

#region Help event

        // Help Event table
        private const string HELP_EVENT_TABLE = "help_event";
        private const string HE_ID = "id";
        private const string HE_EVENT_CODE = "event_code";
        private const string HE_SCENARIO_ID = "scenario_id";
        private const string HE_ACTION_NUMBER = "action_number";
        private const string HE_RECIPIENT = "recipient";
        private const string HE_NAME = "name";

        private readonly string CREATE_HELP_EVENT_TABLE =
            $"CREATE TABLE IF NOT EXISTS {HELP_EVENT_TABLE} " +
            $"( {HE_ID} INTEGER PRIMARY KEY, " +
            $"{HE_EVENT_CODE} INTEGER, " +
            $"{HE_SCENARIO_ID} INTEGER, " +
            $"{HE_ACTION_NUMBER} INTEGER, " +
            $"{HE_RECIPIENT} TEXT, " +
            $"{HE_NAME} TEXT)";

        /// <summary>
        /// Add an help event in the DB
        /// </summary>
        /// <param name="h">Help event to add in the DB</param>
        /// <returns>The primary key of the element if it worked, else -1</returns>
        public int PutHelpEvent(HelpEvent h)
        {
            int nextPrimaryKey = GetNextPrimaryKey(HELP_EVENT_TABLE);
            string query = "INSERT INTO " + HELP_EVENT_TABLE
                + " ( "
                + HE_ID + ", "
                + HE_EVENT_CODE + ", "
                + HE_SCENARIO_ID + ", "
                + HE_ACTION_NUMBER + ", "
                + HE_RECIPIENT + ", "
                + HE_NAME + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + h.eventCode + "', '"
                + h.scenario + "', '"
                + h.action_number + "', '"
                + h.recipient + "', '"
                + h.name + "' )";

            if (!Put(query)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get an help event from the DB based on its id 
        /// </summary>
        /// <param name="id">Help event id</param>
        /// <returns>The virtual event if found, else null</returns>
        public HelpEvent GetHelpEventByID(int id)
        {
            string query = $"SELECT * FROM {HELP_EVENT_TABLE} WHERE {HE_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get help event with id " + id);
                return null;
            }

            HelpEvent h = new HelpEvent(
                scenario: Convert.ToInt32(reader[2]),
                action_number: reader[3].ToString(),
                recipient: reader[4].ToString(),
                name: reader[5].ToString()
                );
            h.SetID(Convert.ToInt32(reader[0]));

            return h;

        }

        /// <summary>
        /// Get all the help event of a scenario 
        /// </summary>
        /// <param name="scenarioID">Scenario id</param>
        /// <returns>A list of help event if found, else null</returns>
        public List<HelpEvent> GetAllHelpEventScenario(int scenarioID)
        {
            string query = $"SELECT * FROM {HELP_EVENT_TABLE} WHERE {HE_SCENARIO_ID}={scenarioID}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get all help events with scenario id " + scenarioID);
                return null;
            }

            List<HelpEvent> events = new List<HelpEvent>();

            while (reader.Read())
            {
                HelpEvent h = new HelpEvent(
                    scenario: scenarioID,
                    action_number: reader[3].ToString(),
                    recipient: reader[4].ToString(),
                    name: reader[5].ToString());
                h.SetID(Convert.ToInt32(reader[0]));

                events.Add(h);
            }

            return events;
        }

        /// <summary>
        /// Delete an help event from the DB based on its id 
        /// </summary>
        /// <param name="id">Help event id</param>
        /// <returns>True if the value was removed, else false</returns>
        public bool DeleteHelpEventByID(int id)
        {
            return DeleteEntryByID(HELP_EVENT_TABLE, id);
        }

#endregion

#region Message event

        // Message event table
        private const string MESSAGE_EVENT_TABLE = "message_event";
        private const string ME_ID = "id";
        private const string ME_EVENT_CODE = "event_code";
        private const string ME_SCENARIO_ID = "scenario_id";
        private const string ME_TYPE = "type";
        private const string ME_CONTENT = "content";
        private const string ME_RECIPIENT = "recipient";

        private readonly string CREATE_MESSAGE_EVENT_TABLE =
            $"CREATE TABLE IF NOT EXISTS {MESSAGE_EVENT_TABLE} " +
            $"({ME_ID} INTEGER PRIMARY KEY, " +
            $"{ME_EVENT_CODE} INTEGER, " +
            $"{ME_SCENARIO_ID} INTEGER, " +
            $"{ME_TYPE} TEXT, " +
            $"{ME_CONTENT} TEXT, " +
            $"{ME_RECIPIENT} TEXT)";

        /// <summary>
        /// Add a message event in the DB
        /// </summary>
        /// <param name="m">Message event to add in the DB</param>
        /// <returns>The primary key of the element if it worked, else -1</returns>
        public int PutMessageEvent(MessageEvent m)
        {
            int nextPrimaryKey = GetNextPrimaryKey(MESSAGE_EVENT_TABLE);

            string query = "INSERT INTO " + MESSAGE_EVENT_TABLE
                + " ( "
                + ME_ID + ", "
                + ME_EVENT_CODE + ", "
                + ME_SCENARIO_ID + ", "
                + ME_TYPE + ", "
                + ME_CONTENT + ", "
                + ME_RECIPIENT + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + m.eventCode + "', '"
                + m.scenario + "', '"
                + m.type + "', '"
                + m.content + "', '"
                + m.recipient + "' )";

            if (!Put(query)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get a message event from the DB based on its id 
        /// </summary>
        /// <param name="id">Message event id</param>
        /// <returns>The message event if found, else null</returns>
        public MessageEvent GetMessageEventByID(int id)
        {
            string query = $"SELECT * FROM {MESSAGE_EVENT_TABLE} WHERE {ME_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get message event with id " + id);
                return null;
            }

            MessageEvent m = new MessageEvent(
                 scenario: Convert.ToInt32(reader[2]),
                 type: reader[3].ToString(),
                 content: reader[4].ToString(),
                 recipient: reader[5].ToString()
                 );

            m.SetID(Convert.ToInt32(reader[0]));
            m.SetEventCode(Convert.ToByte(reader[1]));

            return m;
        }

        /// <summary>
        /// Get all the message event of a scenario 
        /// </summary>
        /// <param name="scenarioID">Scenario id</param>
        /// <returns>A list of message event if found, else null</returns>
        public List<MessageEvent> GetAllMessageEventScenario(int scenarioID)
        {
            string query = $"SELECT * FROM {MESSAGE_EVENT_TABLE} WHERE {ME_SCENARIO_ID}={scenarioID}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get all message event with scenario id " + scenarioID);
                return null;
            }

            List<MessageEvent> events = new List<MessageEvent>();

            while (reader.Read())
            {
                MessageEvent m = new MessageEvent(
                    scenario: Convert.ToInt32(reader[2]),
                    type: reader[3].ToString(),
                    content: reader[4].ToString(),
                    recipient: reader[5].ToString()
                    );

                m.SetID(Convert.ToInt32(reader[0]));
                m.SetEventCode(Convert.ToByte(reader[1]));

                events.Add(m);
            }

            return events;
        }

        /// <summary>
        /// Update a message event entry
        /// </summary>
        /// <param name="m">The message event object</param>
        /// <returns>True if it worked, else False</returns>
        public bool UpdateMessageEvent(MessageEvent m)
        {
            string query = "UPDATE " + MESSAGE_EVENT_TABLE
                + " SET "
                + ME_EVENT_CODE + " = '" + m.eventCode + "', "
                + ME_SCENARIO_ID + " = '" + m.scenario + "', "
                + ME_TYPE + " = '" + m.type + "', "
                + ME_CONTENT + " = '" + m.content + "', "
                + ME_RECIPIENT + " = '" + m.recipient + "' "

                + "WHERE " + ME_ID + " = " + m.id;

            return Put(query);
        }

        /// <summary>
        /// Delete a message event entry from the DB
        /// </summary>
        /// <param name="id">ID of the message event in the DB</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteMessageEventByID(int id)
        {
            return DeleteEntryByID(MESSAGE_EVENT_TABLE, id);
        }

#endregion

#region Simulation

        // Simulation table
        private const string SIMULATION_TABLE = "simulation";
        private const string SIM_ID = "id";
        private const string SIM_REMOTE_ID = "remote_id";
        private const string SIM_NAME = "name";
        private const string SIM_START_TIME = "start_time";
        private const string SIM_END_TIME = "end_time";
        private const string SIM_DURATION = "duration";
        private const string SIM_VIDEO_DEBRIEF = "video_debrief";
        private const string SIM_PDF_DEBRIEF = "pdf_debrief";
        private const string SIM_SCENARIO_ID = "scenario_id";

        private readonly string CREATE_SIMULATION_TABLE =
            $"CREATE TABLE IF NOT EXISTS {SIMULATION_TABLE} " +
            $"({SIM_ID} INTEGER PRIMARY KEY, " +
            $"{SIM_REMOTE_ID} INTEGER, " +
            $"{SIM_NAME} TEXT, " +
            $"{SIM_START_TIME} TEXT, " +
            $"{SIM_END_TIME} TEXT, " +
            $"{SIM_DURATION} TEXT, " +
            $"{SIM_VIDEO_DEBRIEF} TEXT, " +
            $"{SIM_PDF_DEBRIEF} TEXT, " +
            $"{SIM_SCENARIO_ID} INTEGER)";

        /// <summary>
        /// Add a simulation in the DB
        /// </summary>
        /// <param name="s">Simulation object to add</param>
        /// <returns>The primary key of the entry if it worked, else -1</returns>
        public int PutSimulation(Simulation s)
        {
            int nextPrimaryKey = GetNextPrimaryKey(SIMULATION_TABLE);

            string query = "INSERT INTO " + SIMULATION_TABLE
                + " ( "
                + SIM_ID + ", "
                + SIM_REMOTE_ID + ", "
                + SIM_NAME + ", "
                + SIM_START_TIME + ", "
                + SIM_END_TIME + ", "
                + SIM_DURATION + ", "
                + SIM_VIDEO_DEBRIEF + ", "
                + SIM_PDF_DEBRIEF + ", "
                + SIM_SCENARIO_ID + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + s.remoteID + "', '"
                + s.GetName() + "', '"
                + s.startTime + "', '"
                + s.endTime + "', '"
                + s.duration + "', '"
                + s.videoDebrief + "', '"
                + s.pdfDebrief + "', '"
                + s.scenario.id + "' )";

            if (!Put(query)) { return -1; }

            s.GetParticipants().ForEach(p =>
            {
                p.SetSimulation(nextPrimaryKey);
                PutParticipant(p);
            });

            s.listComments.ForEach(c =>
            {
                c.SetSimulation(nextPrimaryKey);
                PutComment(c);
            });

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get a Simulation based on its primary key in the database
        /// </summary>
        /// <param name="id">Primary key of the entry</param>
        /// <returns>A Simulation object</returns>
        public Simulation GetSimulationByID(int id)
        {
            string query = $"SELECT * FROM {SIMULATION_TABLE} WHERE {SIM_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get simulation with id " + id);
                return null;
            }

            int scenarioID = Convert.ToInt32(reader[8]);

            Simulation s = new Simulation(
               scenario: GetScenarioByID(scenarioID),
               name: reader[2].ToString(),
               cutDuration: GetSettings().cutDuration
            );
            s.SetID(id);
            s.SetRemoteID(reader[1].ToString());
            s.SetStartTime(Convert.ToDateTime(reader[3]));
            s.SetEndTime(Convert.ToDateTime(reader[4]));
            s.SetDuration(TimeSpan.Parse(reader[5].ToString()));
            s.SetVideoDebrief(reader[6].ToString());
            s.SetPdfDebrief(reader[7].ToString());
            s.SetListParticipant(GetAllParticipantSimulation(id));
            s.SetListComment(GetAllCommentSimulation(id));

            return s;
        }

        /// <summary>
        /// Get all the Simulation present in the database
        /// </summary>
        /// <returns>A list with all the simulation object</returns>
        public List<Simulation> GetAllSimulations()
        {
            string query = $"SELECT {SIM_ID} FROM {SIMULATION_TABLE}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - No previous simulations or can't get all simulations ");
                return null;
            }

            List<Simulation> simulations = new List<Simulation>();

            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                simulations.Add(GetSimulationByID(id));
            }

            return simulations;
        }

        /// <summary>
        /// Update a Simulation in the database
        /// </summary>
        /// <param name="s">The Simulation object to update</param>
        /// <returns>True if it worked, else False</returns>
        public bool UpdateSimulation(Simulation s)
        {
            string query = "UPDATE " + SIMULATION_TABLE
                + " SET "
                + SIM_NAME + " = '" + s.GetName() + "', "
                + SIM_START_TIME + " = '" + s.startTime + "', "
                + SIM_END_TIME + " = '" + s.endTime + "', "
                + SIM_DURATION + " = '" + s.duration + "', "
                + SIM_SCENARIO_ID + " = '" + s.scenario.id + "' "

                + "WHERE " + SIM_ID + " = " + s.id;

            return Put(query);
        }

        /// <summary>
        /// Delete a Simulation based on its primary key in the database
        /// </summary>
        /// <param name="id">Primary key of the entry in the database</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteSimulationByID(int id)
        {
            Simulation s = GetSimulationByID(id);
            s.listComments?.ForEach(c => DeleteCommentByID(c.id));
            s.GetParticipants()?.ForEach(p => DeleteParticipantByID(p.id));

            return DeleteEntryByID(SIMULATION_TABLE, id);
        }

#endregion

#region Participant

        // Participant table
        private const string PARTICIPANT_TABLE = "participant";
        private const string P_ID = "id";
        private const string P_NAME = "name";
        private const string P_ROLE = "role";
        private const string P_DEVICE = "device";
        private const string P_SIMULATION_ID = "simulation_id";

        private readonly string CREATE_PARTICIPANT_TABLE =
            $"CREATE TABLE IF NOT EXISTS {PARTICIPANT_TABLE} " +
            $"({P_ID} INTEGER PRIMARY KEY, " +
            $"{P_NAME} TEXT, " +
            $"{P_ROLE} TEXT, " +
            $"{P_DEVICE} TEXT, " +
            $"{P_SIMULATION_ID} INTEGER)";

        /// <summary>
        /// Put a participant object in the database
        /// </summary>
        /// <param name="p">Participant object to insert</param>
        /// <returns>The primary key of the entry in the database if it worked, else -1</returns>
        public int PutParticipant(Participant p)
        {
            int nextPrimaryKey = GetNextPrimaryKey(PARTICIPANT_TABLE);

            string query = "INSERT INTO " + PARTICIPANT_TABLE
                + " ( "
                + P_ID + ", "
                + P_NAME + ", "
                + P_ROLE + ", "
                + P_DEVICE + ", "
                + P_SIMULATION_ID + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + p.GetName() + "', '"
                + p.role + "', '"
                + p.GetIp() + "', '"
                + p.simulation + "' )";

            if (!Put(query)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get a participant based on its primary key in the database
        /// </summary>
        /// <param name="id">Primary key of the participant in the database</param>
        /// <returns>A participant object</returns>
        public Participant GetParticipantByID(int id)
        {
            string query = $"SELECT * FROM {PARTICIPANT_TABLE} WHERE {P_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get participant with id " + id);
                return null;
            }


            int simulationID = Convert.ToInt32(reader[4]);

            Participant p = new Participant(
                name: reader[1].ToString(),
                role: reader[2].ToString(),
                ip: reader[3].ToString()
            );
            p.SetID(id);
            p.SetSimulation(simulationID);

            return p;
        }

        /// <summary>
        /// Update a participant
        /// </summary>
        /// <param name="p">Participant object to update</param>
        /// <returns>True if it worked, else False</returns>
        public bool UpdateParticipant(Participant p)
        {
            string query = "UPDATE " + SIMULATION_TABLE
                + " SET "
                + P_NAME + " = '" + p.GetName() + "', "
                + P_ROLE + " = '" + p.role + "', "
                + P_DEVICE + " = '" + p.GetIp() + "', "
                + P_SIMULATION_ID + " = '" + p.simulation + "' "

                + "WHERE " + P_ID + " = " + p.id;

            return Put(query);
        }

        /// <summary>
        /// Delete a participant based on its primary key
        /// </summary>
        /// <param name="id">Primary key of the participant in the database</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteParticipantByID(int id)
        {
            return DeleteEntryByID(PARTICIPANT_TABLE, id);
        }

        /// <summary>
        /// Get all the participant of a simulation based on a simulation
        /// </summary>
        /// <param name="simulationID">Primary key of the simulation</param>
        /// <returns>A list with all the participant objects</returns>
        public List<Participant> GetAllParticipantSimulation(int simulationID)
        {
            string query = $"SELECT * FROM {PARTICIPANT_TABLE} WHERE {P_SIMULATION_ID}={simulationID}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get all participant with simulation id " + simulationID);
                return null;
            }

            List<Participant> participants = new List<Participant>();

            while (reader.Read())
            {
                Participant p = new Participant(
                    name: reader[1].ToString(),
                    role: reader[2].ToString(),
                    ip: reader[3].ToString()
                );
                p.SetID(Convert.ToInt32(reader[0]));
                p.SetSimulation(Convert.ToInt32(reader[4]));

                participants.Add(p);
            }

            return participants;
        }

#endregion

#region Comment

        // Comment table
        private const string COMMENT_TABLE = "comment";
        private const string COM_ID = "id";
        private const string COM_CONTENT = "content";
        private const string COM_TIME_IN_VIDEO = "time_in_video";
        private const string COM_THUMBNAIL = "[thumbnail]";
        private const string COM_SIMULATION_ID = "simulation_id";

        private readonly string CREATE_COMMENT_TABLE =
            $"CREATE TABLE IF NOT EXISTS {COMMENT_TABLE} " +
            $"({COM_ID} INTEGER PRIMARY KEY, " +
            $"{COM_CONTENT} TEXT, " +
            $"{COM_TIME_IN_VIDEO} DOUBLE, " +
            // varbinary(max) doesn't work, so this is the alternative
            $"{COM_THUMBNAIL} varbinary(2147483647), " +
            $"{COM_SIMULATION_ID} INTEGER)";

        /// <summary>
        /// Add a comment in the database
        /// </summary>
        /// <param name="c">Comment object to add</param>
        /// <returns>The primary key of the entry if it worked, else -1</returns>
        public int PutComment(Comment c)
        {
            int nextPrimaryKey = GetNextPrimaryKey(COMMENT_TABLE);

            string query = "INSERT INTO " + COMMENT_TABLE
                + " ( "
                + COM_ID + ", "
                + COM_CONTENT + ", "
                + COM_TIME_IN_VIDEO + ", "
                + COM_THUMBNAIL + ", "
                + COM_SIMULATION_ID + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + c.GetContent() + "', '"
                + c.GetTimeInSimulation() + "', "
                + "@ImageData , '"
                + c.simulation + "' )";

            List<SqliteParameter> parameters = new List<SqliteParameter>();
            var thumbnailData = new SqliteParameter("@ImageData", c.Thumbnail);
            thumbnailData.DbType = DbType.Binary;
            parameters.Add(thumbnailData);

            if (!Put(query, parameters)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get all the comments of a simulation
        /// </summary>
        /// <param name="simulationID">Primary key of the simulation</param>
        /// <returns>A list with all the comment object of the simulation</returns>
        public List<Comment> GetAllCommentSimulation(int simulationID)
        {
            string query = $"SELECT * FROM {COMMENT_TABLE} WHERE {COM_SIMULATION_ID}={simulationID}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - No avaible comments or can't get all comments with simulation id " + simulationID);
                return null;
            }

            List<Comment> comments = new List<Comment>();

            while (reader.Read())
            {
                Comment c = new Comment(
                    content: reader[1].ToString(),
                    timeInSimulation_ms: (double)reader[2]);
                c.SetID(Convert.ToInt32(reader[0]));
                c.SetSimulation(Convert.ToInt32(reader[4]));
                // Read the byte array
                c.Thumbnail = (byte[])reader[3];

                comments.Add(c);
            }

            return comments;
        }

        /// <summary>
        /// Delete a comment based on its primary key
        /// </summary>
        /// <param name="id">Primary key of the comment</param>
        /// <returns>True if it worked, else False</returns>
        public bool DeleteCommentByID(int id)
        {
            return DeleteEntryByID(COMMENT_TABLE, id);
        }

#endregion

#region Settings

        // Settings table
        private const string SETTINGS_TABLE = "settings";
        private const string SETT_ID = "id";
        private const string SETT_CUT_DURATION = "cut_duration";
        private const string SETT_MAIN_VIDEO_URL = "main_video_url";
        private const string SETT_MAIN_SRV_IP = "main_srv_ip";

        private readonly string CREATE_SETTINGS_TABLE =
            $"CREATE TABLE IF NOT EXISTS {SETTINGS_TABLE} " +
            $"({SETT_ID} INTEGER PRIMARY KEY, " +
            $"{SETT_CUT_DURATION} TEXT)";

        /// <summary>
        /// Add settings in the database
        /// </summary>
        /// <param name="s">Settings object to add</param>
        /// <returns>Primary key of the entry if it worked, else -1</returns>
        public int PutSettings(Settings s)
        {
            int nextPrimaryKey = GetNextPrimaryKey(SETTINGS_TABLE);

            string query = "INSERT INTO " + SETTINGS_TABLE
                + " ( "
                + SETT_ID + ", "
                + SETT_CUT_DURATION + " ) "

                + "VALUES ( '"
                + nextPrimaryKey + "', '"
                + s.cutDuration + "' )";

            if (!Put(query)) { return -1; }

            return nextPrimaryKey;
        }

        /// <summary>
        /// Get the Settings
        /// </summary>
        /// <returns>A Settings object</returns>
        public Settings GetSettings()
        {
            int id = GetNextPrimaryKey(SETTINGS_TABLE) - 1;
            string query = $"SELECT * FROM {SETTINGS_TABLE} WHERE {SETT_ID}={id}";
            IDataReader reader = Get(query);

            if (reader[0] == DBNull.Value)
            {
                Debug.LogWarning("[DB] - Can't get settings or no settings set");
                return null;
            }

            Settings s = new Settings(
                cutDuration: reader[1].ToString()
            );

            return s;
        }

        /// <summary>
        /// Update the settings
        /// </summary>
        /// <param name="s">Settings object to update</param>
        /// <returns>True if it worked, else False</returns>
        public bool UpdateSettings(Settings s)
        {
            int id = GetNextPrimaryKey(SETTINGS_TABLE) - 1;

            string query = $"UPDATE {SETTINGS_TABLE} SET "
                           + $"{SETT_CUT_DURATION}='{s.cutDuration}' "
                           + $"WHERE id='{id}'";

            if (!Put(query)) { return false; }

            return true;
        }
#endregion

        /// <summary>
        /// Callback used when the app shuts down
        /// </summary>
        public void OnApplicationQuit()
        {
            CloseDB();
        }

    }

}