using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor_Audio2 : AudioController
{

    public AudioClip Audio_SpeedUp_Heart;
    public AudioClip Audio_SpeedDown_Heart;

    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool isOneSound = false)
    {
        if (isOneSound == false)
        {

        }
        else
        {
            switch (audioName)
            {

                case "HEART_SPEED_UP":
                    audioSource.clip = Audio_SpeedUp_Heart;
                    break;
                case "HEART_SPEED_DOWN":
                    audioSource.clip = Audio_SpeedDown_Heart;
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Survivor2)");
                    break;
            }
            // PlayCurrentAudioRightly();               
            if (audioSource.clip != null)
                StartCoroutine(TestAudio(audioSource.clip.length));

        }
        
    }
}
