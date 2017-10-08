using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCtrl2 : MonoBehaviour ,IListener {

    private Survivor survivor;

    public GameObject Object;
    private bool possible;

    public GameObject Elevator;
    public GameObject Ele_Door;



    // Use this for initialization
    void Start()
    {
        possible = false;
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.KEY_SUCCESS2, this);
    }

    IEnumerator CheckKeyCtrl() // 
    {
        yield return null;

        while (true)
        {

            Vector3 viewPos = Camera.main.WorldToViewportPoint(Object.GetComponent<Transform>().position); // 카메라 뷰포트로 변환

          
            if (possible && viewPos.x > 0.1f && viewPos.x < 0.9f && viewPos.y > 0.1f && viewPos.y < 0.9f && Input.GetButtonDown("Fire2") && survivor.getItemKey())
            {
                KeyOpen();
            }

            yield return null;
        }

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && survivor.getItemKey())
        {
            possible = true;
            EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_POSSIBLE, this);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && survivor.getItemKey())
        {
            possible = false;
            EventManager.Instance.PostNotification(EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE, this);
        }
    }

    public void KeyOpen()
    {

        survivor.GetComponent<KeyTrap2>().enabled = true;

       
        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_KEYCTRL2, this);
    }
    public IEnumerator OpenDoor()
    {
        Elevator.SetActive(true);
        yield return new WaitForSeconds(8.0f);
        Ele_Door.GetComponent<Animator>().SetBool("LeftOpen", true);
    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_CREATE:

                survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();

                if (survivor.Pv.isMine)
                {
                    StartCoroutine(CheckKeyCtrl()); //코루틴 실행

                }

                break;

            case EVENT_TYPE.KEY_SUCCESS2:

                StartCoroutine(OpenDoor());


                break;
        };
    }
}
