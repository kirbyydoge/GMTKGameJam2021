using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSegment : MonoBehaviour
{
    public float width = 1;
    public float length = 2;
    private Rope rope;
    private int segmentID;

    void Start() {
        rope = GameObject.Find("Rope").GetComponent<Rope>();
    }

    void OnCollisionStay2D(Collision2D other) {
        rope.NotifyCollision(segmentID, true);
    }

    void OnCollisionExit2D(Collision2D other) {
        rope.NotifyCollision(segmentID, false);
    }
    public void SetID(int ID) {
        segmentID = ID;
    }
}
