using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMagnetism : Solver
{
    [SerializeField]
    [Tooltip("Distance at which to perform the raycast")]
    private float objectDistance = 2f;

    public float ObjectDistance
    {
        get => objectDistance;
        set => objectDistance = value;
    }

    [SerializeField]
    [Tooltip("Maximum raycast distance")]
    private float maxRaycastDistance = 5f;

    public float MaxRaycastDistance { get => maxRaycastDistance; set => maxRaycastDistance = value; }

    [SerializeField]
    [Tooltip("Closest distance to bring object")]
    private float closestDistance = 0.5f;

    /// <summary>
    /// Closest distance to bring object
    /// </summary>
    public float ClosestDistance
    {
        get => closestDistance;
        set => closestDistance = value;
    }

    [SerializeField]
    [Tooltip("Offset from surface along surface normal")]
    private float surfaceNormalOffset = 0.5f;

    /// <summary>
    /// Offset from surface along surface normal
    /// </summary>
    public float SurfaceNormalOffset
    {
        get => surfaceNormalOffset;
        set => surfaceNormalOffset = value;
    }

    [SerializeField]
    [Tooltip("Offset from surface along ray cast direction")]
    private float surfaceRayOffset = 0;

    /// <summary>
    /// Offset from surface along ray cast direction
    /// </summary>
    public float SurfaceRayOffset
    {
        get => surfaceRayOffset;
        set => surfaceRayOffset = value;
    }

    [SerializeField]
    [Tooltip("Array of LayerMask to execute from highest to lowest priority. First layermask to provide a raycast hit will be used by component")]
    private LayerMask[] magneticSurfaces = { UnityEngine.Physics.DefaultRaycastLayers };
    public LayerMask[] MagneticSurfaces
    {
        get => magneticSurfaces;
        set => magneticSurfaces = value;
    }

    /// <summary>
    /// Whether or not the object is currently magnetized to a surface.
    /// </summary>
    public bool OnSurface { get; private set; }

    [SerializeField]
    [Tooltip("If true, ensures object is kept vertical for TrackedTarget, SurfaceNormal, and Blended Orientation Modes")]
    private bool keepOrientationVertical = true;

    /// <summary>
    /// If true, ensures object is kept vertical for TrackedTarget, SurfaceNormal, and Blended Orientation Modes
    /// </summary>
    public bool KeepOrientationVertical
    {
        get => keepOrientationVertical;
        set => keepOrientationVertical = value;
    }

    [SerializeField]
    [Tooltip("If enabled, the debug lines will be drawn in the editor")]
    private bool debugEnabled = false;

    /// <summary>
    /// If enabled, the debug lines will be drawn in the editor
    /// </summary>
    public bool DebugEnabled
    {
        get => debugEnabled;
        set => debugEnabled = value;
    }

    private Vector3 RaycastOrigin => SolverHandler.TransformTarget == null ? Vector3.zero : SolverHandler.TransformTarget.position + objectDistance * SolverHandler.TransformTarget.forward;

    private Vector3 RaycastDirection => Vector3.down;

    private RayStep currentRayStep = new RayStep();

    public override void SolverUpdate()
    {
        // Pass-through by default
        GoalPosition = WorkingPosition;
        GoalRotation = WorkingRotation;

        // Determine raycast params. Update struct to skip instantiation
        Vector3 origin = RaycastOrigin;
        Vector3 endpoint = RaycastOrigin + maxRaycastDistance * RaycastDirection;
        currentRayStep.UpdateRayStep(ref origin, ref endpoint);

        // Skip if there isn't a valid direction
        if (currentRayStep.Direction == Vector3.zero)
        {
            return;
        }

        if (DebugEnabled)
        {
            Debug.DrawLine(currentRayStep.Origin, currentRayStep.Terminus, Color.magenta);
        }

        // Performing the ray cast

        bool isHit;
        RaycastHit result;

        // Do the cast!
        isHit = MixedRealityRaycaster.RaycastSimplePhysicsStep(currentRayStep, maxRaycastDistance, MagneticSurfaces, false, out result);

        OnSurface = isHit;

        // Enforce CloseDistance
        Vector3 hitDelta = result.point - currentRayStep.Origin;
        float length = hitDelta.magnitude;

        if (length < closestDistance)
        {
            result.point = currentRayStep.Origin + currentRayStep.Direction * closestDistance;
        }

        // Apply results
        if (isHit)
        {
            GoalPosition = result.point + surfaceNormalOffset * result.normal + surfaceRayOffset * currentRayStep.Direction;

            Vector3 direction = SolverHandler.TransformTarget.forward;

            if (KeepOrientationVertical)
            {
                direction.y = 0;
            }

            GoalRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
