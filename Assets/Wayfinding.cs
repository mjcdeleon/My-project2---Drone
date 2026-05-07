using UnityEngine;
using TMPro; // Requires TextMeshPro — import via Package Manager if missing

/// <summary>
/// Wayfinding aid — WORLD COORDINATE SYSTEM.
///
/// Places a 3D arrow above the player that always points toward the next checkpoint,
/// and shows a TextMeshPro label with the distance and checkpoint name.
///
/// SETUP:
/// 1. Create an empty GameObject called "WayfindingArrow" and attach this script.
/// 2. In the Inspector, assign:
///    - arrowMesh     : a simple 3D arrow mesh (e.g. from the Unity asset store or
///                      a primitive cylinder + cone grouped together)
///    - distanceLabel : a TextMeshPro - Text (3D) child object of this GameObject
///    - playerTransform : the drone / camera rig Transform
/// 3. The arrow floats `floatHeight` metres above the player in world space.
/// </summary>
public class WayfindingArrow : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The arrow mesh GameObject (child of this object)")]
    public GameObject arrowMesh;

    [Tooltip("TextMeshPro 3D text for distance + checkpoint label")]
    public TextMeshPro distanceLabel;

    [Tooltip("The player/drone transform to follow")]
    public Transform playerTransform;

    [Header("Display Settings")]
    [Tooltip("How high above the player the arrow floates (world Y offset)")]
    public float floatHeight = 2.5f;

    [Tooltip("How far in front of the player the arrow is offset")]
    public float forwardOffset = 1.0f;

    [Tooltip("Scale of the arrow mesh")]
    public float arrowScale = 0.4f;

    [Tooltip("Rotation speed for the spinning arrow (degrees/sec). Set 0 to disable.")]
    public float spinSpeed = 45f;

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
        if (CheckpointManager.Instance == null || playerTransform == null) return;

        // Hide everything once race is finished
        if (CheckpointManager.Instance.IsRaceFinished())
        {
            arrowMesh.SetActive(false);
            if (distanceLabel) distanceLabel.gameObject.SetActive(false);
            return;
        }

        Transform target = CheckpointManager.Instance.GetCurrentCheckpoint();
        if (target == null) return;

        // --- Position: float above & slightly in front of the player (WORLD space) ---
        Vector3 worldPos = playerTransform.position
                           + Vector3.up * floatHeight
                           + playerTransform.forward * forwardOffset;
        transform.position = worldPos;

        // --- Rotation: point the arrow toward the checkpoint ---
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            // LookRotation makes the arrow's forward axis point at the checkpoint
            transform.rotation = Quaternion.LookRotation(directionToTarget);

            // Optional: add a gentle spin around the pointing axis for visibility
            if (spinSpeed != 0f)
                arrowMesh.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
        }

        // --- Distance label ---
        float distance = Vector3.Distance(playerTransform.position, target.position);
        if (distanceLabel != null)
        {
            distanceLabel.text = $"{CheckpointManager.Instance.GetCheckpointLabel()}\n{distance:F0} m";

            // Make label always face the player (billboard)
            distanceLabel.transform.LookAt(
                distanceLabel.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
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