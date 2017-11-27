using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public Radio_BGM_Audio radio_BGM_Audio;
    public Radio_effect_Audio radio_effect_Audio;

    public Gramophone_Audio_BGM gramophone_Audio_BGM;
    public Gramophone_Audio_effect gramophone_Audio_effect;

    public Elavator_Btn elvator_btn1;

    public Elavator_Btn elvator_btn2;

    public Radio_Gram key_audio1;
    public Radio_gram1 key_audio2;

    public Audio_Mongue mongue_audio1;
    public Audio_Mongue mongue_audio2;
    /*
    void Start()
    {
        key_audio1.Init();
        key_audio1.PlayAudio();

        StartCoroutine(TestAudio(3.0f));
    }

    protected IEnumerator TestAudio(float AudioTime)
    {
        yield return new WaitForSeconds(AudioTime);

        key_audio2.Init();
        key_audio2.PlayAudio();
    }
    */
}
