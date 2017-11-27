using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gramophone_Audio_effect : AudioController
{
    public AudioClip Audio_Effect;

    // Use this for initialization
    void Start()
    {
        base.Init();
        audioSource.clip = Audio_Effect;
    }

    public void PlayAudio()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopAudio()
    {
        audioSource.Pause();
    }

}
