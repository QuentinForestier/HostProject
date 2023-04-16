using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BringObjectsPuzzle : MonoBehaviour
{
    public int ItemNeededCount = 3;
    public int ItemFoundCount = 0;

    public UnityEvent ItemsFound;

    public void OnItemFound()
    {
        ItemFoundCount += 1;
        if(ItemFoundCount >= ItemNeededCount)
        {
            ItemsFound.Invoke();
        }    
    }
}
