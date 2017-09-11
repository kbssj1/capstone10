using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubbyhole_Audio : MonoBehaviour {
    private bool isUse = false;
    public AudioSource source;

    public AudioClip clip;
    public void PlayAudio()
    {
        if (!source.isPlaying && isUse) 
        {
            source.Play();
            isUse = false;
        }
    }

    public void PauseAudio()
    {
        source.Pause();
    }
}
