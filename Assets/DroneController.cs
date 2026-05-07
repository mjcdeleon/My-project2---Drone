using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Flight Settings")]
    public float maxMoveSpeed = 8.0f;
    public float rotationSpeed = 5.0f;

    [Header("Hand Tracking")]
    public XRHandSubsystem handSubsystem;

    [Header("Pinch Settings (Left Hand)")]
    public float pinchOpenDistance = 0.1f;    // Increased for easier detection
    public float pinchClosedDistance = 0.02f;

    private Rigidbody rb;
    private Vector3 lastCheckpointPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Fix the "Kinematic" error automatically
        rb.isKinematic = false;
        rb.useGravity = false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        lastCheckpointPos = transform.position;

        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0) handSubsystem = subsystems[0];
    }

    void FixedUpdate()
    {
        if (handSubsystem == null || !handSubsystem.running) return;

        var leftHand = handSubsystem.leftHand;
        var rightHand = handSubsystem.rightHand;

        if (!leftHand.isTracked || !rightHand.isTracked)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // --- 1. RIGHT HAND: STEERING ---
        var rWrist = rightHand.GetJoint(XRHandJointID.Wrist);
        var rIndex = rightHand.GetJoint(XRHandJointID.IndexProximal);

        if (rWrist.TryGetPose(out Pose rwPose) && rIndex.TryGetPose(out Pose riPose))
        {
            Vector3 pointDir = (riPose.position - rwPose.position).normalized;
            if (pointDir.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(pointDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
            }
        }

        // --- 2. LEFT HAND: SPEED ---
        float currentSpeed = 0f;
        var lThumb = leftHand.GetJoint(XRHandJointID.ThumbTip);
        var lIndex = leftHand.GetJoint(XRHandJointID.IndexTip);

        if (lThumb.TryGetPose(out Pose ltPose) && lIndex.TryGetPose(out Pose liPose))
        {
            float dist = Vector3.Distance(ltPose.position, liPose.position);

            // Map distance to 0-1 throttle
            float throttle = Mathf.InverseLerp(pinchOpenDistance, pinchClosedDistance, dist);
            currentSpeed = Mathf.Clamp01(throttle) * maxMoveSpeed;

            // Debug so you can see if the pinch is working in the console
            if (currentSpeed > 0.1f)
            {
                Debug.Log($"Moving! Speed: {currentSpeed:F2}");
            }
        }

        // --- 3. APPLY MOVEMENT ---
        // Move forward relative to where the drone is currently facing
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Simple bounce/stop on collision
        Debug.Log("Hit something!");
        rb.linearVelocity = Vector3.zero;
    }
}