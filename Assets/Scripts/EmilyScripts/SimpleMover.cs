using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    bool isUp = false;
    Vector3 startPos;
    Vector3 upPos;

    void Start()
    {
        startPos = transform.position;
        upPos = startPos + Vector3.up * 1f; // how far up it goes
    }

    public void Interact()
    {
        // When E is pressed, go up or down
        isUp = !isUp;
        transform.position = isUp ? upPos : startPos;
        Debug.Log($"{name} was interacted with!");
    }
}
