using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance = null;

    public Transform orientation;
    
    [Header("Walking Move Speed")]
    public float walkingSpeed;
    [Header("Running Move Speed")]
    public float runningSpeed;
    private float moveSpeed;
    public float groundDrag;
    
    public bool isPlayerSprinting = false;
    public bool isPlayerInWater = false;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    

    // SFXs
    //public AudioSource walkingAudio;
    //public AudioSource runningAudio;

    void Start()
    {
        instance = this;
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
        //MaybePlayWalkingSound();

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

        if (Input.GetKeyUp(KeyCode.LeftShift))
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isPlayerInWater = true;
            Debug.Log("Entered Water!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isPlayerInWater = false;
            Debug.Log("Exited Water!");
        }
    }

    /*private void MaybePlayWalkingSound()
    {
        bool isPlayerMoving = Mathf.Abs(rb.velocity.x) > 0.01f || Mathf.Abs(rb.velocity.z) > 0.01f;
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
    }*/

    public Vector3 GetPlayerVelocity()
    {
        return rb.velocity;
    }
}
