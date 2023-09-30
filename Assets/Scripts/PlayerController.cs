using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public float runningSpeed = 5f;
    public float walkingSpeed = 3f;

    private float speed;

    public float gravity = 9.87f;
    private float verticalSpeed = 0;
    private Vector3 yDirectionMove;

    void Start()
    {
        yDirectionMove = new Vector3(0, 0, 0);
        speed = walkingSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessPlayerInput();
        Move();
    }

    void ProcessPlayerInput()
    {
        // Sprinting
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = runningSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = walkingSpeed;
        }
    }

    void Move()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        if (characterController.isGrounded) verticalSpeed = 0;
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }
        Vector3 move = (transform.forward * verticalMove) + (transform.right * horizontalMove);
        yDirectionMove.y = verticalSpeed;
        characterController.Move((speed * Time.deltaTime * move) + (yDirectionMove * Time.deltaTime));
    }
}
