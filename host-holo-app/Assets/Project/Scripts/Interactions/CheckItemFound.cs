using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckItemFound : MonoBehaviour
{
    public GameObject WantedObject;

    public Material ItemFoundMaterial;

    public UnityEvent ItemFound;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision Enter");
        if(other.gameObject == WantedObject)
        {
            WantedObject.SetActive(false);

            this.GetComponent<MeshRenderer>().material = ItemFoundMaterial;

            ItemFound.Invoke();
        }
    }
}
