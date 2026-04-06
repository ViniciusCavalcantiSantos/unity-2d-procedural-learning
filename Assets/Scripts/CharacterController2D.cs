using System;
using UnityEngine;
using UnityEngine.InputSystem;

// This script is a basic 2D character controller that allows
// the player to run and jump. It uses Unity's new input system,
// which needs to be set up accordingly for directional movement
// and jumping buttons.

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterController2D : MonoBehaviour
{
    [Header("Hair Offsets (Assume facing right)")]
    [SerializeField] private Vector2 idleOffset;
    [SerializeField] private Vector2 runOffset;
    [SerializeField] private Vector2 jumpOffset;
    [SerializeField] private Vector2 fallOffset;

    [Header("Hair Anchor")]
    [SerializeField] private HairAnchorOld hairAnchor;

    [Header("Movement Params")]
    public float runSpeed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravityScale = 20.0f;

    // components attached to player
    private BoxCollider2D coll;
    private Rigidbody2D rb;
    private PlayerInput input;
    private Animator animator;

    // input
    private Vector2 moveDirection = Vector2.zero;
    private bool jumpPressed = false;

    // other
    private bool facingRight = true;
    private bool isGrounded = false;

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        rb.gravityScale = gravityScale;
    }

    public void JumpPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
        }
        else if (context.canceled)
        {
            jumpPressed = false;
        }
    }

    public void MovePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
    }

    private void FixedUpdate()
    {

        UpdateIsGrounded();

        HandleHorizontalMovement();

        HandleJumping();

        UpdateFacingDirection();

        UpdateHairOffset();

        UpdateAnimator();

    }

    private void UpdateHairOffset()
    {
        const float VELOCITY_THRESHOLD = 0.1f;

        float velX = rb.linearVelocity.x;
        float velY = rb.linearVelocity.y;

        bool tryingToMove = Math.Abs(moveDirection.x) > VELOCITY_THRESHOLD;
        bool movingHorizontally = Math.Abs(velX) > VELOCITY_THRESHOLD;
        bool isAirbone = !isGrounded;
        bool isRising = velY > VELOCITY_THRESHOLD;
        bool isFalling = velY < -VELOCITY_THRESHOLD;


        Vector2 currentOffset;
        if (isAirbone)
        {
            currentOffset = isRising ? jumpOffset : fallOffset;
            if (movingHorizontally || tryingToMove)
            {
                float airBlend = Mathf.Clamp01(Mathf.Abs(velX) / runSpeed);
                currentOffset = Vector2.Lerp(currentOffset, runOffset, airBlend * 0.5f);
            }
        }
        else if (movingHorizontally || tryingToMove)
        {
            float speedRatio = Mathf.Clamp01(Mathf.Abs(velX) / runSpeed);
            currentOffset = Vector2.Lerp(idleOffset, runOffset, speedRatio);
        }
        else
        {
            currentOffset = idleOffset;
        }

        if (!facingRight)
        {
            currentOffset.x *= -1;
        }

        hairAnchor.partOffset = currentOffset;
    }

    private void UpdateIsGrounded()
    {
        Bounds colliderBounds = coll.bounds;
        float colliderRadius = coll.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
        // Check if player is grounded
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
        // Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
        this.isGrounded = false;
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != coll)
                {
                    this.isGrounded = true;
                    break;
                }
            }
        }
    }

    private void HandleHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * runSpeed, rb.linearVelocity.y);
    }

    private void HandleJumping()
    {
        if (isGrounded && jumpPressed)
        {
            isGrounded = false;
            jumpPressed = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        }
    }

    private void UpdateFacingDirection()
    {
        // set facing direction
        if (moveDirection.x > 0.1f)
        {
            facingRight = true;
        }
        else if (moveDirection.x < -0.1f)
        {
            facingRight = false;
        }

        // rotate according to direction
        // we do this instead of using the 'flipX' spriteRenderer option because our player is made up of multiple sprites
        if (facingRight)
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 0, this.transform.eulerAngles.z);
        }
        else
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 180, this.transform.eulerAngles.z);
        }
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("movementX", rb.linearVelocity.x);
        animator.SetFloat("movementY", rb.linearVelocity.y);
    }

}