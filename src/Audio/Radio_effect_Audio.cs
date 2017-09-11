using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio_effect_Audio : MonoBehaviour {

    public AudioSource source;

    public AudioClip clip;

    public void PlayAudio()
    {
        if (!source.isPlaying)
            source.Play();
    }

    public void PauseAudio()
    {
        source.Pause();
    }
}
