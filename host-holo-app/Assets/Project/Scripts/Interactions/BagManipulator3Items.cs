using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagManipulator3Items : MonoBehaviour
{
    [Title("Parameters")]

    public bool IsOpen = false;

    [Title("References")]

    public GameObject BagContent;

    public GameObject[] Items;

    public float[] ItemScale;

    private int _inspectedItem = 0;

    public void Start()
    {
        UpdateItemsPosition();
        BagContent.SetActive(IsOpen);
    }

    public void OnBagOpen()
    {
        BagContent.SetActive(true);
    }

    public void OnBagClose()
    {
        BagContent.SetActive(false);
    }

    public void OnInspectNext()
    {
        _inspectedItem += 1;
        _inspectedItem %= 3;

        UpdateItemsPosition();
    }

    public void OnInspectPrevious()
    {
        _inspectedItem += 2;
        _inspectedItem %= 3;

        UpdateItemsPosition();
    }

    private void UpdateItemsPosition()
    {
        // Center item
        Items[_inspectedItem].transform.localScale = new Vector3(ItemScale[_inspectedItem], ItemScale[_inspectedItem], ItemScale[_inspectedItem]);
        Items[_inspectedItem].transform.localPosition = Vector3.zero;

        // Left item
        int leftItem = (_inspectedItem + 1) % 3;
        Items[leftItem].transform.localScale = new Vector3(1f, 1f, 1f);
        Items[leftItem].transform.localPosition = new Vector3(-0.3f, 0f, 0f);

        // Right item
        int rightItem = (_inspectedItem + 2) % 3;
        Items[rightItem].transform.localScale = new Vector3(1f, 1f, 1f);
        Items[rightItem].transform.localPosition = new Vector3(0.3f, 0f, 0f);
    }
}
