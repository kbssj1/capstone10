using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCtrl : MonoBehaviour, IListener
{
    private Survivor survivor;
    private bool possible;
    public GameObject Object;
    public OutlineSystem outLine;
    public int cnt = 0;
    public GameObject door;
    public GameObject slide;

    // Use this for initialization
    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.RADIO_SUCCESS, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.MURDERER_CREATE, this);
        possible = false;

    }
 
    IEnumerator CheckRadioCtrl() // 
    {
        yield return null;

        while (true)
        {

            Vector3 viewPos = Camera.main.WorldToViewportPoint(Object.GetComponent<Transform>().position); // 카메라 뷰포트로 변환


            if (possible && viewPos.x > 0.1f && viewPos.x < 0.9f && viewPos.y > 0.1f && viewPos.y < 0.9f)
            {

                if (Input.GetButtonDown("Fire2"))
                {


                    survivor.GetComponent<RadioTrap>().enabled = true;
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_RADIOCTRL, this);

                }



                float _input = Input.GetAxis("Oculus_GearVR_DpadY");//Oculus_GearVR_DpadX

                if (survivor != null && survivor.getRadioCtrl() && _input < -0.5f)
                {


                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_RADIOCTRL, this, _input);
                    if (cnt >= 8)
                    {
                        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_RADIO_SUC, this);
                    }
                }
            }


            yield return null;
        }

    }

    [PunRPC]
    public IEnumerator DoorOpen()
    {
        door.GetComponent<Animator>().SetBool("Open", true);
        yield return new WaitForSeconds(1f);
        slide.GetComponent<Animator>().SetBool("Slide", true);
        EventManager.Instance.PostNotification(EVENT_TYPE.RADIO_OPEN_KEY, this);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR"))
        {
            possible = true;
            EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_POSSIBLE, this);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("SURVIVOR"))
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
                if (survivor.pv.isMine)
                {
                    StartCoroutine(CheckRadioCtrl()); //코루틴 실행

                }
                break;

            case EVENT_TYPE.MURDERER_CREATE:

                Murderer murder = GameObject.FindGameObjectWithTag("MURDERER").GetComponent<Murderer>();
                if (murder.pv.isMine)
                    outLine.enabled = false;

                break;
            case EVENT_TYPE.RADIO_SUCCESS:
               
                StartCoroutine(DoorOpen());
                break;
        };
    }
}