using Cinemachine;
using System;
using UnityEngine;

public class PlayerMovementAndJump : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera cam;

    [Header("Movement settings")]
    [SerializeField] private float MovementSpeed;
    [SerializeField] private float DashSpeed;
    [SerializeField] private float DashDistance;

    [Header("Player Jump Settings")]
    [SerializeField] private float JumpPower;
    private float MaxAmountOfJumps = 2;
    private float JumpsAvailable;
    private float fallingMultiplier;
    private float lowJumpMultiplier;

    [Header("Collisins with walls and ground")]
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private LayerMask WallLayer;

    // Offsets for wall and ground detection and radius of the offsets
    private Vector2 BottomOffset = new Vector2(0, -0.4f);
    private Vector2 WallOffset = new Vector2(0.11f, -0.15f);
    private float collisionRadius = 0.09f;

    [Header("Player bools for conditionds")]
    private bool LookingRight = true;
    public bool CanMove = true;
    private bool IsWallSliding = false;

    // Timers and Cooldowns
    private float SlideBlock;
    private float DashCooldown;

    [Header("Prefabs")]
    [SerializeField] private GameObject AirHikeEffect; // Effect for jump in air
    private GameObject AirHikeTemp;

    // Player and other important stuff
    private Rigidbody2D player;
    private Animator anim;
    private SpriteRenderer sprite;
    private CinemachineFramingTransposer transposer;
    private AudioManager audioManager;
    private PlayerAttack playerAttack;

    // Input
    private float HorizontalInput;

    private void Awake()
    {
        // Get all the components
        player = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        transposer = cam.GetComponentInChildren<CinemachineFramingTransposer>();
        audioManager = FindObjectOfType<AudioManager>();
        playerAttack = GetComponent<PlayerAttack>();

        //Set gravity multipliers for falling and for low jump
        fallingMultiplier = Physics2D.gravity.y * (2 - 1);
        lowJumpMultiplier = Physics2D.gravity.y * (10 - 1);
    }

    private void Update()
    {
        // Timers
        SlideBlock += Time.deltaTime;
        DashCooldown += Time.deltaTime;

        // Bools
        CanMove = CheckIfPlayerCanMove();

        // Input
        HorizontalInput = FetchPlayerInput();

        // Player Orientation
        if (player.velocity.x > 0 && !LookingRight) FlipPlayerSprite();
        else if (player.velocity.x < 0 && LookingRight) FlipPlayerSprite();

        // Player Movement
        if ((HorizontalInput == 0) && IsOnGround() && !IsOnWall() && CanMove) StopMovement();
        else if ((HorizontalInput > 0f) && CanMove) MovePlayerRight();
        else if ((HorizontalInput < 0f) && CanMove) MovePlayerLeft();
        //Stop walking Animation
        if ( (IsOnGround() && player.velocity.x == 0 && CanMove) || !IsOnGround()) audioManager.Stop("PlayerWalkStone");

        // Dash
        if (Input.GetKeyDown(KeyCode.C) && DashCooldown > (DashDistance + .3f)) Dash();

        // Regular jump
        if (Input.GetButtonDown("Jump") && IsOnGround() && DashCooldown > .3f) Jump("NormalJump");
        else if (Input.GetButtonDown("Jump") && !IsOnGround() && !IsWallSliding && DashCooldown > .3f) Jump("DoubleJump");

        // Wall sliding and Wall Jumping
        if (IsOnWall() && HorizontalInput != 0) WallSlide();
        else ExitWallSlide();

        // Low jump and faster falling
        if (player.velocity.y < 0)
        {
            player.velocity += Vector2.up * fallingMultiplier * Time.deltaTime;
            anim.SetBool("jumping", false);
            anim.SetBool("falling", true);
        }
        else if (player.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            player.velocity += Vector2.up * lowJumpMultiplier * Time.deltaTime;
        }

        // Animations cause I don't know what I'm doing
        UpdateAnimations();       
    }

    public void FlipPlayerSprite() // Flip the sprite depending on where you are facing
    {
        sprite.flipX = LookingRight;
        LookingRight = !LookingRight;
        WallOffset.x *= -1;
        playerAttack.attackHitbox.x *= -1;
    }

    private void UpdateAnimations()
    {
        if (IsOnGround()) anim.SetBool("falling", false);
        if (!IsOnGround()) anim.SetBool("running", false);
        if (anim.GetBool("dashing") == true)
        {
            anim.SetBool("running", false);
            anim.SetBool("jumping", false);
            anim.SetBool("falling", false);
        }
        if (anim.GetBool("onWall") == true)
        {
            anim.SetBool("jumping", false);
            anim.SetBool("falling", false);
        }
        if (player.velocity.y > 0) anim.SetBool("jumping", true);
    }

    private bool CheckIfPlayerCanMove()
    {
        if (SlideBlock < .2) return false; // Prevents bugs when wall jumping, blocks movement for a moment after a wall jump.
        else if (DashCooldown < DashDistance) return false; // Needed for dash to work
        else return true;
    }

    private void WallSlide() // Slowly slide down a wall when holding input towards the wall
    {
        IsWallSliding = true;
        anim.SetBool("onWall", true);

        if (Input.GetButtonDown("Jump") && SlideBlock > .2) Jump("WallJump");

        if ((LookingRight && HorizontalInput > 0 && SlideBlock > .2) || (!LookingRight && HorizontalInput < 0 && SlideBlock > .2))
        {
            player.velocity = new Vector2(player.velocity.x, -.5f);
            audioManager.Play("PlayerWallSlide");
        }
    }

    private void ExitWallSlide()
    {
        anim.SetBool("onWall", false);
        IsWallSliding = false;
        audioManager.Stop("PlayerWallSlide");
    }

    private void StopMovement() // Prevents player from sliding on ground after sliding down an edge
    {
        player.velocity = new Vector2(0, player.velocity.y);
        anim.SetBool("running", false);
    }

    private void MovePlayerRight() // Moves player to the right
    {
        if (IsOnGround()) anim.SetBool("running", true); // Set animation
        transposer.m_TrackedObjectOffset = new Vector3(.5f, 0, 0); // change tracking to look a little bit forewards to where the player is facing
        if (LookingRight == false) FlipPlayerSprite(); // flip the sprite
        audioManager.Play("PlayerWalkStone"); // Play walking sound
        player.velocity = new Vector2(HorizontalInput * MovementSpeed, player.velocity.y); // Do the move
    }

    private void MovePlayerLeft() // Moves player to the left
    {
        if (IsOnGround()) anim.SetBool("running", true); // Set animation
        transposer.m_TrackedObjectOffset = new Vector3(-.5f, 0, 0); // change tracking to look a little bit forewards to where the player is facing
        if (LookingRight == true) FlipPlayerSprite(); // flip the sprite
        audioManager.Play("PlayerWalkStone"); // Play walking sound
        player.velocity = new Vector2(HorizontalInput * MovementSpeed, player.velocity.y); // Do the move
    }

    private void Dash()
    {
        DashCooldown = 0;
        anim.SetBool("dashing", true);
        player.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        audioManager.Play("PlayerDash");
        if ((LookingRight && !IsWallSliding) || (!LookingRight & IsWallSliding)) player.velocity = new Vector2(DashSpeed, 0);
        else if ((!LookingRight && !IsWallSliding) || (LookingRight & IsWallSliding)) player.velocity = new Vector2(-DashSpeed, 0);
        Invoke("EndDash", .3f);
    }

    private void EndDash()
    {
        player.constraints = RigidbodyConstraints2D.FreezeRotation;
        anim.SetBool("dashing", false);
        player.velocity = new Vector2(Mathf.Sign(player.velocity.x) * MovementSpeed ,player.velocity.y);
    }

    private void Jump(string TypeOfJump) // Handles Regular Jump, Double Jump and Wall Jump
    {
        if (TypeOfJump == "NormalJump" && JumpsAvailable > 0)
        {
            player.velocity = new Vector2(player.velocity.x, JumpPower);
            anim.SetBool("jumping", true);
            JumpsAvailable = 1;
            audioManager.Play("PlayerJump");
        }
        else if (TypeOfJump == "DoubleJump" && JumpsAvailable > 0)
        {
            player.velocity = new Vector2(player.velocity.x, JumpPower);
            anim.SetBool("jumping", true);
            JumpsAvailable = 0;
            AirHikeTemp = Instantiate(AirHikeEffect, new Vector2(player.position.x + BottomOffset.x, player.position.y), Quaternion.identity);
            Destroy(AirHikeTemp, .6f);
        }
        else if (TypeOfJump == "WallJump")
        {
            anim.SetBool("jumping", true);
            SlideBlock = 0;
            if (LookingRight) player.velocity = new Vector2((MovementSpeed * -1.2f), JumpPower);
            else player.velocity = new Vector2((MovementSpeed * 1.2f), JumpPower);
            audioManager.Play("PlayerJump");
        }
    }

    private float FetchPlayerInput() // Returns horizontal input
    {
        return Input.GetAxis("horizontal");
    }


    private bool IsOnGround() // Check if on ground
    {
        return Physics2D.OverlapCircle((Vector2)transform.position + BottomOffset, collisionRadius, GroundLayer);
    }

    private bool IsOnWall() // Check if on a wall
    {
        return (Physics2D.OverlapCircle((Vector2)transform.position + WallOffset, collisionRadius, WallLayer) && !IsOnGround());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 && !IsOnWall()) // Resets jumps when landing on ground
        {
            JumpsAvailable = MaxAmountOfJumps;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere((Vector2)transform.position + BottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + WallOffset, collisionRadius);
    }
}