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

    public void PlayAudio(string audioName, bool playedAudioRightly = false)
    {
		if (audioSource == null)
			return;
		else
        {

			switch (audioName) {
			case "NORMAL":
				if (!playedAudioRightly) {
					audioSource.clip = audio_Attack_Normal;
					PlayCurrentAudio();
				}
				break;
			case "START":
				if (playedAudioRightly) {
					audioSource.clip = audio_Attack_Start;
					if (audioSource.clip != null)
						StartCoroutine (TestAudio (audioSource.clip.length));
				}
				break;
			case "ATTACK":
				if (playedAudioRightly) {
					audioSource.clip = audio_Attack_Attack;
					if (audioSource.clip != null)
						StartCoroutine (TestAudio (1.2f));
				}
				break;
			default:
				Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Attack)");
				break;
			}
				
        }       
    }
}

