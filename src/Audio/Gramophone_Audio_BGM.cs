using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gramophone_Audio_BGM : AudioController
{
    private float playTime = 0.0f;

    public AudioClip Audio_BGM;

    // Use this for initialization
    void Start()
    {
        base.Init();
        audioSource.clip = Audio_BGM;
    }

    public void PlayBGMAudio()
    {
        if (playTime <= Audio_BGM.length && !audioSource.isPlaying)
        {
            audioSource.time = playTime;
            audioSource.Play();
        }
    }

    public void StopAudio()
    {
        playTime += (audioSource.time - playTime);
        audioSource.Pause();
    }
}
