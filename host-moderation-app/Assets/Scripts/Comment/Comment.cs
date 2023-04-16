using System;
using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class representing a comment
    /// </summary>
    [Serializable]
    public class Comment
    {
        /// <summary>
        /// ID of the comment in the database
        /// </summary>
        public int id { get; private set; }

        /// <summary>
        /// Content of the comment
        /// </summary>
        [SerializeField] private string content;

        /// <summary>
        /// Time where the comment was taken based on the simulation time
        /// </summary>
        [SerializeField] private double timeInSimulation_ms;

        /// <summary>
        /// URL of the comment thumbnail in the media server
        /// </summary>
        public byte[] Thumbnail;

        /// <summary>
        /// ID of the simulation in the database
        /// </summary>
        public int simulation { get; private set; }

        public Comment(string content, double timeInSimulation_ms)
        {
            this.content = content;
            this.timeInSimulation_ms = timeInSimulation_ms;
        }

        /// <summary>
        /// Get the content of the comment
        /// </summary>
        /// <returns>The content of the comment</returns>
        public string GetContent() { return this.content; }

        /// <summary>
        /// Set the content of the comment
        /// </summary>
        /// <param name="content">Content of the comment</param>
        public void SetContent(string content) { this.content = content; }

        /// <summary>
        /// Get the thumbnail URL associated to the comment
        /// </summary>
        /// <returns>URL where we can find the thumbnail of the comment</returns>
        public Texture2D GetThumbnailTexture()
        {
            // Create a texture. Texture size does not matter, since LoadImage will replace with incoming image size.
            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, Thumbnail);

            return tex;
        }

        /// <summary>
        /// Set the thumbnail URL of the comment
        /// </summary>
        /// <param name="thumbnail">Thumbnail URL</param>
        public void SetThumbnail(Texture2D texture)
        {
            Thumbnail = texture.EncodeToPNG();
        }

        /// <summary>
        /// Get the time where the comment was taken
        /// </summary>
        /// <returns>Time where the comment was taken based on the simulation time</returns>
        public double GetTimeInSimulation() { return this.timeInSimulation_ms; }

        /// <summary>
        /// Set the time where the comment was taken
        /// </summary>
        /// <param name="timeInVideo">Time where the comment was taken based on the simulation time</param>
        public void SetTimeInSimulation(double timeInVideo) { this.timeInSimulation_ms = timeInVideo; }

        /// <summary>
        /// Set the database comment ID
        /// </summary>
        /// <param name="id">ID of the comment in the database</param>
        public void SetID(int id) { this.id = id; }

        /// <summary>
        /// Set the ID of the simulation in the database
        /// </summary>
        /// <param name="simulationID">Primary key of the simulation in the database</param>
        public void SetSimulation(int simulationID) { this.simulation = simulationID; }

        /// <summary>
        /// Representation of the comment as a string
        /// </summary>
        /// <returns>A string with the content of the comment</returns>
        public override string ToString()
        {
            return this.content;
        }
    }

}

