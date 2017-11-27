using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murder_Audio : AudioController
{
    public AudioClip Audio_Run;
    public AudioClip Audio_Walk;
    public AudioClip Audio_Attack_cry;
    public AudioClip Audio_Death;
    private float playTime = 1.0f;
    public bool RepeatCheck_Death = true;

    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool isOneSound = false)
    {
        if (audioSource != null)
        {
            if (isOneSound == false)
            {
                switch (audioName)
                {
                    case "RUN":
                        audioSource.clip = Audio_Run;
                        break;
                    case "WALK":
                        audioSource.clip = Audio_Walk;
                        break;
                    case "NOT":
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
                switch (audioName)
                {
                    case "ATTACK":
                        audioSource.clip = Audio_Attack_cry;
                        playTime = 1.2f;
                        break;
                    case "DEATH":
                        if (RepeatCheck_Death == true)
                        {
                            audioSource.clip = Audio_Death;
                            RepeatCheck_Death = false;
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
}
