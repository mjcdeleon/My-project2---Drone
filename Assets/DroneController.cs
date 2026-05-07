using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Flight Settings")]
    public float maxMoveSpeed = 8.0f;
    public float maxAltitudeSpeed = 4.0f;
    public float yawSpeed = 90.0f; // degrees per second

    [Header("Hand Tracking")]
    public XRHandSubsystem handSubsystem;

    [Header("Pinch Settings")]
    public float pinchOpenDistance = 0.08f;  // fully open = 0 speed
    public float pinchClosedDistance = 0.01f; // fully closed = max speed

    [Header("Tilt Sensitivity")]
    public float tiltDeadzone = 0.1f;   // ignore tiny tilts (-1 to 1 range)
    public float tiltMaxAngle = 0.6f;   // how far you tilt for full effect (-1 to 1 range)

    private Rigidbody rb;
    private Vector3 lastCheckpointPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        lastCheckpointPos = transform.position;

        List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0) handSubsystem = subsystems[0];
    }

    void FixedUpdate()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        var leftHand = handSubsystem.leftHand;
        var rightHand = handSubsystem.rightHand;

        if (!leftHand.isTracked || !rightHand.isTracked)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // -------------------------------------------------------
        // RIGHT HAND: Pinch = forward speed throttle
        // -------------------------------------------------------
        float forwardSpeed = 0f;

        var rThumb = rightHand.GetJoint(XRHandJointID.ThumbTip);
        var rIndex = rightHand.GetJoint(XRHandJointID.IndexTip);

        if (rThumb.TryGetPose(out Pose rtPose) && rIndex.TryGetPose(out Pose riPose))
        {
            float pinchDist = Vector3.Distance(rtPose.position, riPose.position);
            // InverseLerp: open hand = 0, closed pinch = 1
            float throttle = Mathf.InverseLerp(pinchOpenDistance, pinchClosedDistance, pinchDist);
            throttle = Mathf.Clamp01(throttle);
            forwardSpeed = throttle * maxMoveSpeed;
        }

        // -------------------------------------------------------
        // LEFT HAND: Palm orientation = altitude + yaw
        //
        // We derive the palm normal from:
        //   Wrist -> Index Knuckle  (points "forward" along hand)
        //   Wrist -> Pinky Knuckle  (points across the hand)
        // Cross product of these two = palm normal (points out of palm)
        // -------------------------------------------------------
        float altitudeSpeed = 0f;
        float yawDelta = 0f;

        var lWrist = leftHand.GetJoint(XRHandJointID.Wrist);
        var lIndexKnuck = leftHand.GetJoint(XRHandJointID.IndexProximal);
        var lPinkyKnuck = leftHand.GetJoint(XRHandJointID.LittleProximal);

        if (lWrist.TryGetPose(out Pose lwPose) &&
            lIndexKnuck.TryGetPose(out Pose liPose) &&
            lPinkyKnuck.TryGetPose(out Pose lpPose))
        {
            // Build palm normal from joint positions (no reliance on joint rotation)
            Vector3 toIndex = (liPose.position - lwPose.position).normalized;
            Vector3 toPinky = (lpPose.position - lwPose.position).normalized;

            // Cross product gives us the vector pointing out of the palm
            // (order matters: for left hand this gives upward-facing normal when palm faces up)
            Vector3 palmNormal = Vector3.Cross(toIndex, toPinky).normalized;

            // palmNormal.y:  +1 = palm facing up,  -1 = palm facing down
            // Use this to control altitude
            float verticalTilt = Mathf.Clamp(palmNormal.y, -1f, 1f);
            verticalTilt = ApplyDeadzone(verticalTilt, tiltDeadzone, tiltMaxAngle);
            altitudeSpeed = verticalTilt * maxAltitudeSpeed;

            // palmNormal.x:  controls yaw (left/right lean of palm)
            // +1 = palm tilted right,  -1 = palm tilted left
            float horizontalTilt = Mathf.Clamp(palmNormal.x, -1f, 1f);
            horizontalTilt = ApplyDeadzone(horizontalTilt, tiltDeadzone, tiltMaxAngle);
            yawDelta = horizontalTilt * yawSpeed * Time.fixedDeltaTime;
        }

        // -------------------------------------------------------
        // APPLY ROTATION (yaw only, drone stays level)
        // -------------------------------------------------------
        if (Mathf.Abs(yawDelta) > 0f)
        {
            Quaternion yawRotation = Quaternion.Euler(0f, yawDelta, 0f);
            rb.MoveRotation(rb.rotation * yawRotation);
        }

        // -------------------------------------------------------
        // APPLY VELOCITY
        // Forward is the drone's current facing, altitude is world Y
        // -------------------------------------------------------
        Vector3 forwardVelocity = transform.forward * forwardSpeed;
        Vector3 altitudeVelocity = Vector3.up * altitudeSpeed;
        rb.linearVelocity = forwardVelocity + altitudeVelocity;

        Debug.Log($"Throttle speed: {forwardSpeed:F2} | Altitude: {altitudeSpeed:F2} | Yaw: {yawDelta:F2}");
    }

    // Remaps a value so small tilts are ignored and output is 0-1
    float ApplyDeadzone(float value, float deadzone, float maxRange)
    {
        float abs = Mathf.Abs(value);
        if (abs < deadzone) return 0f;
        float remapped = Mathf.InverseLerp(deadzone, maxRange, abs);
        return remapped * Mathf.Sign(value);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.isTrigger) Respawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint")) lastCheckpointPos = other.transform.position;
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = lastCheckpointPos + Vector3.up * 1.5f;
        transform.position = rb.position;
    }
}