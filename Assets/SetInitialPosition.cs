using System.Collections.Generic;
using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    public parse parser;
    public Transform xrOrigin; // Assign your XR Origin transform

    void Start()
    {
        if (parser != null)
        {
            List<Vector3> positions = parser.ParseFile();

            if (positions.Count >= 2)
            {
                // Position 0 is the first checkpoint
                Vector3 firstCheckpoint = positions[0];

                // Position 1 is the second checkpoint (used for directional heading)
                Vector3 secondCheckpoint = positions[1];

                // 1. Set Position to the first checkpoint
                xrOrigin.position = firstCheckpoint;

                // 2. Calculate the direction to the second checkpoint
                Vector3 direction = secondCheckpoint - firstCheckpoint;

                // Flatten the Y-axis so the drone remains level with the ground (zero pitch and roll)
                direction.y = 0;

                // 3. Rotate to face the second checkpoint
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    xrOrigin.rotation = targetRotation;
                }
            }
            else
            {
                Debug.LogWarning("Not enough checkpoints parsed to set orientation.");
            }
        }
    }
}
