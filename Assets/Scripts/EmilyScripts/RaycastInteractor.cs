using UnityEngine;

public class RaycastInteractor : MonoBehaviour
{
    public Camera cam;
    public float distance = 5f;
    public LayerMask mask = ~0;
    public string interactableTag = "Interactable";
    public KeyCode interactKey = KeyCode.E;
    public bool showDebugRay = true;

    public GameObject handIcon;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (handIcon) handIcon.SetActive(false);
    }

    void Update()
    {
        bool lookingAtInteractable = false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }

            if (hit.collider.CompareTag(interactableTag))
            {
                lookingAtInteractable = true;

                if (Input.GetKeyDown(interactKey))
                {
                    hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else
        {
            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
            }
        }

        if (handIcon) handIcon.SetActive(lookingAtInteractable);
    }
}
