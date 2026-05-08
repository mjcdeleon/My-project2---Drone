using UnityEngine;
using TMPro;


public class WayfindingArrow : MonoBehaviour
{

    public GameObject arrowMesh;
    public TextMeshPro distanceLabel;
    public Transform playerTransform;
    public float floatHeight = 2.5f;
    public float forwardOffset = 1.0f;
    public float arrowScale = 0.4f;
    public float spinSpeed = 45f;
    public CheckpointManager checkpointManager;

    void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            // Try to find the XR camera rig
            //var rig = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            //if (rig != null) playerTransform = rig.transform;
            //else playerTransform = Camera.main?.transform;
            playerTransform = Camera.main?.transform;
            if (playerTransform == null)
                Debug.LogError("WayfindingArrow: Could not find player transform. Assign it in the Inspector.");
        }

        if (arrowMesh == null)
        {
            Debug.LogWarning("WayfindingArrow: No arrow mesh assigned. Creating a placeholder arrow.");
            arrowMesh = CreatePlaceholderArrow();
        }

        arrowMesh.transform.localScale = Vector3.one * arrowScale;
    }

    void Update()
    {
        if (checkpointManager == null || playerTransform == null) return;

        if (checkpointManager.IsRaceFinished())
        {
            arrowMesh.SetActive(false);
            if (distanceLabel) distanceLabel.gameObject.SetActive(false);
            return;
        }

        Vector3? target = checkpointManager.GetCurrentCheckpointPosition();
        if (target == null) return;

        Vector3 worldPos = playerTransform.position
                           + Vector3.up * floatHeight
                           + playerTransform.forward * forwardOffset;
        transform.position = worldPos;

        Vector3 directionToTarget = (target.Value - transform.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            if (spinSpeed != 0f)
                arrowMesh.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
        }

        float distance = Vector3.Distance(playerTransform.position, target.Value);
        if (distanceLabel != null)
        {
            distanceLabel.text = $"Checkpoint {checkpointManager.GetCurrentIndex() + 1}\n{distance:F0} m";
            distanceLabel.transform.LookAt(
                distanceLabel.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }

    // -----------------------------------------------------------------------
    // Builds a simple arrow from primitives if no mesh is assigned
    // -----------------------------------------------------------------------
    private GameObject CreatePlaceholderArrow()
    {
        GameObject arrow = new GameObject("PlaceholderArrow");
        arrow.transform.SetParent(transform);
        arrow.transform.localPosition = Vector3.zero;

        // Shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(arrow.transform);
        shaft.transform.localPosition = new Vector3(0, 0, 0);
        shaft.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
        // Rotate so the cylinder points along Z (forward)
        shaft.transform.localRotation = Quaternion.Euler(90, 0, 0);
        shaft.GetComponent<Renderer>().material.color = Color.red;
        Destroy(shaft.GetComponent<Collider>());

        // Arrowhead (cone approximated with a scaled sphere for simplicity)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(arrow.transform);
        head.transform.localPosition = new Vector3(0, 0, 0.55f);
        head.transform.localScale = new Vector3(0.22f, 0.22f, 0.35f);
        head.GetComponent<Renderer>().material.color = Color.red;
        Destroy(head.GetComponent<Collider>());

        return arrow;
    }

}