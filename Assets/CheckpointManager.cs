using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public parse parser;
    public Material checkpointMaterial;

    // 30 feet converted to meters
    public float reachRadius = 9.144f;

    private List<Vector3> checkpointPositions;
    private List<GameObject> drawnCheckpoints = new List<GameObject>();
    private int currentTargetIndex = 0;

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

            // --- ADD THESE THREE LINES ---
            sphere.tag = "Checkpoint"; // This is the label
            SphereCollider col = sphere.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true; // This makes it a "Ghost" you can fly through
                                                   // -----------------------------

            float diameter = reachRadius * 2;
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);

            if (checkpointMaterial != null)
            {
                sphere.GetComponent<Renderer>().material = checkpointMaterial;
            }

            drawnCheckpoints.Add(sphere);
        }
        HighlightCheckpoint(0, true);
    }

    void HighlightCheckpoint(int index, bool active)
    {
        if (index >= 0 && index < drawnCheckpoints.Count)
        {
            var renderer = drawnCheckpoints[index].GetComponent<Renderer>();
            renderer.material.color = active ? new Color(0, 1, 0, 0.5f) : new Color(1, 1, 1, 0.3f);
        }
    }

    void Update()
    {
        if (checkpointPositions == null || currentTargetIndex >= checkpointPositions.Count) return;

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
    public Vector3? GetCurrentCheckpointPosition()
    {
        if (checkpointPositions == null || currentTargetIndex >= checkpointPositions.Count)
            return null;
        return checkpointPositions[currentTargetIndex];
    }

    public bool IsRaceFinished()
    {
        return checkpointPositions != null && currentTargetIndex >= checkpointPositions.Count;
    }

    public int GetCurrentIndex()
    {
        return currentTargetIndex;
    }
}

