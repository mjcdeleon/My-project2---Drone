using System.Collections.Generic;
using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    public parse parser;
    public Rigidbody droneRigidbody; // Drag the Drone here

    void Start()
    {
        if (parser != null)
        {
            List<Vector3> positions = parser.ParseFile();

            if (positions != null && positions.Count >= 2)
            {
                // 1. Calculate Spawn (Position 0) with a slight height offset
                Vector3 spawnPos = positions[0] + (Vector3.up * 1.5f);

                // 2. Calculate Direction (Looking at Position 1)
                Vector3 lookAtPos = positions[1];
                Vector3 direction = (lookAtPos - spawnPos).normalized;
                direction.y = 0; // Keep the drone level

                // 3. Teleport via Rigidbody (Safest for Physics)
                droneRigidbody.position = spawnPos;
                droneRigidbody.transform.position = spawnPos;

                if (direction != Vector3.zero)
                {
                    Quaternion spawnRot = Quaternion.LookRotation(direction);
                    droneRigidbody.rotation = spawnRot;
                    droneRigidbody.transform.rotation = spawnRot;
                }

                // 4. Kill any accidental momentum from the scene start
                droneRigidbody.linearVelocity = Vector3.zero;
                droneRigidbody.angularVelocity = Vector3.zero;

                Debug.Log("Drone placed at Start Point and facing first goal.");
            }
        }
    }
}