using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeActions : MonoBehaviour
{

    public enum SpikeActionType {
        Damage,
        Kill,
        Debug
    };

    public SpikeActionType playerAction = SpikeActionType.Debug;
    public SpikeActionType spiritAction = SpikeActionType.Debug;

    void Start() {
        Physics2D.IgnoreLayerCollision(12, 10); //Ignore Ground
        Physics2D.IgnoreLayerCollision(12, 11); //Ignore Wall
        Physics2D.IgnoreLayerCollision(12, 8);  //Ignore Rope
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") {
            HandlePlayerTrigger();
        }
        if(other.tag == "Spirit") {
            HandleSpiritTrigger();
        }
    }

    void HandlePlayerTrigger() {
        switch(playerAction) {
        case SpikeActionType.Damage:
            //Function calls, calculations, flag operations etc.
            break;
        case SpikeActionType.Kill:
            //Function calls, calculations, flag operations etc.
            break;
        case SpikeActionType.Debug:
            print("Player triggered a spike.");
            break;
        default:
            print("Undefined player action.");   
            break;
        }
    }

    void HandleSpiritTrigger(){
        switch(spiritAction) {
        case SpikeActionType.Damage:
            //Function calls, calculations, flag operations etc.
            break;
        case SpikeActionType.Kill:
            //Function calls, calculations, flag operations etc.
            break;
        case SpikeActionType.Debug:
            print("Spirit triggered a spike.");
            break;
        default:
            print("Undefined spirit action.");   
            break;
        }
    }

}
