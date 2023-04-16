using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ButtonStatusController : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    private Color textOriginalColor;
    private Color iconOriginalColor;
    private Renderer iconRenderer;
    private List<MonoBehaviour> buttonBehaviours;
    private Transform buttonHighLightComponent;
    private bool isInitialized = false;


    private void Awake()
    {
        if (!isInitialized)
        {
            isInitialized = true;

            var iconParent = transform.Find("IconAndText");
            textMeshPro = iconParent.GetComponentInChildren<TextMeshPro>();
            iconRenderer = iconParent.Find("UIButtonSquareIcon").
                gameObject.GetComponent<Renderer>();
            buttonHighLightComponent =
                transform.Find("CompressableButtonVisuals");
            buttonBehaviours = GetComponents<MonoBehaviour>().ToList();
            textOriginalColor = textMeshPro.color;
            iconOriginalColor = iconRenderer.material.color;
        }
    }

    public void SetStatus(bool active)
    {
        foreach (var b in buttonBehaviours.Where(p => p != this))
        {
            b.enabled = active;
        }
        buttonHighLightComponent.gameObject.SetActive(active);
        textMeshPro.color = active ? textOriginalColor : Color.gray;
        iconRenderer.material.color = active ? iconOriginalColor : Color.gray;
    }
}
