using UnityEngine;

public class RaycastInteractor : MonoBehaviour
{
    public Camera cam;              // your main camera
    public float distance = 3.5f;   // how far you can look
    public LayerMask mask = ~0;     // hit everything
    public string interactableTag = "Interactable";
    public KeyCode interactKey = KeyCode.E;

    public GameObject handIcon;     // UI hand icon

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
            if (hit.collider.CompareTag(interactableTag))
            {
                lookingAtInteractable = true;

                // call Interact() if the object has it
                if (Input.GetKeyDown(interactKey))
                {
                    hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        if (handIcon) handIcon.SetActive(lookingAtInteractable);
    }
}
