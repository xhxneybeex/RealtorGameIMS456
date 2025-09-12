using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // --- Movement ---
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -24f;   // stronger works better with CharacterController
    public float jumpHeight = 1.6f;   // meters

    // --- Mouse Look ---
    public Transform cameraHolder;     // child at head height with Main Camera inside
    public float lookSensitivity = 2f;
    public float minPitch = -80f, maxPitch = 80f;

    // --- Animator parameter names ---
    public string speedParam = "Speed";       // Blend Tree: 0 idle, 0.5 walk, 1 run
    public string groundedBool = "IsGrounded";  // Bool
    public string jumpTrigger = "Jump";        // Trigger

    CharacterController cc;
    Animator animator;
    Vector3 velocity;
    float yaw, pitch;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // auto-find cameraHolder if not assigned
        if (!cameraHolder)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam) cameraHolder = cam.transform.parent ? cam.transform.parent : cam.transform;
        }

        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        MoveAndAnimate();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);          // player yaw
        if (cameraHolder) cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f); // camera pitch
    }

    void MoveAndAnimate()
    {
        // --- Grounded / stick-to-ground ---
        bool isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;

        // --- Input & movement (no rotation from WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (transform.right * h + transform.forward * v).normalized;

        bool moving = dir.sqrMagnitude > 0f;
        bool running = moving && Input.GetKey(KeyCode.LeftShift);

        float moveSpeed = running ? runSpeed : (moving ? walkSpeed : 0f);
        if (moveSpeed > 0f)
            cc.Move(dir * moveSpeed * Time.deltaTime);

        // --- Jump ---
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity); // v = sqrt(2 g h)
            if (animator) animator.SetTrigger(jumpTrigger);
        }

        // --- Gravity ---
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        // --- Animator params ---
        if (animator)
        {
            // your Blend Tree thresholds: 0 idle, 0.5 walk, 1 run
            float animSpeed = running ? 1f : (moving ? 0.5f : 0f);
            animator.SetFloat(speedParam, animSpeed, 0.1f, Time.deltaTime);
            animator.SetBool(groundedBool, isGrounded);
        }
    }
}
