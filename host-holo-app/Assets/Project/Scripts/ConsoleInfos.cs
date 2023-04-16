using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleInfos : MonoBehaviour
{
    [Title("References")]

    public GameObject VisibleElements;

    public TMPro.TextMeshProUGUI Text;

    public void SetVisible(bool visible)
    {
        VisibleElements.SetActive(visible);
        Text.gameObject.SetActive(visible);
    }

    public void ShowMessage(string text)
    {
        Text.text = text;
    }     
}
