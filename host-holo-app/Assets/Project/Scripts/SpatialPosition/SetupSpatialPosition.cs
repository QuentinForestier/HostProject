using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupSpatialPosition : MonoBehaviour
{
    public Follow FollowSolver;

    public GroundMagnetism GroundSolver;
    public Interactable LockToggleButton;

    [Header("Pin Button")]

    public Interactable PinButtonInteractable;
    public FollowMeToggle FollowMeToggle;

    void Awake()
    {
        GroundSolver.gameObject.SetActive(false);
    }

    public void StartPlacing()
    {
        GroundSolver.gameObject.SetActive(true);

        // Resume Mesh Observation from all Observers
        CoreServices.SpatialAwarenessSystem.ResumeObservers();

        // Pin Behaviour
        PinButtonInteractable.IsToggled = false;
        FollowMeToggle.SetFollowMeBehavior(true);

        // Make sure the menu toggle is in the right state
        LockToggleButton.IsToggled = false;

        FollowSolver.StartManipulating();
    }

    public void StopPlacing()
    {
        // Set the global position
        /*var globalElements = GlobalElements.Instance();
        globalElements.SetAnchor(VisualCue.transform);
        globalElements.AnchorSetupDone = true;*/

        GroundSolver.gameObject.SetActive(false);

        // Suspend Mesh Observation from all Observers
        CoreServices.SpatialAwarenessSystem.SuspendObservers();

        FollowSolver.EndManipulating();
    }
}
