using System.Xml.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance
    {
        get; private set;
    }
    public Transform[] checkpoints;
    private int currentIndex = 0;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //if(checkpoints == null || checkpoints.Length == 0)
        //{

        //checkpoints = CreatePlaceholders();
        //}
    }
    public Transform GetCurrentCheckpoint()
    {
        if (currentIndex < checkpoints.Length)
        {
            return checkpoints[currentIndex];
        }
        return null;
    }
    public string GetCheckpointLabel()
    {
        if (currentIndex >= checkpoints.Length)
        {
            return "Final Checkpoint";
        }
        bool isLast = currentIndex == checkpoints.Length - 1;
        return $"{name} ({currentIndex + 1}/{checkpoints.Length})";
    }
    public void AdvanceCheckpoint()
    {
        if (currentIndex < checkpoints.Length)
        {
            Debug.Log($"Checkpoint {currentIndex + 1} reached");
            currentIndex++;
        }
    }

    public bool IsRaceFinished() => currentIndex >= checkpoints.Length;
}
    //maybe delete later -- see if checkpoints are already loaded from canvas
    //private Transform[] CreateCheckpoints()
    //{
    //int count = 7;
    //Transform[] placeholders = new Transform[count];
    //for (int i = 0; i < count; i++)
    //{
    //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //go.name = i == count - 1 ? "FinishLine_Placeholder" : $"Checkpoint_{i + 1}_Placeholder";
    //go.transform.position = new Vector3(i * 30f, 10f, 30f);
    //go.transform.localScale = Vector3.one * 3f;
    //go.GetComponent<Renderer>().material.color = i == count - 1 ? Color.yellow : Color.cyan;
    //Destroy(go.GetComponent<Collider>());
    //placeholders[i] = go.transform;
    //}
    //return placeholders;
    //}