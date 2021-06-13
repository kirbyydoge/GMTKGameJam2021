using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMovement : MonoBehaviour
{
    //Components
    private Rigidbody2D playerRb;
    private Rigidbody2D spiritRb;
    private GameObject debuggerPrefab;
    private CapsuleCollider2D playerCollider;
    private DistanceJoint2D circularJoint;
    
    
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
    public float spiritDelay = 0.5f;
    public float circularForce = 10f;
    public float minCircularLength = 4f;
    [Range(0, 1)] public float circularTransactionSmoothing = 0.95f;

    [Header("Collision Setup")]
    [Space]
    public Vector2 bottomOffset = new Vector2(0, -0.1f);
    public Vector2 rightOffset = new Vector2(0.1f, 0);
    public Vector2 leftOffset = new Vector2(-0.1f, 0);
    public float groundCheckRadius = 0.1f;
    public float wallCheckRadius = 0.1f;

    //State Variables
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
    private bool key_dashDown;
    private bool circularMovement;
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
    private Vector2 anchorPosition;
    private int curSegmment;
    private List<float> delayQ;
    private List<InputVars> inputQ;
    private bool execute;
    private InputVars curInput;
    private bool isLocked;
    private bool flying;
    private Vector2 lastCheck;
    private int wallInfo;


    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        GameObject spirit = GameObject.Find("Player");
        debuggerPrefab = (GameObject) Resources.Load("Prefabs/Debugger");
        spiritRb = spirit.GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        circularJoint = GetComponent<DistanceJoint2D>();
        circularJoint.enableCollision = true;
        ropeLength = GameObject.Find("Rope").GetComponent<Rope>().ropeLength;
        numSegments = GameObject.Find("Rope").GetComponent<Rope>().numSegments;
        curSegmment = numSegments;
        wallKick = false;
        dashAvailable = true;
        jumpAvailable = false;
        circularMovement = false;
        isLocked = false;
        jumpSpeed = jumpHeight / jumpDuration;
        dashSpeed = dashLength / dashDuration;
        wallKickDuration = wallKickAmplitude / maxMoveSpeed;
        wallKickJumpSpeed = wallKickJumpHeight / wallKickDuration;
        Physics2D.IgnoreLayerCollision(9, 9);
        delayQ = new List<float>();
        inputQ = new List<InputVars>();
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
            float spiritDistance = (anchorPosition - playerRb.position).magnitude;
            if(spiritDistance >= circularJoint.distance * circularTransactionSmoothing) {
                velocity.x = playerRb.velocity.x;
                velocity.y = playerRb.velocity.y;
                if(wallInfo == 0) {
                    circularJoint.distance = spiritDistance;
                    circularJoint.enabled = true;
                }
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

        if((playerRb.position - spiritRb.position).magnitude  > ropeLength * 1.1f) {
            velocity = Vector2.zero;
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

        if(flying) {
            flyTimer += Time.deltaTime;
            if(flyTimer > spiritDelay) {
                flying = !isGrounded || wallInfo != 0 || userInputMoveDirection != 0;
            }
        }

        if(!jumpAvailable && !isJumping) {
            jumpAvailable = isGrounded || (wallInfo != 0 && objectMoveDirection == wallInfo);
        }

        //Handle Input and Determine Next State

        UpdateInputVars();

        InputVars inputVars = new InputVars();

        inputVars.key_jumpDown = Input.GetKeyDown("space");
        inputVars.key_jump = Input.GetKey("space");
        inputVars.key_dashDown = Input.GetKeyDown(KeyCode.LeftShift);
        inputVars.key_lockDown = Input.GetKeyDown("k");
        inputVars.key_lock = Input.GetKey("k");
        inputVars.userInputMoveDirection = Input.GetAxisRaw("Horizontal");

        UnpackInput();

        QueueInputVars(inputVars, spiritDelay);

        if(dashAvailable && key_dashDown) {
            dashing = true;
            dashAvailable = false;
            dashDirection = userInputMoveDirection;
            dashTime = 0;
        } else if(!isGrounded && wallInfo != 0 && key_jump) {
            wallKick = true;
            jumpAvailable = false;
            wallKickDirection = -wallInfo;
            wallKickTime = 0;
        } else if(jumpAvailable && key_jump) {
            isJumping = true;
            jumpAvailable = false;
            airTime = 0;
        } else if(isJumping && !wallKick && !key_jump) {
            isJumping = false;
        }
    }

    void GroundCheck() {
        if(isJumping && airTime < 0.1f) {
            isGrounded = false;
        }
        if(playerRb.velocity.y <= 0.1f) {
            int layerMask = LayerMask.GetMask("Ground", "Wall");
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
        if  (   (!key_lock && (segmentID < curSegmment)) ||
                (RayCastControl(position))
            ) {
            curSegmment = segmentID;
            circularMovement = true;
            anchorPosition = position;
            circularJoint.connectedAnchor = position;
            circularJoint.distance = Mathf.Max(ropeLength * (segmentID / numSegments), minCircularLength);
        } else {
            circularJoint.distance = ropeLength;
        }
    }

    public bool RayCastControl(Vector2 position) {
        int layerMask = LayerMask.GetMask("Wall", "Ground");
        lastCheck = position;
        Collider2D collider = Physics2D.OverlapCircle(position, 0.5f, layerMask);
        if(collider != null) {
            float center = position.x - collider.transform.position.x;
            float angle = Vector2.SignedAngle(position - playerRb.position, spiritRb.position - playerRb.position);
            return center * angle > 0;
        }
        return false;
    } 

    public void SetCircularMovement(bool circularMovement) {
        if(!circularMovement) {
            this.circularJoint.connectedAnchor = spiritRb.position;
            this.circularJoint.distance = ropeLength;
        }
        this.curSegmment = numSegments;
        this.circularMovement = circularMovement;
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

    public void EmptyQueue() {
        delayQ.Clear();
        inputQ.Clear();
    }

    public void Lock() {
        wallKick = false;
        dashAvailable = true;
        jumpAvailable = false;
        circularMovement = false;
        this.isLocked = true;
    }

    public void UnLock() {
        this.isLocked = false;
        if(!isGrounded) {
            flyTimer = 0;
            flying = true;
        }
    }

    void UpdateInputVars() {
        for(int i = 0; i < delayQ.Count; i++) {
            delayQ[i] -= Time.deltaTime;
            if(delayQ[i] <= 0) {
                execute = true;
                curInput = inputQ[i];
                RemoveIndex(i);
                i--;
            }
        }
    }

    void UnpackInput() {
        if(execute && !isLocked) {
            key_jumpDown = curInput.key_jumpDown;
            key_jump = curInput.key_jump;
            key_dashDown = curInput.key_dashDown;
            key_lockDown = curInput.key_lockDown;
            userInputMoveDirection = curInput.userInputMoveDirection;
        } else {
            key_jumpDown = false;
            key_jump = false;
            key_dashDown = false;
            key_lockDown = false;
            userInputMoveDirection = 0;
        }
    }

    void QueueInputVars(InputVars inputVar, float delay) {
        if(!isLocked) {
            delayQ.Add(delay);
            inputQ.Add(inputVar);
        }
    }

    void RemoveIndex(int i) {
        delayQ.RemoveAt(i);
        inputQ.RemoveAt(i);
    }
    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        Vector2 positionToDraw = transform.position;

        Gizmos.DrawWireSphere(lastCheck, 0.5f);
        Gizmos.DrawWireSphere(positionToDraw  + bottomOffset, groundCheckRadius);
        Gizmos.DrawWireSphere(positionToDraw + rightOffset, wallCheckRadius);
        Gizmos.DrawWireSphere(positionToDraw + leftOffset, wallCheckRadius);
    }

    /*
    private Rigidbody2D spiritRb;
    public float circularForce = 10f;
    private List<bool[]> inputQ;
    private List<float> delayQ;
    private bool[] curInput;
    private bool execute;
    private const int MOVE_RIGHT = 0;
    private const int MOVE_LEFT = 1;
    private const int JUMP = 2;
    private bool circularMovement;
    
    void Start()
    {
        spiritRb = GetComponent<Rigidbody2D>();
        inputQ = new List<bool[]>();
        delayQ = new List<float>();
        circularMovement = false;
    }

    void FixedUpdate() {
        if(execute) {
            Vector2 velocity = spiritRb.velocity;

            if(curInput[MOVE_RIGHT]) {
                velocity.x = 5;
            } else if(curInput[MOVE_LEFT]) {
                velocity.x = -5;
            } else {
                velocity.x = 0;
            }

            if(curInput[JUMP]) {
                velocity.y = 6.45f;
            }

            if(circularMovement) {
                velocity.x = spiritRb.velocity.x;
                velocity.y = spiritRb.velocity.y;
                if(curInput[MOVE_RIGHT]) {
                    spiritRb.AddForce(Vector2.right * circularForce);
                }
                if(curInput[MOVE_LEFT]) {
                    spiritRb.AddForce(Vector2.left * circularForce);
                }
            }

            spiritRb.velocity = velocity;
        }
    }

    void Update()
    {
        execute = false;
        for(int i = 0; i < delayQ.Count; i++) {
            delayQ[i] = delayQ[i] - Time.deltaTime;
            if(delayQ[i] <= 0) {
                execute = true;
                curInput = inputQ[i];
                RemoveInput(i);
                i--;
            }
        }   
    }
    public void SetCircularMovement(bool circularMovement) {
        this.circularMovement = circularMovement;
    }

    public void EmptyQueue() {
        inputQ.Clear();
        delayQ.Clear();
    }

    public void QueueInput(bool[] inputs, float timeToExecute) {
        inputQ.Add(inputs);
        delayQ.Add(timeToExecute);
    }

    public void RemoveInput(int i) {
        inputQ.RemoveAt(i);
        delayQ.RemoveAt(i);
    }
    */
}
public struct InputVars {
    
    public bool key_jumpDown;
    public bool key_jump;
    public bool key_dashDown;
    public bool key_lockDown;
    public bool key_lock;
    public float userInputMoveDirection;

}