using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D playerRb;
    private Animator animPlayer;

    void Start()
    {
        animPlayer = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerRb.velocity.x>0.01||playerRb.velocity.x<-0.01){
            if(playerRb.velocity.y>0.05||playerRb.velocity.y<-0.05){
                animPlayer.SetInteger("AnimationState",2);
            }
            else{
                animPlayer.SetInteger("AnimationState",1);
            }
        }
        else{
            if(playerRb.velocity.y>0.05||playerRb.velocity.y<-0.05){
                animPlayer.SetInteger("AnimationState",2);
            }
            else{
                animPlayer.SetInteger("AnimationState",0);
            }
        }
    }
}
