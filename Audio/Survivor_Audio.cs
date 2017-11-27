using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor_Audio : AudioController {

    public AudioClip Audio_Run;
    public AudioClip Audio_Walk;
    public AudioClip Audio_crouch_walk;
    public AudioClip Audio_crouch;
    public AudioClip Audio_Death;
    public AudioClip Audio_GetDamage_Cry;

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
                    case "CROUCH_WALK":
                        audioSource.clip = Audio_crouch_walk;
                        break;
                    case "NOT":
                        audioSource.clip = null;
                        audioSource.Stop();
                        break;
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Survivor)");
                        break;
                }
                PlayCurrentAudio();
            }
            else
            {
                switch (audioName)
                {
                    case "CROUCH":
                        audioSource.clip = Audio_crouch;
                        break;
                    case "ATTACKED":
                        audioSource.clip = Audio_GetDamage_Cry;
                        break;
                    case "DEATH":
                        audioSource.clip = Audio_Death;
                        break;
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Survivor)");
                        break;
                }
                // PlayCurrentAudioRightly();               
                if (audioSource.clip != null)
                    StartCoroutine(TestAudio(audioSource.clip.length));
                
            }
        }
    }
   
}
