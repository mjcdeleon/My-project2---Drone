using UnityEngine;
using UnityEngine.InputSystem;


public class CameraModeSwitcher : MonoBehaviour
{
    public enum ViewMode { PilotOnly = 0, PilotWithCockpit = 1, ThirdPerson = 2 }
    public Camera pilotCamera;
    public Camera thirdPersonCamera;
    public GameObject cockpitRoot;
    public Transform droneTransform;
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -5f);
    public float followSmoothness = 8f;
    public InputActionProperty pinchAction;
    public float pinchThreshold = 0.8f;
    public float switchCooldown = 1.0f;




    private ViewMode currentMode = ViewMode.PilotOnly;
    private float cooldownTimer = 0f;
    private bool wasPinching = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pinchAction.action?.Enable();
        ApplyMode(currentMode);

    }

    // Update is called once per frame
    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        float pinchValue = pinchAction.action?.ReadValue<float>() ?? 0f;
        bool isPinching = pinchValue >= pinchThreshold;
        if(isPinching && !wasPinching && cooldownTimer <= 0f)
        {
            CycleMode();
            cooldownTimer = switchCooldown;
        }

        wasPinching = isPinching;
        if(currentMode == ViewMode.ThirdPerson && droneTransform != null && thirdPersonCamera != null)
        {
            UpdateThirdPersonCamera();
        }
    }

    private void CycleMode()
    {
        int next = ((int)currentMode + 1) % 3;
        currentMode = (ViewMode)next;
        ApplyMode(currentMode);
        Debug.Log($"CameraModeSwitcher: Switched to {currentMode}");

    }

    private void ApplyMode(ViewMode mode)
    {
        switch (mode)
        {
            case ViewMode.PilotOnly:
                SetPilotCameraActive(true);
                SetCockpitVisible(false);
                SetThirdPersonCameraActive(false);
                break;

            case ViewMode.PilotWithCockpit:
                SetPilotCameraActive(true);
                SetCockpitVisible(true);
                SetThirdPersonCameraActive(false);
                break;
            case ViewMode.ThirdPerson:
                SetPilotCameraActive(false);
                SetCockpitVisible(false);
                SetThirdPersonCameraActive(true);
                break;
        }
    }

    private void SetPilotCameraActive(bool active)
    {
        if (pilotCamera != null)
        {
            pilotCamera.gameObject.SetActive(active);
  
        }
    }

    private void SetCockpitVisible(bool visible)
    {
        if(cockpitRoot != null)
        {
            cockpitRoot.SetActive(visible);
        }
    }

    private void SetThirdPersonCameraActive(bool active)
    {
        if(thirdPersonCamera != null)
        {
            thirdPersonCamera.gameObject.SetActive(active);

        }
    }

    private void UpdateThirdPersonCamera()
    {
        Vector3 desiredPosition = droneTransform.TransformPoint(thirdPersonOffset);
        thirdPersonCamera.transform.position = Vector3.Lerp(thirdPersonCamera.transform.position, desiredPosition, Time.deltaTime * followSmoothness);

        thirdPersonCamera.transform.LookAt(droneTransform.position);
    }

    void OnDisable()
    {
        pinchAction.action?.Disable();
    }
}
