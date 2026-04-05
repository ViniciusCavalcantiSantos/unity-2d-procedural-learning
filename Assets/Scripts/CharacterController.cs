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

    public bool facingRight = true;

    [Header("Sensors Offset")]
    private Transform floorCheckLeft;
    private Transform floorCheckRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        floorCheckLeft = transform.Find("Sensors/FloorCheckLeft");
        floorCheckRight = transform.Find("Sensors/FloorCheckRight");
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
        UpdateIsGrounded();
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isGrounded", isGrounded);

        // Se estiver no chão, forçamos o desligamento das animações aéreas
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else // SE ESTIVER NO AR:
        {
            if (isMovingUp)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
                animator.SetBool("isRunning", false);
            }
            else if (isMovingDown)
            {
                animator.SetBool("isFalling", true);
                animator.SetBool("isJumping", false);
                animator.SetBool("isRunning", false);
            }
            // NOTA: Não precisamos de um "else" aqui! 
            // Se isMovingUp e isMovingDown forem FALSE (ápice do pulo), 
            // o código ignora, e o Animator mantém o "isJumping" como TRUE
            // até o personagem começar a cair.
        }

        // Rotaciona o personagem para a direção que ele está se movendo    
        Vector3 scale = transform.localScale;
        if (isMovingRight)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    private void UpdateIsGrounded()
    {
        Debug.DrawRay(floorCheckLeft.position, Vector2.down * 0.1f, Color.red);
        Debug.DrawRay(floorCheckRight.position, Vector2.down * 0.1f, Color.red);

        RaycastHit2D hitLeft = Physics2D.Raycast(floorCheckLeft.position, Vector2.down, 0.1f, GameLayers.Ground);
        RaycastHit2D hitRight = Physics2D.Raycast(floorCheckRight.position, Vector2.down, 0.1f, GameLayers.Ground);

        if (hitLeft.collider != null || hitRight.collider != null)
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

        if (horizontal != 0)
        {
            speed += horizontal * acceleration * Time.deltaTime;
            animator.SetBool("isRunning", true);
        }
        else
        {
            speed = Mathf.MoveTowards(speed, 0, deceleration * Time.deltaTime);
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
