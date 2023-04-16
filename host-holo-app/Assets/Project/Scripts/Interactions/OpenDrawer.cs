using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDrawer : MonoBehaviour
{
    public GameObject Drawer;

    public GameObject InsideContent;

    public void OpenDoor()
    {
        Drawer.transform.DOLocalMoveZ(-0.502f, 1.5f);
        
        // Show the content (was hiddent to prevent cheating by peaking inside)
        InsideContent.SetActive(true);
    }

    public void CloseDoor()
    {
        Drawer.transform.DOLocalMoveZ(-0.258f, 1.5f);
    }
}
