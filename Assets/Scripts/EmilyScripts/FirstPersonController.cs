using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    // --- Movement ---
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -24f;      // works well with CharacterController
    public float jumpHeight = 1.6f;     // meters

    // --- Jump (with delay) ---
    [Header("Jump Settings")]
    public float jumpDelay = 1f;

    // --- Fast grounded probe ---
    [Header("Ground Check")]
    public LayerMask groundLayers = ~0; // set to your floor layers
    public float groundCheckRadius = 0.2f;
    public float groundCheckOffset = 0.05f;

    // --- Mouse Look ---
    [Header("Look")]
    public Transform cameraHolder;      // parent of the Camera (pitch lives here)
    public float lookSensitivity = 2f;
    public float minPitch = -80f, maxPitch = 80f;

    // --- Animator (optional) ---
    [Header("Animator (optional)")]
    public Animator animator;
    public string speedParam = "Speed";
    public string groundedBool = "IsGrounded";
    public string jumpTrigger = "Jump";

    CharacterController cc;
    Vector3 velocity;
    float yaw, pitch;

    // Jump state
    bool groundedNow;
    bool jumpQueued = false;
    float jumpTimer = 0f;
    bool jumpArmed = true;  // re-armed when landed & Space released
    bool jumpHeld = false;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // Auto-find cameraHolder if not set
        if (!cameraHolder)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam) cameraHolder = cam.transform.parent ? cam.transform.parent : cam.transform;
        }

        if (!animator) animator = GetComponentInChildren<Animator>();

        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Input edges for jump
        bool pressJump = Input.GetKeyDown(KeyCode.Space);
        bool releaseJump = Input.GetKeyUp(KeyCode.Space);
        jumpHeld = Input.GetKey(KeyCode.Space);
        if (releaseJump) jumpArmed = true;

        Look();
        Move(pressJump);
        HandleJumpDelay();
        Animate();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // yaw on body, pitch on camera holder
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraHolder) cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move(bool pressJump)
    {
        // Planar movement (camera/body forward)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (transform.right * h + transform.forward * v).normalized;

        bool moving = dir.sqrMagnitude > 0f;
        bool running = moving && Input.GetKey(KeyCode.LeftShift);
        float speed = running ? runSpeed : (moving ? walkSpeed : 0f);

        if (speed > 0f)
            cc.Move(dir * speed * Time.deltaTime);

        // Fast grounded probe (snappier than isGrounded)
        groundedNow = FastGrounded();
        if (groundedNow && velocity.y < 0f)
            velocity.y = -4f; // stick to floor

        // Queue jump with delay (don’t apply Y yet)
        if (groundedNow && pressJump && !jumpQueued && jumpArmed)
        {
            jumpQueued = true;
            jumpTimer = Mathf.Max(0f, jumpDelay);
            if (animator) { animator.ResetTrigger(jumpTrigger); animator.SetTrigger(jumpTrigger); }
            jumpArmed = false; // re-armed after land & release
        }

        // Gravity + vertical move
        velocity.y += gravity * Time.deltaTime;
        cc.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);

        // Re-arm once landed and key released
        if (groundedNow && !jumpHeld) jumpArmed = true;
    }

    void HandleJumpDelay()
    {
        if (!jumpQueued) return;

        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f)
        {
            // Liftoff only if still grounded (prevents midair jump)
            if (groundedNow)
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            jumpQueued = false;
        }
    }

    bool FastGrounded()
    {
        var b = cc.bounds;
        Vector3 feet = new Vector3(b.center.x, b.min.y + groundCheckOffset, b.center.z);
        return Physics.CheckSphere(feet, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void Animate()
    {
        if (!animator) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool moving = (new Vector2(h, v)).sqrMagnitude > 0f;
        bool running = moving && Input.GetKey(KeyCode.LeftShift);
        float animSpeed = moving ? (running ? 1f : 0.5f) : 0f;

        animator.SetFloat(speedParam, animSpeed, 0.1f, Time.deltaTime);
        animator.SetBool(groundedBool, groundedNow);
    }

  
}
