using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    [Header("Door Mesh")]
    [Tooltip("Child mesh that represents the door leaf. Must have a MeshFilter.")]
    public Transform doorMesh;

    public enum HingeSide { Left, Right }

    [Header("Hinge Auto-Placement")]
    public HingeSide side = HingeSide.Left;
    [Tooltip("Offset along local Y if you want the hinge higher or lower.")]
    public float verticalOffset = 0f;
    [Tooltip("Offset along local Z if you want a tiny depth offset from the frame.")]
    public float depthOffset = 0f;

    [Header("Open/Close")]
    [Tooltip("Positive angle in degrees for the single allowed swing direction.")]
    public float openAngle = 90f;
    [Tooltip("Degrees per second.")]
    public float rotateSpeed = 180f;

    bool isOpen = false;
    float closedYaw;
    float openYaw;
    float targetYaw;

    void Awake()
    {
        if (!doorMesh)
        {
            // try find first child with a MeshFilter
            var mf = GetComponentInChildren<MeshFilter>();
            if (mf) doorMesh = mf.transform;
        }

        if (!Application.isPlaying)
        {
            // keep hinge aligned to door when editing
            AlignHingeToDoor();
        }

        // cache angles
        closedYaw = transform.localEulerAngles.y;
        openYaw = Mathf.Repeat(closedYaw + Mathf.Abs(openAngle), 360f);
        targetYaw = closedYaw;
    }

    void Update()
    {
        // rotate hinge toward target
        float current = transform.localEulerAngles.y;
        float newYaw = Mathf.MoveTowardsAngle(current, targetYaw, rotateSpeed * Time.deltaTime);
        var e = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(e.x, newYaw, e.z);
    }

    public void Interact()
    {
        isOpen = !isOpen;
        targetYaw = isOpen ? openYaw : closedYaw;
        Debug.Log($"{name}: {(isOpen ? "Opening" : "Closing")} to {targetYaw}°");
    }

    [ContextMenu("Snap Hinge To Edge")]
    public void SnapHingeToEdge()
    {
        if (!AlignHingeToDoor())
            Debug.LogWarning("SimpleHingedDoor: Assign a doorMesh with a MeshFilter first.");
        else
            Debug.Log($"SimpleHingedDoor: Hinge snapped to {side} edge.");
    }

    bool AlignHingeToDoor()
    {
        if (!doorMesh) return false;
        var mf = doorMesh.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return false;

        // local bounds of the door mesh
        var lb = mf.sharedMesh.bounds;
        Vector3 c = lb.center;
        Vector3 ext = lb.extents;

        // pick left or right edge in local space
        float edgeX = side == HingeSide.Left ? -ext.x : ext.x;

        // local point on edge, allow optional offsets
        Vector3 localEdge = new Vector3(edgeX, c.y + verticalOffset, c.z + depthOffset);

        // move hinge to that edge, align forward and up to match door
        transform.position = doorMesh.TransformPoint(localEdge);
        transform.rotation = Quaternion.LookRotation(doorMesh.forward, doorMesh.up);

        // after snapping, recompute angles so closedYaw is current
        closedYaw = transform.localEulerAngles.y;
        openYaw = Mathf.Repeat(closedYaw + Mathf.Abs(openAngle), 360f);
        if (!isOpen) targetYaw = closedYaw;

        return true;
    }

    void OnValidate()
    {
        // keep angles consistent if you tweak values in inspector
        openAngle = Mathf.Abs(openAngle);
        openYaw = Mathf.Repeat(transform.localEulerAngles.y + openAngle, 360f);
        if (!isOpen) targetYaw = transform.localEulerAngles.y;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.04f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
    }
}
