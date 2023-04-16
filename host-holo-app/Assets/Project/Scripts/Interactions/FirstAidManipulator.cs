using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FirstAidManipulator : MonoBehaviour
{
    public GameObject CabinetDoor;

    public GameObject InsideContent;

    public ShowBiggerItem ShowBiggerItem;

    public void Start()
    {
        InsideContent.SetActive(false);
    }

    public void OpenCabinet()
    {

        // The cabinet has been unlocked, the main lock will play an animation and silently close, whe should the follow by opening the cabinet
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            // Hide the bigger item and disable it
            ShowBiggerItem.CloseButtonClick();
            ShowBiggerItem.gameObject.SetActive(false);
        });
        sequence.AppendInterval(0.5f);
        sequence.Append(CabinetDoor.transform.DOLocalRotate(new Vector3(0f, 0f, -170f), 1.5f));

        // Show the content (was hiddent to prevent cheating by peaking inside)
        InsideContent.SetActive(true);
    }
}
