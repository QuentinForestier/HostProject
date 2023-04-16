using UnityEngine;

namespace Host
{
    /// <summary>
    /// Class used to represent a comment as a GameObject and store informations
    /// </summary>
    public class CommentGO : MonoBehaviour
    {
        /// <summary>
        /// ID of the comment on the remote media server
        /// </summary>
        public int id;

        /// <summary>
        /// Content of the comment
        /// </summary>
        public string content;

    }
}


