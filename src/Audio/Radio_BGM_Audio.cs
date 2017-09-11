using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio_BGM_Audio : MonoBehaviour
{
    public AudioSource source1_NOISE;
    public AudioSource source2_BGM;

    public AudioClip Audio_Noise;
    public AudioClip Audio_BGM;
    
    public void SetVolume()
    {
        source1_NOISE.volume = 1.0f - (GameInformation.cnt_value * 0.1f + 0.2f);
        source2_BGM.volume = 1.0f - source1_NOISE.volume;
    }
    
    public void PlayAudio()
    {
        if (!source1_NOISE.isPlaying)
            source1_NOISE.Play();
        if(!source2_BGM.isPlaying)
            source2_BGM.Play();
    }

    public void PauseAudio()
    {
        source1_NOISE.Pause();
        source2_BGM.Pause();
    }
}
