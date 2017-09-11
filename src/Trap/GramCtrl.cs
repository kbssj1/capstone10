using UnityEngine;
using System.Collections;

public class GramCtrl : MonoBehaviour, IListener
{
    private Survivor survivor;
    private bool possible;
    public GameObject Object;
    public Animator anim;
    public GameObject door;
    public GameObject slide;
    public OutlineSystem outLine;
    public int time = 19;

    public System.Diagnostics.Stopwatch sw;
    // Use this for initialization
    void Start()
    {
        sw = new System.Diagnostics.Stopwatch();
        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_SUCCESS, this);
        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_STOP, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.MURDERER_CREATE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_HIT, this);
        possible = false;

        
        StartCoroutine(TimeCheck());
    }

 
    IEnumerator CheckGramCtrl() // 
    {
        yield return null;

        while (true)
        {

            Vector3 viewPos = Camera.main.WorldToViewportPoint(Object.GetComponent<Transform>().position); // 카메라 뷰포트로 변환

         
            if (possible && viewPos.x > 0.1f && viewPos.x < 0.9f && viewPos.y > 0.1f && viewPos.y < 0.9f)
            {

                if (Input.GetButtonDown("Fire2"))
                {

                    if (survivor != null && survivor.getGramCtrl())
                    {
                        sw.Stop();

                    }
                    else if (survivor != null && !survivor.getGramCtrl())
                    {
                        sw.Start();
                    }

                    survivor.GetComponent<GramTrap>().enabled = true;
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);
                }
                
            }
            
            yield return null;
        }

    }
    public IEnumerator TimeCheck()
    {

        string text;
        System.TimeSpan ts;

        while (true)
        {

            ts = sw.Elapsed;

           
            if (ts.Seconds > time)
            {
                EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_GRAM_SUC, this);

                break;
            }


            yield return null;

        }
    }

    public IEnumerator DoorOpen()
    {

        door.GetComponent<Animator>().SetBool("Open", true);
        yield return new WaitForSeconds(1f);
        slide.GetComponent<Animator>().SetBool("Slide", true);
        EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_OPEN_KEY, this);
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

            case EVENT_TYPE.GRAM_START:
                StopCoroutine("GramAnim");
                StartCoroutine("GramAnim", true);

                break;

            case EVENT_TYPE.GRAM_STOP:
                StopCoroutine("GramAnim");
                StartCoroutine("GramAnim", false);


                break;
            case EVENT_TYPE.SURVIVOR_CREATE:

                survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();
                if (survivor.m_pv.isMine)
                {
                    StartCoroutine(CheckGramCtrl()); //코루틴 실행

                }

                break;

            case EVENT_TYPE.MURDERER_CREATE:
                Murderer murder= GameObject.FindGameObjectWithTag("MURDERER").GetComponent<Murderer>();
                if(murder.m_pv.isMine)
                    outLine.enabled = false;

                break;
            case EVENT_TYPE.GRAM_SUCCESS:

                StartCoroutine(DoorOpen());

                break;
            case EVENT_TYPE.SURVIVOR_HIT:

                sw.Stop();

                break;
        };
    }

    public IEnumerator GramAnim(bool flag)
    {

        if (flag)
        {
            yield return new WaitForSeconds(1f);
            anim.SetBool("Start", true);
            anim.SetBool("Stop", false);
        }
        else
        {
            anim.SetBool("Stop", true);
            anim.SetBool("Start", false);
        }
        yield return null;
    }

}