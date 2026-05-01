using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public parse parser;
    public Material checkpointMaterial;

    // 30 feet converted to meters (30 * 0.3048 = 9.144m)
    public float reachRadius = 9.144f;

    private List<Vector3> checkpointPositions;
    private List<GameObject> drawnCheckpoints = new List<GameObject>();
    private int currentTargetIndex = 0; // Keeping the naming convention from earlier

    void Start()
    {
        if (parser != null)
        {
            checkpointPositions = parser.ParseFile();
            DrawCheckpoints();
        }
    }

    void DrawCheckpoints()
    {
        foreach (Vector3 pos in checkpointPositions)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pos;

            float diameter = reachRadius * 2;
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);

            if (checkpointMaterial != null)
            {
                sphere.GetComponent<Renderer>().material = checkpointMaterial;
            }

            Destroy(sphere.GetComponent<SphereCollider>());
            drawnCheckpoints.Add(sphere);
        }

        HighlightCheckpoint(0, true);
    }

    void HighlightCheckpoint(int index, bool active)
    {
        if (index >= 0 && index < drawnCheckpoints.Count)
        {
            var renderer = drawnCheckpoints[index].GetComponent<Renderer>();
            if (active)
            {
                renderer.material.color = new Color(0, 1, 0, 0.5f); // Green for target
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 0.3f); // Default translucent
            }
        }
    }

    void Update()
    {
        if (checkpointPositions == null || checkpointPositions.Count == 0) return;

        // Measure the distance directly from this object (the XR Origin)
        Vector3 targetPos = checkpointPositions[currentTargetIndex];
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= reachRadius)
        {
            Debug.Log($"Checkpoint {currentTargetIndex + 1} reached!");

            drawnCheckpoints[currentTargetIndex].SetActive(false);
            currentTargetIndex++;

            if (currentTargetIndex < checkpointPositions.Count)
            {
                HighlightCheckpoint(currentTargetIndex, true);
            }
            else
            {
                Debug.Log("Training track finished!");
            }
        }
    }
}