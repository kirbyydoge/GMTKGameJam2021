using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D playerRb;
    private Animator animPlayer;
    private const float errorNumber=0.4f;
    private 


    void Start()
    {
        animPlayer = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Mathf.Round(playerRb.velocity.x)+" "+Mathf.Round(playerRb.velocity.y));
        if(Mathf.Round(playerRb.velocity.x)>errorNumber){
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            if(Mathf.Round(playerRb.velocity.y)>errorNumber||Mathf.Round(playerRb.velocity.y)<-errorNumber){
                animPlayer.SetInteger("AnimationState",2);
            }
            else{
                animPlayer.SetInteger("AnimationState",1);
            }
        }
        else if(Mathf.Round(playerRb.velocity.x)<-errorNumber){
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            if(Mathf.Round(playerRb.velocity.y)>errorNumber||Mathf.Round(playerRb.velocity.y)<-errorNumber){
                animPlayer.SetInteger("AnimationState",2);
            }
            else{
                animPlayer.SetInteger("AnimationState",1);
            }
        }
        else{
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            if(Mathf.Round(playerRb.velocity.y)>errorNumber||Mathf.Round(playerRb.velocity.y)<-errorNumber){
                animPlayer.SetInteger("AnimationState",2);
            }
            else{
                animPlayer.SetInteger("AnimationState",0);
            }
        }
    }
}
