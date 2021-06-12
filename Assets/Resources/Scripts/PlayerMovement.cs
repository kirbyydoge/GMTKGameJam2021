using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Components
    private Rigidbody2D playerRb;
    private Rigidbody2D spiritRb;
    private CapsuleCollider2D playerCollider;
    private SpiritMovement spiritMovement;
    
    //Action Variables
    public float maxMoveSpeed = 10f;
    [Range(0, 1)] public float stopSmoothing = 0f;
    public float jumpHeight = 5f;
    public float jumpDuration = 0.2f;
    [Range(0, 1)] public float jumpStopSmoothing = 0.4f;
    public float wallKickAmplitude = 3f;
    public float wallKickJumpHeight = 10f;
    public float wallSlideSpeed = 0f;
    [Range(0, 1)] public float wallSlowdownSmoothing = 0f;
    public float dashLength = 5f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.1f;
    public float swingForce = 10f;
    public float spiritDelay = 0.5f;

    //State Variables
    private bool mouse_fireDown;
    private bool mouse_fire;
    private bool key_jump;
    private bool key_jumpDown;
    private bool isJumping;
    private bool isJumpingPast;
    private bool jumpAvailable;
    private bool wallKick;
    private bool wallKickPast;
    private bool dashing;
    private bool dashAvailable;
    private bool key_lockDown;
    private bool key_lock;
    private bool spirit_lock;
    private float dashDirection;
    private float userInputMoveDirection;
    private float objectMoveDirection;
    private float wallKickTime;
    private float wallKickDuration;
    private float wallKickJumpSpeed;
    private float jumpSpeed;
    private float airTime;
    private float dashTime;
    private float dashSpeed;
    private int wallKickDirection;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        GameObject spirit = GameObject.Find("Spirit");
        spiritRb = spirit.GetComponent<Rigidbody2D>();
        spiritMovement = spirit.GetComponent<SpiritMovement>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        wallKick = false;
        dashAvailable = true;
        jumpAvailable = true;
        spirit_lock = false;
        jumpSpeed = jumpHeight / jumpDuration;
        dashSpeed = dashLength / dashDuration;
        wallKickDuration = wallKickAmplitude / maxMoveSpeed;
        wallKickJumpSpeed = wallKickJumpHeight / wallKickDuration;
        Physics2D.IgnoreLayerCollision(9, 9);
    }

    void FixedUpdate()
    {
        int wallInfo = WallCheck();

        Vector2 velocity = playerRb.velocity;

        velocity.x = maxMoveSpeed * userInputMoveDirection;

        if(isJumping) {
            velocity.y = jumpSpeed;
        } else if(isJumpingPast) {
            velocity.y *= jumpStopSmoothing;
        }

        if(wallKick) {
            if(wallKickTime < wallKickDuration / 2) {
                velocity.y = wallKickJumpSpeed;
                velocity.x = maxMoveSpeed * wallKickDirection;
            } else if(userInputMoveDirection != wallKickDuration) {
                velocity.y = wallKickJumpSpeed;
                velocity.x = maxMoveSpeed * userInputMoveDirection;
            } else {
                wallKick = false;
                wallKickTime = wallKickDuration;
            }
        } else if(wallKickPast) {
            velocity.y *= jumpStopSmoothing;
        }

        if(wallInfo * userInputMoveDirection > 0) {
            if(velocity.y <= -wallSlideSpeed) {
                velocity.y = -wallSlideSpeed;
            } else {
                velocity.y *= wallSlowdownSmoothing;
            }
        }

        if(dashing) {
            velocity.y = 0;
            playerRb.AddForce(Vector2.up * Physics2D.gravity.magnitude * playerRb.gravityScale);
            velocity.x = dashDirection * dashSpeed;
        }

        playerRb.velocity = velocity;

        isJumpingPast = isJumping;
        wallKickPast = wallKick;
    }

    void Update()
    { 
        //Update states
        bool isGrounded = IsGrounded();
        int wallInfo = WallCheck();
        objectMoveDirection =   playerRb.velocity.x > 0 ? 1
                            :   playerRb.velocity.x < 0 ? -1
                            :   0;

        if(dashing || !dashAvailable) {
            dashTime += Time.deltaTime;
            if(dashTime > dashDuration) {
                dashing = false;
            }
            if(dashTime > dashDuration + dashCooldown) {
                dashAvailable = true;
            }
         }

        if(isJumping) {
            airTime += Time.deltaTime;
            if((airTime > jumpDuration)
                || ((isGrounded || wallInfo != 0)
                    && playerRb.velocity.y < 0)) {
                isJumping = false;
            }
        }

        if(wallKick) {
            wallKickTime += Time.deltaTime;
            if (wallKickTime > wallKickDuration) {
                wallKick = false;
            }
        }

        if(!jumpAvailable) {
            jumpAvailable = (isGrounded && playerRb.velocity.y < 0)
                            || (wallInfo != 0 && objectMoveDirection == wallInfo);
        }

        //Handle Input and Determine Next State

        key_jumpDown = Input.GetKeyDown("space");
        key_jump = Input.GetKey("space");
        mouse_fireDown = Input.GetMouseButtonDown(0);
        mouse_fire = Input.GetMouseButton(0);
        bool key_dashDown = Input.GetKeyDown(KeyCode.LeftShift);
        key_lockDown = Input.GetKeyDown("k");
        key_lock = Input.GetKey("k");
        userInputMoveDirection = Input.GetAxisRaw("Horizontal");

        bool[] inputs = new bool[3];
        inputs[0] = userInputMoveDirection > 0;
        inputs[1] = userInputMoveDirection < 0;
        inputs[2] = key_jump;

        if(!key_lock && (userInputMoveDirection != 0 || key_jump)) {
            spiritMovement.QueueInput(inputs, spiritDelay);
        }

        if(dashAvailable && key_dashDown) {
            dashing = true;
            dashAvailable = false;
            dashDirection = userInputMoveDirection;
            dashTime = 0;
        } else if(!isGrounded && wallInfo != 0 && key_jumpDown) {
            wallKick = true;
            jumpAvailable = false;
            wallKickDirection = -wallInfo;
            wallKickTime = 0;
        } else if(jumpAvailable && key_jumpDown) {
            isJumping = true;
            jumpAvailable = false;
            airTime = 0;
        } else if(isJumping && !wallKick && !key_jump) {
            isJumping = false;
        }

        if(key_lockDown) {
            spiritRb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            spiritMovement.EmptyQueue();
            spirit_lock = true;
        } else if(spirit_lock && !key_lock) {
            spiritRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            spirit_lock = false;
        }
    }

    bool IsGrounded() {
        if(playerRb.velocity.y <= 0) {
            int layerMask = LayerMask.GetMask("Ground");
            RaycastHit2D[] hits;
            Vector2 positionToCheck = transform.position;
            Vector2 leftCheck = positionToCheck;
            leftCheck.x -= playerCollider.size.x / 2;
            Vector2 rightCheck = positionToCheck;
            rightCheck.x += playerCollider.size.x / 2;
            float checkDistance = playerCollider.size.y / 2 + 0.05f;
            hits = Physics2D.RaycastAll(leftCheck, new Vector2 (0, -1), checkDistance, layerMask);
            if(hits.Length > 0)
                return true;
            hits = Physics2D.RaycastAll(rightCheck, new Vector2 (0, -1), checkDistance, layerMask);
            return hits.Length > 0;
        }
        return false;
    }

    bool IsTouchingWall() {
        return WallCheck() != 0;
    }

    int WallCheck() {
        int layerMask = LayerMask.GetMask("Wall");
        RaycastHit2D[] hits;
        Vector2 positionToCheck = playerRb.position;
        Vector2 upCheck = positionToCheck;
        upCheck.y += (playerCollider.size.y / 2 + 0.02f);
        Vector2 downCheck = positionToCheck;
        downCheck.y -= (playerCollider.size.y / 2 + 0.02f);
        float checkDistance = playerCollider.size.x / 2 + 0.02f;
        hits = Physics2D.RaycastAll(upCheck, new Vector2 (1, 0), checkDistance, layerMask);
        if(hits.Length > 0)
            return 1;   //Wall to the right
        hits = Physics2D.RaycastAll(downCheck, new Vector2 (1, 0), checkDistance, layerMask);
        if(hits.Length > 0)
            return 1;   //Wall to the right
        hits = Physics2D.RaycastAll(upCheck, new Vector2 (-1, 0), checkDistance, layerMask);
        if(hits.Length > 0)
            return -1;  //Wall to the left
        hits = Physics2D.RaycastAll(downCheck, new Vector2 (-1, 0), checkDistance, layerMask);
        if(hits.Length > 0)
            return -1;  //Wall to the left
        return 0;   //No wall
    }

    void RemoveHits(RaycastHit2D[] hits) {
        for(int i = 0; i < hits.Length; i++) {
            RaycastHit2D hit = hits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();

            if (rend)
            {
                rend.material.shader = Shader.Find("Transparent/Diffuse");
                Color tempColor = rend.material.color;
                tempColor.a = 0.3F;
                rend.material.color = tempColor;
            }
        }
    }

}