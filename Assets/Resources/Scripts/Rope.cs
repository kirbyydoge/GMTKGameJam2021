using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public float ropeLength = 10;
    public float ropeActualWidth = 0.1f;
    public float ropeVisualWidth = 0.15f;
    public int numSegments = 2;
    public int lineSmoothness = 10;
    private GameObject ropeSegmentPrefab;
    private GameObject debuggerPrefab;
    private GameObject[] ropeSegments;
    private LineRenderer lineRenderer;
    private Rigidbody2D playerRb;
    private Rigidbody2D spiritRb;
    private DistanceJoint2D playerJoint;
    private PlayerMovement playerMovement;
    private SpiritMovement spiritMovement;
    private bool[] segmentCollisions;
    private float segmentLength;
    void Start()
    {
        ropeSegmentPrefab = (GameObject) Resources.Load("Prefabs/MainActors/RopeSegment");
        debuggerPrefab = (GameObject) Resources.Load("Prefabs/Debugger");
        lineRenderer = GetComponent<LineRenderer>();
        segmentCollisions = new bool[numSegments];
        lineRenderer.numCornerVertices = lineSmoothness;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = numSegments+1;
        lineRenderer.startWidth = ropeVisualWidth;
        lineRenderer.endWidth = ropeVisualWidth;
        ropeSegments = new GameObject[numSegments];
        segmentLength = ropeLength / numSegments;
        Vector3 start = transform.position;
        for(int i = 0; i < numSegments; i++) {
            Vector3 end = start + Vector3.right * segmentLength;
            BoxCollider2D curCollider = ropeSegmentPrefab.GetComponent<BoxCollider2D>();
            curCollider.offset = new Vector2((end.x - start.x)/2, 0f);
            curCollider.size = new Vector2(segmentLength, ropeActualWidth);
            GameObject curObj = Instantiate(ropeSegmentPrefab, start, Quaternion.identity);
            curObj.GetComponent<RopeSegment>().SetID(i);
            // Instantiate(debuggerPrefab, start, Quaternion.identity);
            ropeSegments[i] = curObj;
            start = end;
        }
        HingeJoint2D hingeJoint;
        Rigidbody2D nextSegmentrb;
        //Connect spirit to first rope segment
        GameObject playerObj = GameObject.Find("Spirit");
        spiritMovement = playerObj.GetComponent<SpiritMovement>();
        spiritRb = playerObj.GetComponent<Rigidbody2D>();
        ropeSegments[0].AddComponent<HingeJoint2D>();
        hingeJoint = ropeSegments[0].GetComponents<HingeJoint2D>()[0];
        nextSegmentrb = playerObj.GetComponent<Rigidbody2D>();
        ConnectHingeToRigid(hingeJoint, nextSegmentrb, 0);
        //Connect 2nd end of first rope to next rope segment
        hingeJoint = ropeSegments[0].GetComponents<HingeJoint2D>()[1];
        nextSegmentrb = ropeSegments[1].GetComponent<Rigidbody2D>();
        ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
        //Connect spirit to first rope segment
        //Connect rope segments together
        for(int i = 1; i < numSegments - 1; i++) {
            hingeJoint = ropeSegments[i].GetComponent<HingeJoint2D>();
            nextSegmentrb = ropeSegments[i+1].GetComponent<Rigidbody2D>();
            ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
            /*
            distJoint = ropeSegments[i].GetComponent<DistanceJoint2D>();
            distJoint.anchor  = hingeJoint.anchor;
            distJoint.connectedAnchor = hingeJoint.connectedAnchor;
            distJoint.connectedBody = nextSegmentrb;
            distJoint.autoConfigureConnectedAnchor = false;
            distJoint.distance = 0;
            distJoint.enabled = true;
            */
        }
        //Connect last rope segment to player
        playerObj = GameObject.Find("Player");
        hingeJoint = ropeSegments[numSegments-1].GetComponent<HingeJoint2D>();
        playerRb = playerObj.GetComponent<Rigidbody2D>();
        playerMovement = playerObj.GetComponent<PlayerMovement>();
        playerJoint = playerObj.GetComponent<DistanceJoint2D>();
        playerJoint.enabled = false;
        ConnectHingeToRigid(hingeJoint, playerRb, segmentLength);
        /*
        distJoint = ropeSegments[numSegments-1].GetComponent<DistanceJoint2D>();
        distJoint.anchor  = hingeJoint.anchor;
        distJoint.connectedAnchor = hingeJoint.connectedAnchor;
        distJoint.connectedBody = nextSegmentrb;
        distJoint.autoConfigureConnectedAnchor = false;
        distJoint.distance = 0;
        distJoint.enabled = true;
        */
        Physics2D.IgnoreLayerCollision(8, 8);   //Rope rope
        Physics2D.IgnoreLayerCollision(8, 9);   //Rope player
    }
    void Update()
    {
        HandleCollision();
        DrawRope();
    }

    void HandleCollision() {
        int min = numSegments;
        int max = -1;
        for(int i = 0; i < numSegments; i++) {
            if(segmentCollisions[i] && i < min) {
                min = i;
            }
            if(segmentCollisions[i] && i > max) {
                max = i;
            }
        }
        if(max > -1 && playerRb.position.y < ropeSegments[max].transform.position.y) {
            playerMovement.SetCircularAnchor(ropeSegments[max].transform.position, max);
            playerMovement.SetCircularMovement(true);
        }
        if(min < numSegments && spiritRb.position.y < ropeSegments[min].transform.position.y) {
            spiritMovement.SetCircularAnchor(ropeSegments[min].transform.position, min);
            spiritMovement.SetCircularMovement(true);
        }
        if(min == numSegments && max == -1) {
            playerMovement.SetCircularMovement(false);
            spiritMovement.SetCircularMovement(false);
        }
    }

    void DrawRope() {
        Vector3[] points = new Vector3[numSegments+1];
        for(int i = 0; i < numSegments; i++) {
            points[i] = ropeSegments[i].transform.position;
        }
        points[numSegments] = playerRb.position;
        lineRenderer.SetPositions(points);
    }

    void ConnectHingeToRigid(HingeJoint2D hingeJoint, Rigidbody2D nextSegmentrb, float offset) {
        hingeJoint.anchor = new Vector2(offset, 0);
        hingeJoint.connectedAnchor = new Vector2(0, 0);
        hingeJoint.connectedBody = nextSegmentrb;
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.enableCollision = true;
        hingeJoint.enabled = true;
    }

    void ConnectRelativeToRigid(HingeJoint2D relativeJoint, Rigidbody2D nextSegmentrb, float offset) {
        relativeJoint.anchor = new Vector2(offset, 0);
        relativeJoint.connectedAnchor = new Vector2(0, 0);
        relativeJoint.connectedBody = nextSegmentrb;
        relativeJoint.autoConfigureConnectedAnchor = false;
        relativeJoint.enableCollision = true;
        relativeJoint.enabled = true;
    }

    public void NotifyCollision(int ID, bool state) {
        segmentCollisions[ID] = state;
    }
}
