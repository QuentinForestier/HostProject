using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SwitchStateEvent : UnityEvent<int, bool>
{
}


public class SwitchController : MonoBehaviour
{
    public GameObject OnButton;
    public GameObject OffButton;

    public int Index;

    public bool State = true;

    public SwitchStateEvent StateChanged;
    
    // Start is called before the first frame update
    void Start()
    {
        OnButton.SetActive(State);
        OffButton.SetActive(!State);
    }

    public void SetState(bool state)
    {
        if (state == State)
            return;

        State = state;
        OnButton.SetActive(State);
        OffButton.SetActive(!State);
    }

    public void ToggleState()
    {
        State = !State;
        OnButton.SetActive(State);
        OffButton.SetActive(!State);

        StateChanged.Invoke(Index, State);
    }
}
