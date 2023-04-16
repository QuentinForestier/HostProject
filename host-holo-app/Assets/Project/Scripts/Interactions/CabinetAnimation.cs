using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinetAnimation : MonoBehaviour
{
    public GameObject CabinetDoor;

    public GameObject InsideContent;

    public void Start()
    {
        InsideContent.SetActive(false);
    }

    public void OpenDoor()
    {
        CabinetDoor.transform.DOLocalRotate(new Vector3(0f, 0f, -170f), 1.5f);

        // Show the content (was hiddent to prevent cheating by peaking inside)
        InsideContent.SetActive(true);
    }

    public void CloseDoor()
    {
        CabinetDoor.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 1.5f);
    }
}
