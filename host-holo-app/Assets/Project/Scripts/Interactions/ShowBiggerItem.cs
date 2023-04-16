using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBiggerItem : MonoBehaviour
{
    public GameObject BiggerItem;

    public bool IsItemVisible = false;

    public void Start()
    {
        BiggerItem.SetActive(IsItemVisible);
    }

    public void InvisibleTriggerDetected()
    {
        IsItemVisible = true;
        BiggerItem.SetActive(true);
    }

    public void CloseButtonClick()
    {
        IsItemVisible = false;
        BiggerItem.SetActive(false);
    }
}
