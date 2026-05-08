using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    public float maxMoveSpeed = 8.0f;
    public XRHandSubsystem handSubsystem;

    private Rigidbody rb;
    private Vector3 lastCheckpointPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastCheckpointPos = transform.position;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 4f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var subsystems = new List<XRHandSubsystem>();
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

        Vector3 flyDir = Vector3.zero;

        var indexTip = rightHand.GetJoint(XRHandJointID.IndexTip);
        var middleTip = rightHand.GetJoint(XRHandJointID.MiddleTip);

        if (indexTip.TryGetPose(out Pose iPose) && middleTip.TryGetPose(out Pose mPose))
        {
            var wristJoint = rightHand.GetJoint(XRHandJointID.Wrist);
            if (wristJoint.TryGetPose(out Pose wPose))
            {
                Vector3 fingerMidpoint = (iPose.position + mPose.position) * 0.5f;
                flyDir = -(fingerMidpoint - wPose.position).normalized;
                flyDir.y = -flyDir.y;
            }
        }

        float speed = 0f;
        var leftThumb = leftHand.GetJoint(XRHandJointID.ThumbTip);
        var leftIndex = leftHand.GetJoint(XRHandJointID.IndexTip);

        if (leftThumb.TryGetPose(out Pose tPose) && leftIndex.TryGetPose(out Pose lPose))
        {
            float dist = Vector3.Distance(tPose.position, lPose.position);
            speed = Mathf.Clamp01(1f - (dist / 0.05f)) * maxMoveSpeed;
        }

        if (speed > 0.1f && flyDir != Vector3.zero)
            rb.linearVelocity = flyDir * speed;
        else
            rb.linearVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint")) lastCheckpointPos = other.transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.isTrigger) Respawn();
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Vector3 safeSpot = lastCheckpointPos + Vector3.up * 1.5f;
        rb.position = safeSpot;
        transform.position = safeSpot;
    }
}
