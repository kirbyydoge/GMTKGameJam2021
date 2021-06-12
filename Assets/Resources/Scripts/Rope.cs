using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public float ropeLength = 10;
    public float ropeWidth = 0.1f;
    public int numSegments = 2;
    public int lineSmoothness = 10;
    private GameObject ropeSegmentPrefab;
    private GameObject debuggerPrefab;
    private GameObject[] ropeSegments;
    private LineRenderer lineRenderer;
    private float segmentLength;
    void Start()
    {
        ropeSegmentPrefab = (GameObject) Resources.Load("Prefabs/RopeSegment");
        debuggerPrefab = (GameObject) Resources.Load("Prefabs/Debugger");
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.numCornerVertices = lineSmoothness;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = numSegments;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        ropeSegments = new GameObject[numSegments];
        segmentLength = ropeLength / numSegments;
        Vector3 start = transform.position;
        for(int i = 0; i < numSegments; i++) {
            Vector3 end = start + Vector3.right * segmentLength;
            RopeSegment curSegment = ropeSegmentPrefab.GetComponent<RopeSegment>();
            curSegment.width = ropeWidth;
            curSegment.length = segmentLength;
            BoxCollider2D curCollider = ropeSegmentPrefab.GetComponent<BoxCollider2D>();
            curCollider.offset = new Vector2((end.x - start.x)/2, 0f);
            curCollider.size = new Vector2(segmentLength, ropeWidth);
            GameObject curObj = Instantiate(ropeSegmentPrefab, start, Quaternion.identity);
            // Instantiate(debuggerPrefab, start, Quaternion.identity);
            ropeSegments[i] = curObj;
            start = end;
        }
        HingeJoint2D hingeJoint;
        Rigidbody2D nextSegmentrb;
        // DistanceJoint2D  distJoint;
        //Connect spirit to first rope segment
        GameObject playerObj = GameObject.Find("Spirit");
        ropeSegments[0].AddComponent<HingeJoint2D>();
        hingeJoint = ropeSegments[0].GetComponents<HingeJoint2D>()[0];
        nextSegmentrb = playerObj.GetComponent<Rigidbody2D>();
        ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
        //Connect 2nd end of first rope to next rope segment
        hingeJoint = ropeSegments[0].GetComponents<HingeJoint2D>()[1];
        nextSegmentrb = ropeSegments[1].GetComponent<Rigidbody2D>();
        ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
        //Connect rope segments together
        for(int i = 1; i < numSegments - 1; i++) {
            hingeJoint = ropeSegments[i].GetComponent<HingeJoint2D>();
            nextSegmentrb = ropeSegments[i+1].GetComponent<Rigidbody2D>();
            ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
            /*  Currently not needed, seperated colliders and line drawing.
            distJoint = ropeSegments[i].GetComponent<DistanceJoint2D>();
            distJoint.anchor  = hingeJoint.anchor;
            distJoint.connectedAnchor = hingeJoint.connectedAnchor;
            distJoint.connectedBody = nextSegmentrb;
            distJoint.autoConfigureConnectedAnchor = false;
            distJoint.distance = 0;
            distJoint.enabled = false;
            */
        }
        //Connect last rope segment to player
        playerObj = GameObject.Find("Player");
        hingeJoint = ropeSegments[numSegments-1].GetComponent<HingeJoint2D>();
        nextSegmentrb = playerObj.GetComponent<Rigidbody2D>();
        ConnectHingeToRigid(hingeJoint, nextSegmentrb, segmentLength);
        Physics2D.IgnoreLayerCollision(8, 8);
        Physics2D.IgnoreLayerCollision(8, 9);
    }
    void Update()
    {
        DrawRope();
    }

    void DrawRope() {
        Vector3[] points = new Vector3[numSegments];
        for(int i = 0; i < numSegments; i++) {
            points[i] = ropeSegments[i].transform.position;
        }
        lineRenderer.SetPositions(points);
    }

    void ConnectHingeToRigid(HingeJoint2D hingeJoint, Rigidbody2D nextSegmentrb, float offset) {
        hingeJoint.anchor = new Vector2(offset, 0);
        hingeJoint.connectedAnchor = new Vector2(0, 0);
        hingeJoint.connectedBody = nextSegmentrb;
        hingeJoint.autoConfigureConnectedAnchor = false;
        hingeJoint.enabled = true;
    }
}
