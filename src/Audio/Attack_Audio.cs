using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Audio : AudioController
{

    public AudioClip Audio_Attack_Start;
    public AudioClip Audio_Attack_Normal;
    public AudioClip Audio_Attack_Attack;

    void Start()
    {
        base.Init();
    }

    public void PlayAudio(AudioType attackType, bool Rightly = false)
    {
        if (audioSource != null)
        {
            if (Rightly == false)
            {
                switch (attackType)
                {
                    case AudioType.CHAINSSAW_IDLE:
                        audioSource.clip = Audio_Attack_Normal;
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
                switch (attackType)
                {
                    case AudioType.CHAINSSAW_IDLE:
                        audioSource.clip = Audio_Attack_Start;
                        if (audioSource.clip != null)
                            StartCoroutine(TestAudio(audioSource.clip.length));
                        break;
                    case AudioType.CHAINSSAW_ATTACK:
                        if (GetCheck())
                        {
                            audioSource.clip = Audio_Attack_Attack;
                            if (audioSource.clip != null)
                                StartCoroutine(TestAudio(1.2f));
                        }
                        break;
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Attack)");
                        break;
                }
                
                
            }
        }       
    }
}

