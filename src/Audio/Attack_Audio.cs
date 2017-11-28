using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Audio : AudioController
{

    public AudioClip audio_Attack_Start;
    public AudioClip audio_Attack_Normal;
    public AudioClip audio_Attack_Attack;

    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool Rightly = false)
    {
        if (audioSource != null)
            return null;

        if (Rightly == false)
        {
            switch (audioName)
            {
                case "NORMAL":
                    audioSource.clip = audio_Attack_Normal;
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Attack)");
                    break;
            }
            //PlayCurrentAudioRightly();

            PlayCurrentAudio();

        }
        else
        {
            switch (audioName)
            {
                case "START":                      
                    audioSource.clip = audio_Attack_Start;
                    if (audioSource.clip != null)
                        StartCoroutine(TestAudio(audioSource.clip.length));
                    break;
                case "ATTACK":
                    audioSource.clip = audio_Attack_Attack;
                    if (audioSource.clip != null)
                        StartCoroutine(TestAudio(1.2f));
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Attack)");
                    break;
            }
                
                
        }
            
    }
}

