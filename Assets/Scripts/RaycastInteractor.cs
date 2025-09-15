using UnityEngine;

public class RaycastInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                      // FPS camera (auto-finds MainCamera if empty)
    public float distance = 3.5f;
    public LayerMask mask = ~0;             // start with Everything
    public string interactableTag = "Interactable";
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject handIcon;             // UI Image GameObject (PNG sprite in a Canvas)

    SimpleDoor currentDoor;

    void Awake()
    {
        if (!cam) cam = Camera.main ? Camera.main : GetComponentInChildren<Camera>();
        if (handIcon) handIcon.SetActive(false);
    }

    void Update()
    {
        currentDoor = null;

        // Cast a single ray from camera forward
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            // TAG-BASED FILTER: only consider objects tagged Interactable
            if (hit.collider.CompareTag(interactableTag))
            {
                currentDoor = hit.collider.GetComponentInParent<SimpleDoor>();
            }
        }

        // Show/hide hand icon
        if (handIcon) handIcon.SetActive(currentDoor != null);

        // Interact
        if (currentDoor != null && Input.GetKeyDown(interactKey))
        {
            currentDoor.Interact();
        }
    }
}
