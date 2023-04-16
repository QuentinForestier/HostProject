using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnStart : MonoBehaviour
{
    public string SceneName = "";

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(SceneName);
    }
}
