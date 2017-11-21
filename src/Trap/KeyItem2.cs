using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem2 : MonoBehaviour, IListener {

    private Survivor survivor;

    public GameObject Object;
    private bool open;
    private bool possible;

    // Use this for initialization
    void Start()
    {
        possible = false;
        open = false;
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.KEY_GET_SUCCESS2, this);
        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_OPEN_KEY, this);
    }

    IEnumerator CheckKeyItem() // 
    {
        yield return null;

        while (true)
        {
            
            Vector3 viewPos = Camera.main.WorldToViewportPoint(Object.GetComponent<Transform>().position); // 카메라 뷰포트로 변환

           
            if (possible && viewPos.x > 0.1f && viewPos.x < 0.9f && viewPos.y > 0.1f && viewPos.y < 0.9f)
            {

                if (Input.GetButtonDown("Fire2"))
                {

                    EventManager.Instance.PostNotification(EVENT_TYPE.KEY_GET2, this);
                    
                }
            }

            yield return null;
        }

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && open)
        {
            possible = true;
            EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_POSSIBLE, this);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && open)
        {
            possible = false;

            EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE, this);
        }
    }
    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_CREATE:

                survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();

              

                break;
            case EVENT_TYPE.KEY_GET_SUCCESS2:
                //keyAudio.PlayAudio();
                Object.SetActive(false);
                EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE, this);
                break;

            case EVENT_TYPE.GRAM_OPEN_KEY:

                if (survivor.pv.isMine)
                {
                    open = true;
                    StartCoroutine(CheckKeyItem()); //코루틴 실행

                }

                break;
        };
    }
}
