using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public float maxMoveSpeed = 8.0f;
    public float turnSpeed = 5.0f;
    public XRHandSubsystem handSubsystem;

    private Rigidbody rb;
    private Vector3 lastCheckpointPos;
    private float pinchThreshold = 0.015f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastCheckpointPos = transform.position;

        // FORCE CORRECT PHYSICS SETTINGS
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 4f;
        // Prevents the drone from "tumbling" when it clips a wall
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // High-precision collision
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

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

        // --- DIRECTION FIX (Wrist to Index Knuckle) ---
        Vector3 worldFlyVec = Vector3.zero;
        var wrist = rightHand.GetJoint(XRHandJointID.Wrist);
        var knuckle = rightHand.GetJoint(XRHandJointID.IndexProximal);

        if (wrist.TryGetPose(out Pose wPose) && knuckle.TryGetPose(out Pose kPose))
        {
            // Pointing vector: where you are aiming your hand
            worldFlyVec = (kPose.position - wPose.position).normalized;

            // ROTATION FIX (No Flipping)
            Vector3 flatLook = new Vector3(worldFlyVec.x, 0, worldFlyVec.z).normalized;
            if (flatLook != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatLook, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * turnSpeed));
            }
        }

        // --- SPEED (Left Hand Pinch) ---
        float speed = 0f;
        var thumb = leftHand.GetJoint(XRHandJointID.ThumbTip);
        var index = leftHand.GetJoint(XRHandJointID.IndexTip);

        if (thumb.TryGetPose(out Pose tPose) && index.TryGetPose(out Pose iPose))
        {
            float dist = Vector3.Distance(tPose.position, iPose.position);
            if (dist < 0.05f)
            {
                float factor = Mathf.InverseLerp(0.05f, pinchThreshold, dist);
                speed = factor * maxMoveSpeed;
            }
        }

        // --- APPLY VELOCITY ---
        if (speed > 0.1f && worldFlyVec != Vector3.zero)
        {
            // By applying velocity, the Mesh Collider on Machu Picchu will STOP you.
            rb.linearVelocity = worldFlyVec * speed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Checkpoints are triggers (ghosts), so we just save the position
        if (other.CompareTag("Checkpoint")) lastCheckpointPos = other.transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solid objects (walls/floors) are NOT triggers.
        // If we hit one, we respawn.
        if (!collision.collider.isTrigger)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Move to last safe spot + 1.5m up
        Vector3 safeSpot = lastCheckpointPos + (Vector3.up * 1.5f);
        rb.position = safeSpot;
        transform.position = safeSpot;
    }
}