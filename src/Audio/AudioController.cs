using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{

    public AudioSource audioSource;
    protected bool Check = true;

    protected void Init()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected void PlayCurrentAudio()
    {
        if (audioSource != null)
        {
            if (audioSource.clip == null)
            {
                audioSource.Stop();
                // Do Nothing
            }
            else
            {
                if(!audioSource.isPlaying)
                    audioSource.Play();                
            }
        }
    }

    private void PlayCurrentAudioRightly()
    {
        if (audioSource != null)
        {
            if (audioSource.clip == null)
            {
                // Do Nothing
            }
            else
            {
                audioSource.Stop();
                audioSource.Play();
            }
        }
    }


    public bool isAudioPlay()
    {
        return Check;
    }
    public void SetCheck(bool set)
    {
        Check = set;
    }
    protected IEnumerator TestAudio(float AudioTime)
    {
        Check = false;

        PlayCurrentAudioRightly();
        yield return new WaitForSeconds(AudioTime);

        Check = true;
    }
}
