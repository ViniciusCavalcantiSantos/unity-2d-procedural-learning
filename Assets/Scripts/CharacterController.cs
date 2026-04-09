using UnityEngine;

public class CharacterController : MonoBehaviour
{
    const float VELOCITY_THRESHOLD = 0.1f;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    public Animator animator;

    public float speed = 0f;
    public float acceleration = 20f;
    public float deceleration = 30f;
    public float airacceleration = 15f;
    public float airDeceleration = 2f;

    public float maxSpeed = 5f;

    public float jumpForce = 6f;

    private InputSystem_Actions input;
    private Vector2 moveDirection = Vector2.zero;

    [Header("Ground Check")]
    public bool isGrounded = false;
    public bool isMovingUp = false;
    public bool isMovingDown = false;
    public bool isMovingLeft = false;
    public bool isMovingRight = false;

    public bool isFacingRight = true;
    private bool previousFacingRight;


    [Header("Debug")]
    public bool showSensorRays = false;

    private RaycastHit2D
        hitDownLeft, hitDownCenter, hitDownRight,
        hitRightUp, hitRightCenter, hitRightDown,
        hitLeftUp, hitLeftCenter, hitLeftDown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        previousFacingRight = isFacingRight;
    }
    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        HandleHorizontalMovement();
        HandleJumping();

        UpdateYMovement();
        UpdateXMovement();

        UpdateAnimations();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateSensors();
        UpdateIsGrounded();
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isGrounded", isGrounded);


        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else
        {
            animator.SetBool("isRunning", false);
            if (isMovingUp)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else if (isMovingDown)
            {
                animator.SetBool("isFalling", true);
                animator.SetBool("isJumping", false);
            }
        }

        // Rotaciona o personagem para a direção que ele está se movendo    
        Vector3 scale = transform.localScale;
        if (isMovingLeft)
        {
            scale.x = -Mathf.Abs(scale.x);
            isFacingRight = false;
        }
        else
        {
            scale.x = Mathf.Abs(scale.x);
            isFacingRight = true;
        }


        if (isFacingRight != previousFacingRight && Mathf.Abs(speed) > maxSpeed * 0.5f && isGrounded)
        {
            animator.SetTrigger("turn");
        }
        previousFacingRight = isFacingRight;

        transform.localScale = scale;
    }

    private void UpdateSensors()
    {
        float sensorSize = 0.05f;

        // floor check
        Vector2 floorCheckLeft = new Vector2(boxCollider.bounds.min.x + 0.005f, boxCollider.bounds.min.y);
        Vector2 floorCheckCenter = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Vector2 floorCheckRight = new Vector2(boxCollider.bounds.max.x - 0.005f, boxCollider.bounds.min.y);

        hitDownLeft = Physics2D.Raycast(floorCheckLeft, Vector2.down, sensorSize, GameLayers.Ground);
        hitDownCenter = Physics2D.Raycast(floorCheckCenter, Vector2.down, sensorSize, GameLayers.Ground);
        hitDownRight = Physics2D.Raycast(floorCheckRight, Vector2.down, sensorSize, GameLayers.Ground);

        // wall check right
        Vector2 wallCheckRightUp = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.max.y - 0.005f);
        Vector2 wallCheckRightCenter = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.center.y);
        Vector2 wallCheckRightDown = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.min.y + 0.005f);

        hitRightUp = Physics2D.Raycast(wallCheckRightUp, Vector2.right, sensorSize, GameLayers.Ground);
        hitRightCenter = Physics2D.Raycast(wallCheckRightCenter, Vector2.right, sensorSize, GameLayers.Ground);
        hitRightDown = Physics2D.Raycast(wallCheckRightDown, Vector2.right, sensorSize, GameLayers.Ground);

        // wall check left
        Vector2 wallCheckLeftUp = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.max.y - 0.005f);
        Vector2 wallCheckLeftCenter = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.center.y);
        Vector2 wallCheckLeftDown = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.min.y + 0.005f);

        hitLeftUp = Physics2D.Raycast(wallCheckLeftUp, Vector2.left, sensorSize, GameLayers.Ground);
        hitLeftCenter = Physics2D.Raycast(wallCheckLeftCenter, Vector2.left, sensorSize, GameLayers.Ground);
        hitLeftDown = Physics2D.Raycast(wallCheckLeftDown, Vector2.left, sensorSize, GameLayers.Ground);

        if (showSensorRays)
        {
            Debug.DrawRay(floorCheckLeft, Vector2.down * sensorSize, Color.red);
            Debug.DrawRay(floorCheckCenter, Vector2.down * sensorSize, Color.red);
            Debug.DrawRay(floorCheckRight, Vector2.down * sensorSize, Color.red);


            Debug.DrawRay(wallCheckRightUp, Vector2.right * sensorSize, Color.red);
            Debug.DrawRay(wallCheckRightCenter, Vector2.right * sensorSize, Color.red);
            Debug.DrawRay(wallCheckRightDown, Vector2.right * sensorSize, Color.red);

            Debug.DrawRay(wallCheckLeftUp, Vector2.left * sensorSize, Color.red);
            Debug.DrawRay(wallCheckLeftCenter, Vector2.left * sensorSize, Color.red);
            Debug.DrawRay(wallCheckLeftDown, Vector2.left * sensorSize, Color.red);
        }
    }

    private void UpdateIsGrounded()
    {

        if (hitDownLeft.collider != null || hitDownRight.collider != null || hitDownCenter.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded && rb.linearVelocityY <= 0)
        {
            rb.linearVelocityY = 0;
        }
    }

    private void UpdateYMovement()
    {
        if (rb.linearVelocityY > VELOCITY_THRESHOLD)
        {
            isMovingUp = true;
            isMovingDown = false;
        }
        else if (rb.linearVelocityY < -VELOCITY_THRESHOLD)
        {
            isMovingUp = false;
            isMovingDown = true;
        }
        else
        {
            isMovingUp = false;
            isMovingDown = false;
        }
    }

    private void UpdateXMovement()
    {
        // set facing direction
        if (moveDirection.x > VELOCITY_THRESHOLD)
        {
            isMovingRight = true;
            isMovingLeft = false;
        }
        else if (moveDirection.x < -VELOCITY_THRESHOLD)
        {
            isMovingRight = false;
            isMovingLeft = true;
        }
    }

    private void HandleHorizontalMovement()
    {
        moveDirection = input.Player.Move.ReadValue<Vector2>();
        float horizontal = moveDirection.x;

        float currentAcceleration = isGrounded ? acceleration : airacceleration;
        float currentDeceleration = isGrounded ? deceleration : airDeceleration;

        if (horizontal != 0)
        {
            speed += horizontal * currentAcceleration * Time.deltaTime;
            animator.SetBool("isRunning", true);
        }
        else
        {
            speed = Mathf.MoveTowards(speed, 0, currentDeceleration * Time.deltaTime);
            animator.SetBool("isRunning", false);
        }
        speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);
        rb.linearVelocityX = speed;
    }

    private void HandleJumping()
    {
        if (isGrounded && input.Player.Jump.triggered)
        {
            isGrounded = false;
            rb.linearVelocityY = jumpForce;
        }
    }
}
