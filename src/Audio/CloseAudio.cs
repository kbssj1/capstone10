using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseAudio : AudioController, IListener {
    [SerializeField]
    private Transform suri;
    [SerializeField]
    private Transform murdrer;

    public AudioClip close_audio;
    public AudioSource mainAudio;

    void Start()
    {
        base.Init();
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.MURDERER_CREATE, this);

    }

    public void PlayAudio()
    {
        if (audioSource != null && Check)
        {
            audioSource.clip = close_audio;
            StartCoroutine(TestAudio(close_audio.length));
            
        }
    }
    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_CREATE:
                suri = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Transform>();
                break;
            case EVENT_TYPE.MURDERER_CREATE:
                murdrer = GameObject.FindGameObjectWithTag("MURDERER").GetComponent<Transform>();
                break;


        };
    }
    // Update is called once per frame
    void Update () {

        /*
        if(suri == null)
            suri = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Transform>();
        if (murdrer == null)
        {
            try { 
                murdrer = GameObject.FindGameObjectWithTag("MURDERER").GetComponent<Transform>();
            }catch(System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        */
        if (suri != null && murdrer != null)
        {
            float distace = Vector3.Distance(murdrer.transform.position, suri.transform.position);

            if (distace <= 10.0)
            {
                PlayAudio();
                mainAudio.volume = 0.5f;
                audioSource.volume = 1.0f - (distace * 0.1f);
            }
            else
            {
                audioSource.Stop();
                mainAudio.volume = 1.0f;
                Check = true;
            }
        }
        
    }
}
