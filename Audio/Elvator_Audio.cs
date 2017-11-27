using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elvator_Audio : AudioController
{
    public AudioClip Audio_Effect;
    private bool isUsing = false;

    // Use this for initialization
    void Start()
    {
        base.Init();
        audioSource.clip = Audio_Effect;
    }

    public void PlayAudio()
    {
        if (!audioSource.isPlaying && !isUsing)
        {
            audioSource.Play();
            isUsing = true;
        }
    }
}
