using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Components
    private Rigidbody2D playerRb;
    private Rigidbody2D spiritRb;
    private GameObject debuggerPrefab;
    private CapsuleCollider2D playerCollider;
    private DistanceJoint2D circularJoint;
    private SpiritMovement spiritMovement;
    
    [Header("Action Variables")]
    [Space]
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
    public float swingJumpAmplification = 2f;
    public float spiritDelay = 0.5f;
    public float circularForce = 10f;
    public float minCircularLength = 4f;
    [Range(0, 1)] public float circularTransactionSmoothing = 0.95f;
    public float controldelay;

    [Header("Collision Setup")]
    [Space]
    public Vector2 bottomOffset = new Vector2(0, -0.1f);
    public Vector2 rightOffset = new Vector2(0.1f, 0);
    public Vector2 leftOffset = new Vector2(-0.1f, 0);
    public float groundCheckRadius = 0.1f;
    public float wallCheckRadius = 0.1f;

    //State Variables
    private Vector2 anchorPosition;
    private bool mouse_fireDown;
    private bool mouse_fire;
    private bool key_jump;
    private bool key_jumpDown;
    private bool isJumping;
    private bool isGrounded;
    private bool isJumpingPast;
    private bool jumpAvailable;
    private bool wallKick;
    private bool wallKickPast;
    private bool dashing;
    private bool dashAvailable;
    private bool key_lockDown;
    private bool key_lock;
    private bool spirit_lock;
    private bool circularMovement;
    private bool flying;
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
    private float ropeLength;
    private float flyTimer;
    private int numSegments;
    private int wallKickDirection;
    private int curSegmment;
    private int wallInfo;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        GameObject spirit = GameObject.Find("Spirit");
        debuggerPrefab = (GameObject) Resources.Load("Prefabs/Debugger");
        spiritRb = spirit.GetComponent<Rigidbody2D>();
        spiritMovement = spirit.GetComponent<SpiritMovement>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        circularJoint = GetComponent<DistanceJoint2D>();
        circularJoint.enableCollision = true;
        ropeLength = GameObject.Find("Rope").GetComponent<Rope>().ropeLength;
        numSegments = GameObject.Find("Rope").GetComponent<Rope>().numSegments;
        curSegmment = -1;
        wallKick = false;
        dashAvailable = true;
        jumpAvailable = true;
        spirit_lock = false;
        circularMovement = false;
        jumpSpeed = jumpHeight / jumpDuration;
        dashSpeed = dashLength / dashDuration;
        wallKickDuration = wallKickAmplitude / maxMoveSpeed;
        wallKickJumpSpeed = wallKickJumpHeight / wallKickDuration;
        Physics2D.IgnoreLayerCollision(9, 9);
    }

    void FixedUpdate()
    {
        Vector2 velocity = playerRb.velocity;

        if(!flying) {
            velocity.x = maxMoveSpeed * userInputMoveDirection;
        }

        if(isJumping) {
            velocity.y = jumpSpeed;
        } else if(isJumpingPast && !isJumping) {
            velocity.y *= jumpStopSmoothing;
        }

        if(dashing) {
            velocity.y = 0;
            playerRb.AddForce(Vector2.up * Physics2D.gravity.magnitude * playerRb.gravityScale);
            velocity.x = dashDirection * dashSpeed;
        }

        if(circularMovement && !isGrounded) {
            velocity.x = playerRb.velocity.x;
            velocity.y = playerRb.velocity.y;
            if((anchorPosition - playerRb.position).magnitude >= circularJoint.distance * circularTransactionSmoothing) {
                circularJoint.enabled = wallInfo == 0;
            } else {
                circularJoint.enabled = false;
            }
            if(userInputMoveDirection != 0) {
                playerRb.AddForce(Vector2.right * userInputMoveDirection * circularForce);
            }
        } else {
            circularJoint.enabled = false;
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
            playerRb.AddForce(Vector2.up * Physics2D.gravity.magnitude * playerRb.gravityScale);
            if(velocity.y <= -wallSlideSpeed) {
                velocity.y = -wallSlideSpeed;
            } else {
                velocity.y *= wallSlowdownSmoothing;
            }
        }

        playerRb.velocity = velocity;

        isJumpingPast = isJumping;
        wallKickPast = wallKick;
    }

    void Update()
    { 
        //Update states
        GroundCheck();
        WallCheck();
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

        if(!jumpAvailable && !isJumping) {
            jumpAvailable = isGrounded || (wallInfo != 0 && objectMoveDirection == wallInfo);
        }

        if(flying) {
            flyTimer += Time.deltaTime;
            if(flyTimer > controldelay) {
                flying = !isGrounded || wallInfo != 0 || userInputMoveDirection != 0;
            }
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
            spiritMovement.Lock();
            spiritMovement.EmptyQueue();
            circularJoint.connectedAnchor = spiritRb.position;
            anchorPosition = spiritRb.position;
            circularJoint.distance = ropeLength;
            spirit_lock = true;
            circularMovement = true;
        } else if(spirit_lock && !key_lock) {
            spiritRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            spiritMovement.UnLock();
            Vector2 pushDirection = (playerRb.position + playerRb.velocity * swingJumpAmplification - spiritRb.position).normalized;
            float pushForce = playerRb.velocity.magnitude * swingJumpAmplification / spiritRb.mass;
            spirit_lock = false;
            flying = true;
            flyTimer = 0f;
            circularJoint.enabled = false;
            circularMovement = false;
            spiritRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse); 
        }
    }

    void GroundCheck() {
        if(isJumping && airTime < 0.1f) {
            isGrounded = false;
        }
        if(playerRb.velocity.y <= 0.1f) {
            int layerMask = LayerMask.GetMask("Ground");
            Vector2 positionToCheck = playerRb.position;
            isGrounded = Physics2D.OverlapCircle(positionToCheck + bottomOffset, groundCheckRadius, layerMask);
        }
    }

    bool IsTouchingWall() {
        return wallInfo != 0;
    }

    void WallCheck() {
        int layerMask = LayerMask.GetMask("Wall");
        Vector2 positionToCheck = playerRb.position;
        bool wallLeft = Physics2D.OverlapCircle(positionToCheck + leftOffset, wallCheckRadius, layerMask);
        bool wallRight = Physics2D.OverlapCircle(positionToCheck + rightOffset, wallCheckRadius, layerMask);
        wallInfo =  wallLeft ? -1:
                    wallRight ? 1:
                                0;
    }

    /*
    if  (   (segmentID > curSegmment) ||
                (!circularMovement ||
                newDistance < (anchorPosition - playerRb.position).magnitude &&
                newDistance > minCircularLength)
            ) {
    */
    public void SetCircularAnchor(Vector2 position, int segmentID) {
        float newDistance = (position - playerRb.position).magnitude;
        if  (   (!key_lock && (segmentID > curSegmment)) ||
                (RayCastControl(position))
            ) {
            curSegmment = segmentID;
            circularMovement = true;
            anchorPosition = position;
            circularJoint.connectedAnchor = position;
            circularJoint.distance = ropeLength * (1 - segmentID / numSegments);
        } else {
            circularJoint.distance = ropeLength;
        }
    }

    public bool RayCastControl(Vector2 position) {
        int layerMask = LayerMask.GetMask("Wall", "Ground");
        Collider2D collider = Physics2D.OverlapCircle(position, 0.5f, layerMask);
        if(collider != null) {
            float center = position.x - collider.transform.position.x;
            float angle = Vector2.SignedAngle(position - playerRb.position, spiritRb.position - position);
            return center * angle > 0;
        }
        return false;
    } 

    public void SetCircularMovement(bool circularMovement) {
        if(!circularMovement) {
            this.circularJoint.connectedAnchor = spiritRb.position;
            this.circularJoint.distance = ropeLength;
        }
        this.circularMovement = circularMovement || spirit_lock;
        this.circularJoint.enabled = this.circularMovement;
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
    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        Vector2 positionToDraw = transform.position;

        Gizmos.DrawWireSphere(positionToDraw  + bottomOffset, groundCheckRadius);
        Gizmos.DrawWireSphere(positionToDraw + rightOffset, wallCheckRadius);
        Gizmos.DrawWireSphere(positionToDraw + leftOffset, wallCheckRadius);
    }

}