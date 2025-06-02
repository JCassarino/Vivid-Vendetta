using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{
   
    #region Variables

    // Player Attributes
    private Collider2D playerCollider;
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool facingRight = true;
    private bool canDoubleJump = true;

    // Movement Tuning
    public float groundMoveSpeed = 10f;
    public float airMoveSpeed = 8f;
    public float jumpForce = 5f;
    public float wallSlideSpeed = 2f;
    public float groundDeceleration = 35f;
    public float airDeceleration = 10f;

    // Dash Handling
    private bool canDash = true;
    private bool isDashing;
    private float dashSpeed = 25f;
    private float dashTime = 0.15f;
    private float dashCooldown = 0.5f;
    private Vector2 preDashVelocity;

    // Ground Detection
    public LayerMask groundLayer;
    public Transform groundCheck;
    private bool isGrounded;
    public float groundCheckDistance = 0.1f;

    // Wall detection
    public float wallCheckDistance;

    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;
    private bool isTouchingWall;
    public Transform footWallCheck;
    private bool leftFootWallContact;
    private bool rightFootWallContact;

    public Transform midWallCheck;
    private bool leftMidWallContact;
    private bool rightMidWallContact;

    public Transform headWallCheck;
    private bool leftHeadWallContact;
    private bool rightHeadWallContact;

    public Transform mantleWallCheck;
    private bool leftMantle;
    private bool rightMantle;

    // Wall Interactions
    private bool isWallSliding;

    // Animation Handling
    private Animator animator;
    private string currentState;
    private SpriteRenderer spriteRenderer;

    // Animation States
    const string player_idle = "player_idle";
    const string player_idle_transition1 = "player_idle_transition1";
    const string player_walk = "player_walk";
    const string player_run = "player_run";
    const string player_jump_rise = "player_jump_rise";
    const string player_jump_apex = "player_jump_apex";
    const string player_jump_fall = "player_jump_fall";
    const string player_dash_loop = "player_dash_loop";

    #endregion

    #region UnityMethods

    void Start(){ 

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        playerCollider = GetComponent<Collider2D>(); // Cache it here

        if (playerCollider == null) {
            Debug.LogError("PlayerCollider not found!");
        }
    }

    void Update(){

        HandleInput();
        HandleAnimations();
        HandleWallSlide();
    }

    #endregion

    #region Inputs

    private void HandleInput(){

        float moveInput = Input.GetAxisRaw("Horizontal");          
        bool jumpInput = Input.GetButtonDown("Jump");
        bool dashInput = Input.GetButtonDown("Dash");

        if (!isDashing){
            HandleMovement(moveInput);
        }

        if (jumpInput) HandleJump();
        if (dashInput) HandleDash();
    }

    #endregion

    #region Movement
    
    // Intakes the player's input, and directly scales their velocity accordingly
    // Calls Flip() if the player moves in a direction opposite to how they're currently facing
    private void HandleMovement(float moveInput){


        // If no movement input is detected, rapidly deccelerate character
        if (moveInput == 0 && isGrounded && !isDashing) {
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, groundDeceleration * Time.deltaTime), rb.velocity.y);

        }

        else if (moveInput == 0 && !isGrounded && !isDashing){
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, airDeceleration * Time.deltaTime), rb.velocity.y);

        }

        else {
            
            if (isGrounded){
                rb.velocity = new Vector2(moveInput * groundMoveSpeed, rb.velocity.y);
            }

            else{
                rb.velocity = new Vector2(moveInput * airMoveSpeed, rb.velocity.y);
            }
        }
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();

    }

    #endregion

    #region Animations
    
    private void HandleAnimations(){

        movement = new Vector2(rb.velocity.x, rb.velocity.y);

        float absSpeedX = Mathf.Abs(movement.x);

        if (isGrounded){
            if (isDashing) ChangeAnimationState(player_dash_loop);
            else if (absSpeedX >= 0 && absSpeedX < 0.1) ChangeAnimationState(player_idle);
            else if (absSpeedX < 7) ChangeAnimationState(player_walk);
            else if (absSpeedX >= 7) ChangeAnimationState(player_run);
        }

        else{
            if (isDashing) ChangeAnimationState(player_dash_loop);
            else if (movement.y > 0) ChangeAnimationState(player_jump_rise);
            else if (movement.y < 0) ChangeAnimationState(player_jump_fall);
            else if(movement.y == 0) ChangeAnimationState(player_jump_apex);
        }
    }

    private void ChangeAnimationState(string newState){

        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;

    }

    #endregion

    #region Jumping
    
    private void HandleJump(){
       
         if (isGrounded){
            
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // First jump
            canDoubleJump = true; // Reset double jump
         }

        else if (canDoubleJump){

            ChangeAnimationState(player_jump_apex);

            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Double jump
            canDoubleJump = false; // Disable double jump
        }
    }

    #endregion

    #region Collision & Detection
    
    private void FixedUpdate(){

        if (playerCollider != null){

            // Get the player's collider dimensions
            float width = playerCollider.bounds.size.x;
            float height = playerCollider.bounds.size.y;

            // Ground detection raycast positions
            Vector2 leftGroundRaycast = new Vector2(groundCheck.position.x - (width / 2) + (width / 10), groundCheck.position.y);
            Vector2 centerGroundRaycast = new Vector2(groundCheck.position.x, groundCheck.position.y);
            Vector2 rightGroundRaycast = new Vector2(groundCheck.position.x + (width / 2) - (width / 10), groundCheck.position.y);

            // Wall detection raycast positions
            Vector2 leftFootWallRaycast = new Vector2(footWallCheck.position.x - (width / 2) + (width / 10), footWallCheck.position.y);
            Vector2 leftMidWallRaycast = new Vector2(midWallCheck.position.x - (width / 2) + (width / 10), midWallCheck.position.y);
            Vector2 leftHeadWallRaycast = new Vector2(headWallCheck.position.x - (width / 2) + (width / 10), headWallCheck.position.y);
            Vector2 leftMantleRaycast = new Vector2(mantleWallCheck.position.x - (width / 2) + (width / 10), mantleWallCheck.position.y);

            Vector2 rightFootWallRaycast = new Vector2(footWallCheck.position.x + (width / 2) - (width / 10), footWallCheck.position.y);
            Vector2 rightMidWallRaycast = new Vector2(midWallCheck.position.x + (width / 2) - (width / 10), midWallCheck.position.y);
            Vector2 rightHeadWallRaycast = new Vector2(headWallCheck.position.x + (width / 2) - (width / 10), headWallCheck.position.y);
            Vector2 rightMantleRaycast = new Vector2(mantleWallCheck.position.x + (width / 2) - (width / 10), mantleWallCheck.position.y);


            // Spawning ground detection raycasts
            RaycastHit2D leftGroundHit = Physics2D.Raycast(leftGroundRaycast, Vector2.down, groundCheckDistance, groundLayer);
            RaycastHit2D centerGroundHit = Physics2D.Raycast(centerGroundRaycast, Vector2.down, groundCheckDistance, groundLayer);
            RaycastHit2D rightGroundHit = Physics2D.Raycast(rightGroundRaycast, Vector2.down, groundCheckDistance, groundLayer);

            // Spawning wall detection raycasts
            RaycastHit2D leftFootWallHit = Physics2D.Raycast(leftFootWallRaycast, Vector2.left, wallCheckDistance, groundLayer);
            RaycastHit2D leftMidWallHit = Physics2D.Raycast(leftMidWallRaycast, Vector2.left, wallCheckDistance, groundLayer);
            RaycastHit2D leftHeadWallHit = Physics2D.Raycast(leftHeadWallRaycast, Vector2.left, wallCheckDistance, groundLayer);
            RaycastHit2D leftMantleWallHit = Physics2D.Raycast(leftMantleRaycast, Vector2.left, wallCheckDistance, groundLayer);

            RaycastHit2D rightFootWallHit = Physics2D.Raycast(rightFootWallRaycast, Vector2.right, wallCheckDistance, groundLayer);
            RaycastHit2D rightMidWallHit = Physics2D.Raycast(rightMidWallRaycast, Vector2.right, wallCheckDistance, groundLayer);
            RaycastHit2D rightHeadWallHit = Physics2D.Raycast(rightHeadWallRaycast, Vector2.right, wallCheckDistance, groundLayer);
            RaycastHit2D rightMantleWallHit = Physics2D.Raycast(rightMantleRaycast, Vector2.right, wallCheckDistance, groundLayer);


            // Check if any of the raycasts hit the ground
            isGrounded = leftGroundHit.collider != null || centerGroundHit.collider != null || rightGroundHit.collider != null;

            // Checking which wall detection raycasts hit a wall
            
            leftFootWallContact = leftFootWallHit.collider != null;
            leftMidWallContact = leftMidWallHit.collider != null;
            leftHeadWallContact = leftHeadWallHit.collider != null;
            leftMantle = leftMantleWallHit.collider != null;

            rightFootWallContact = rightFootWallHit.collider != null;
            rightMidWallContact = rightMidWallHit.collider != null;
            rightHeadWallContact = rightHeadWallHit.collider != null;
            rightMantle = rightMantleWallHit.collider != null;

            if (leftFootWallContact || leftMidWallContact || leftHeadWallContact){
                isTouchingWallLeft = true;
                isTouchingWall = true;
            }
            else if (rightFootWallContact || rightMidWallContact || rightHeadWallContact){
                isTouchingWallRight = true;
                isTouchingWall = true;
            }
            else {
                isTouchingWallLeft = false;
                isTouchingWallRight = false;
                isTouchingWall = false;
            }
            
            // Debug the ground raycasts in the Scene view for troubleshooting
            Debug.DrawRay(leftGroundRaycast, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
            Debug.DrawRay(centerGroundRaycast, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
            Debug.DrawRay(rightGroundRaycast, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

            // Debug wall raycasts
            Debug.DrawRay(leftFootWallRaycast, Vector2.left * wallCheckDistance, leftFootWallContact ? Color.green : Color.red);
            Debug.DrawRay(leftMidWallRaycast, Vector2.left * wallCheckDistance, leftMidWallContact ? Color.green : Color.red);
            Debug.DrawRay(leftHeadWallRaycast, Vector2.left * wallCheckDistance, leftHeadWallContact ? Color.green : Color.red);
            Debug.DrawRay(leftMantleRaycast, Vector2.left * wallCheckDistance, leftMantle ? Color.green : Color.red);

            Debug.DrawRay(rightFootWallRaycast, Vector2.right * wallCheckDistance, rightFootWallContact ? Color.green : Color.red);
            Debug.DrawRay(rightMidWallRaycast, Vector2.right * wallCheckDistance, rightMidWallContact ? Color.green : Color.red);
            Debug.DrawRay(rightHeadWallRaycast, Vector2.right * wallCheckDistance, rightHeadWallContact ? Color.green : Color.red);
            Debug.DrawRay(rightMantleRaycast, Vector2.right * wallCheckDistance, rightMantle ? Color.green : Color.red);

        }

    }

    
    #endregion

    #region Dashing

    private void HandleDash(){

        if (isDashing || !canDash) return;

        preDashVelocity = rb.velocity;

        StartCoroutine(PerformDash());  
    }

    private IEnumerator PerformDash(){

        canDash = false;
        isDashing = true;

        float preDashVelocity = rb.velocity.x;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        if (facingRight){
            rb.velocity = new Vector2(dashSpeed, 0f);        
        }
        else{
            rb.velocity = new Vector2(-dashSpeed, 0f);
        }

        yield return new WaitForSeconds(dashTime);

        rb.velocity = new Vector2(preDashVelocity, 0f);
        rb.gravityScale = originalGravity;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    #endregion

    #region Wall Slide

    private void HandleWallSlide() {

        float moveInput = Input.GetAxis("Horizontal");

        if (isTouchingWall && moveInput != 0) {

            isWallSliding = true;

            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);

        }
    }

    #endregion

    #region HelperMethods
    
    private void Flip()
    {
        facingRight = !facingRight; // Toggle the direction

        // Rotate the player on the Y-axis to face the direction of movement
        float rotationY = facingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    #endregion
}