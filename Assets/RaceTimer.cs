using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{


    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 2, 3);
    public TextMeshPro timerLabel;
    public CheckpointManager checkpointManager;

    private float elapsedTime = 0f;
    private bool isRunning = true;

    // Update is called once per frame
    void Update()
    {
        if(checkpointManager == null)
        {
            return;
        }

        if(!checkpointManager.IsRaceFinished() && isRunning)
        {
            elapsedTime += Time.deltaTime;
        }
        else if (checkpointManager.IsRaceFinished() && isRunning)
        {
            isRunning = false;
            Debug.Log($"Race finished! Final time: {FormatTime(elapsedTime)}");
        }

        if(timerLabel != null)
        {
            timerLabel.text = FormatTime(elapsedTime);
        }

        if (playerTransform != null)
        {
            timerLabel.transform.position = playerTransform.position + playerTransform.TransformDirection(offset);
            timerLabel.transform.LookAt(timerLabel.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
        if (timerLabel != null)
        {
            timerLabel.text = FormatTime(elapsedTime);
            timerLabel.transform.LookAt(
                timerLabel.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = (int) (time / 60);
        int seconds = (int) (time % 60);
        int milliseconds = (int)((time * 100) % 100);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00+}";
    }
}
