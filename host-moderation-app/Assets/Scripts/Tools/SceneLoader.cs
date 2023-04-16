using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Host.Toolbox
{
    public class SceneLoader : MonoBehaviour
    {
        public static string previousScene;

        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}

