using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private CanvasGroup cg;

    [Header("Choose base button alpha")]
    public float unHoveredAlpha;

    private const float hovered = 1.0f;
    private const float duration = 0.1f;
 

    private void Start()
    {
        cg = this.transform.GetChild(0).GetComponent<CanvasGroup>();
        cg.alpha = unHoveredAlpha;
    }

    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        FadeCanvasGroup.fadeCouroutine(cg, cg.alpha, hovered, duration);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        FadeCanvasGroup.fadeCouroutine(cg, cg.alpha, unHoveredAlpha, duration);
    }
}
