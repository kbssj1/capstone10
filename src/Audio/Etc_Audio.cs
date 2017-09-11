using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Etc_Audio : AudioController
{
    private float playTime = 1.0f;

    public AudioClip Audio_CountDown;
    public AudioClip Audio_Bomb;

    // Use this for initialization
    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool isOneSound = false)
    {
        if (audioSource != null)
        {
            switch (audioName)
            {
                case "COUNTDOWN":
                    audioSource.clip = Audio_CountDown;
                    playTime = audioSource.clip.length;
                    //playTime = 5.0f;
                    break;
                case "BOMB":
                    audioSource.clip = Audio_Bomb;
                    playTime = audioSource.clip.length;
                    break;
                default:
                    Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Gramophone)");
                    break;
            }
            
            if (audioSource.clip != null)
                StartCoroutine(TestAudio(playTime));
        }
    }
}
