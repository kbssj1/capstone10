using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Mongue : MonoBehaviour {

    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private AudioClip clip;
    private bool isUse = false;

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

    public void PauseAudio()
    {
        source.Pause();
    }
}
