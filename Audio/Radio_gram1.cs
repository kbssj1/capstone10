using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio_gram1 : MonoBehaviour {

    public AudioSource source;
    private bool isUse = false;
    public AudioClip clip;

    void Start()
    {
        source.clip = clip;
    }

    public void PlayAudio()
    {
        if (!source.isPlaying && !isUse)
        {
            source.Play();
            isUse = true;
        }
    }
}
