using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murder_Audio : AudioController
{
    public AudioClip AUDIO_RUN;
    public AudioClip AUDIO_WALK;
    public AudioClip AUDIO_ATTACK_CRY;
    public AudioClip AUDIO_DEATH;
    private float playTime = 1.0f;
    public bool checkMurderDeath = true;

    void Start()
    {
        base.Init();
    }

    public void PlayAudio(AudioType audiotype, bool isOneSound = false)
    {
        if (isOneSound == false)
        {
            switch (audiotype)
            {
                case AudioType.MURDER_RUN:
                    audioSource.clip = AUDIO_RUN;
                    break;
                case AudioType.MURDER_WALK:
                    audioSource.clip = AUDIO_WALK;
                    break;
                case AudioType.NOT:
                    audioSource.clip = null;
                    audioSource.Stop();
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Murder)");
                    break;
            }
            PlayCurrentAudio();
        }
        else
        {
            switch (audiotype)
            {
                case AudioType.MURDER_ATTACK:
                    if (isAudioPlay())
                    { 
                        audioSource.clip = AUDIO_ATTACK_CRY;
                        playTime = 1.2f;
                    }
                    break;
                case AudioType.MURDER_DEATH:
                    if (checkMurderDeath == true)
                    {
                        audioSource.clip = AUDIO_DEATH;
                        checkMurderDeath = false;
                        playTime = audioSource.clip.length;
                    }
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Survivor)");
                    break;
            }
            // PlayCurrentAudioRightly();             
            if (audioSource.clip != null)
                StartCoroutine(TestAudio(playTime));
        }
    }
    
}
