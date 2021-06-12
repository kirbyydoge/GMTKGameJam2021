﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMovement : MonoBehaviour
{
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
}