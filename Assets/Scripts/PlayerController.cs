using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform orientation;
    
    [Header("Walking Move Speed")]
    public float walkingSpeed;
    [Header("Running Move Speed")]
    public float runningSpeed;
    private float moveSpeed;
    public float groundDrag;
    
    private bool isPlayerSprinting = false;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    

    // SFXs
    public AudioSource walkingAudio;
    public AudioSource runningAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        moveSpeed = walkingSpeed;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        MaybePlayWalkingSound();

        rb.drag = grounded ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = runningSpeed;
            isPlayerSprinting = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = walkingSpeed;
            isPlayerSprinting = false;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void MaybePlayWalkingSound()
    {
        bool isPlayerMoving = rb.velocity.x > 0.1f || rb.velocity.z > 0.1f;
        if (isPlayerMoving && !isPlayerSprinting && !walkingAudio.isPlaying)
            walkingAudio.Play();
        else if (isPlayerMoving && isPlayerSprinting && !runningAudio.isPlaying)
            runningAudio.Play();

        if (!isPlayerMoving)
        {
            if (walkingAudio.isPlaying)
                walkingAudio.Stop();

            if (runningAudio.isPlaying)
                runningAudio.Stop();
        }
    }
}
