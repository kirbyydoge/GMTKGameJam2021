using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Range(0, 1)]public float smoothing = 0.00001f;
    private Transform playerTransform;
    void Start()
    {
        playerTransform = GameObject
                                .FindWithTag("Player")
                                .GetComponent<Rigidbody2D>()
                                .transform;
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;
        playerPos.z = this.transform.position.z;
        this.transform.position += (playerPos - this.transform.position) * smoothing;
    }
}
