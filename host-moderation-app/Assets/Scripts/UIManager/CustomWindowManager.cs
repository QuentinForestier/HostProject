using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomWindowManager : MonoBehaviour
{
    [Serializable]
    public class _Window
    {
        public string name;
        public GameObject buttonObject;
        public GameObject windowObject;
    }

    [Header("Window Elements")]
    public List<_Window> windows = new List<_Window>();

    [Header("Select Window")]
    public int preselected;

    private _Window previousSelected;

    private float duration = 0.4f;

    private const float unSelected = 0.0f;
    private const float selected = 1.0f;
    private const float unHovered = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // Verify if the preselected window exist
        if (preselected > windows.Count)
        {
            preselected = 0;
            Debug.LogError("Preselected window was invalidate and set to default value 0");
        }

        // add the listener to all the buttons
        int count = 0;
        foreach (_Window window in windows)
        {
            if (count == preselected)
            {
                // preselect logically and graphically the preselected window
                previousSelected = window;

                CanvasGroup cg = window.windowObject.GetComponent<CanvasGroup>();
                cg.alpha = selected;
                cg.interactable = true;
                cg.blocksRaycasts = true;

                window.buttonObject.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = unSelected;
                window.buttonObject.transform.GetChild(1).GetComponent<CanvasGroup>().alpha = selected;
            }
            else
            {
                changeWindowState(window.windowObject.GetComponent<CanvasGroup>(), false);
            }

            int index = count;
            window.buttonObject.GetComponent<Button>().onClick.AddListener(delegate { OnClickListener(index); });
            count += 1;
        }
    }

    void OnClickListener(int whichButton)
    {
        _Window selected = windows[whichButton];

        // do nothing if the user select the same one twice or more
        if (!selected.Equals(previousSelected))
        {
            // fade out previous window
            changeWindowState(previousSelected.windowObject.GetComponent<CanvasGroup>(), false);

            // fade out previous button
            changeButtonState(
                previousSelected.buttonObject.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>(),
                previousSelected.buttonObject.transform.GetChild(1).gameObject.GetComponent<CanvasGroup>(),
                true);

            // fade in selected window
            changeWindowState(selected.windowObject.GetComponent<CanvasGroup>(), true);

            // fade in selected button
            changeButtonState(
                selected.buttonObject.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>(),
                selected.buttonObject.transform.GetChild(1).gameObject.GetComponent<CanvasGroup>(),
                false);

            previousSelected = selected;
        }
    }

    void changeWindowState(CanvasGroup cg, bool state)
    {
        if (state)
        {
            FadeCanvasGroup.fadeCouroutine(cg, cg.alpha, selected, duration);
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.alpha = unSelected;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    void changeButtonState(CanvasGroup normal, CanvasGroup pressed, bool state)
    {
        if (state)
        {
            FadeCanvasGroup.fadeCouroutine(normal, normal.alpha, unHovered, duration);
            pressed.alpha = unSelected;
        }
        else
        {
            FadeCanvasGroup.fadeCouroutine(pressed, pressed.alpha, selected, duration);
            normal.alpha = unSelected;
        }
    }

    public IEnumerator fade(CanvasGroup cg, float start, float end)
    {

        float counter = 0.0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }
    }

    public void OnPointerEnter(int whichButton)
    {
        Debug.Log("enter : " + whichButton);
    }

    public void OnPointerExit(int whichButton)
    {
        Debug.Log("exit : " + whichButton);
    }

    // Update is called once per frame
    void Update()
    {
        /*foreach (_Window window in windows)
        {
            if (!window.Equals(previousSelected))
            {
                CanvasGroup cgw = window.windowObject.GetComponent<CanvasGroup>();
                cgw.alpha = unSelected;
                cgw.interactable = true;
                cgw.blocksRaycasts = true;

                window.buttonObject.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>().alpha = selected;
                window.buttonObject.transform.GetChild(1).gameObject.GetComponent<CanvasGroup>().alpha = unSelected;
            }
        }*/
    }
}
