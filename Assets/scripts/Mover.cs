using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float sprintMultiplier = 2f;

    [Tooltip("Higher = snappier steering in air. Lower = heavier inertia.")]
    public float airControl = 5f;

    private Rigidbody rb;
    private Vector2 movementInput;
    private bool isGrounded;
    private bool isSprinting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jumped!");
        }
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void FixedUpdate()
    {
        float currentSpeed = isSprinting ? (moveSpeed * sprintMultiplier) : moveSpeed;

        // Calculate the exact speed and direction the player WANTS to go
        Vector3 targetDirection = new Vector3(movementInput.x, 0f, movementInput.y);
        Vector3 targetVelocity = targetDirection * currentSpeed;

        if (isGrounded)
        {
            // ON THE GROUND: Instant, perfect control
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            // IN THE AIR: Isolate horizontal momentum so we don't mess with gravity
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // LERP TRICK: Smoothly slides from current momentum to the requested target velocity
            Vector3 newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, airControl * Time.fixedDeltaTime);

            // Reapply the smoothed horizontal speed while keeping the vertical falling speed
            rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        }
    }
}