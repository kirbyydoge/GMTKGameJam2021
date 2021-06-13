using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public AudioSource audio;
    private bool isReady;
    private float delay;
    void Start()
    {
        //Reference to the AudioListener component on the object
        audio = GetComponent<AudioSource>();
        isReady = true;
        delay = 1;
    }

    void Update()
    {
        if (!isReady)
        {
            delay = delay - Time.deltaTime;
        }

        if (delay < 0.1 && !isReady)
        {

            isReady = true;
        }
        //Toggles sound on/off by pressing 'M'
        if (Input.GetKeyUp(KeyCode.M) && isReady)
        {
            delay = 1;
            isReady = false;
            Debug.Log("AAAAAAAA");
            audio.mute = !audio.mute;

        }

    }
}
