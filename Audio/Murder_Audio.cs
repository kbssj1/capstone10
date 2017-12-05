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

    public void PlayAudio(string audioName, bool isPlayedAudioOnce = false)
    {
		if (audioSource == null)
			return;
		else
        {
			switch (audioName) {
			case "RUN":
				if (!isPlayedAudioOnce) {
					audioSource.clip = Audio_Run;
					PlayCurrentAudio();
				}
				break;
			case "WALK":
				if (!isPlayedAudioOnce) {
					audioSource.clip = Audio_Walk;
					PlayCurrentAudio();
				}
				break;
			case "NOT":
				if (!isPlayedAudioOnce) {
					audioSource.clip = null;
					audioSource.Stop();
					PlayCurrentAudio();
				}
				break;
			case "ATTACK":
				if (isPlayedAudioOnce) {
					audioSource.clip = Audio_Attack_cry;
					playTime = 1.2f;
					if (audioSource.clip != null)
						StartCoroutine(TestAudio(playTime));
				}
				break;
			case "DEATH":
				if (isPlayedAudioOnce) {
					if (RepeatCheck_Death == true)
					{
						audioSource.clip = Audio_Death;
						RepeatCheck_Death = false;
						playTime = audioSource.clip.length;
					}
					if (audioSource.clip != null)
						StartCoroutine(TestAudio(playTime));
				}
				break;
			default:
				Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Murder)");
				break;
			}
        }
    }
}
