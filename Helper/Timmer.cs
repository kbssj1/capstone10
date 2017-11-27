using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System;
public class Timmer : MonoBehaviour,IListener {

    // Use this for initialization
    
    Stopwatch sw;
    bool isStart= false;
    public Text textTime_murder;
    public Text textTime_survi;
    GameObject murder;
    GameObject survi;
	float timer;
    void Awake()
    {
        sw = new Stopwatch();
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
		timer = LevelManager.Instance.SetGameTimerByLevel ();
    }
    
    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
	{

		switch (Event_Type) {

		case EVENT_TYPE.TIME_START:
                
			sw.Start ();
			isStart = true;
                
			break;
     
		}
		;
	}
    void Update()
    {
        if(textTime_murder == null)
        {
            textTime_murder = GameObject.FindGameObjectWithTag("MURDERER").transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("Text").GetComponentInChildren<Text>();

        }
        if(textTime_survi == null)
        {
            textTime_survi = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("Text").GetComponentInChildren<Text>();
        }
        
        if (isStart)
        {
            if(textTime_murder != null && textTime_survi != null)
            {
                textTime_murder.text = "" + (int)(timer - sw.ElapsedMilliseconds / 1000) / 60 + " : " + (int)(timer - sw.ElapsedMilliseconds / 1000) % 60;
                textTime_survi.text = "" + (int)(timer - sw.ElapsedMilliseconds / 1000) / 60 + " : " + (int)(timer - sw.ElapsedMilliseconds / 1000) % 60;

                if ((timer - sw.ElapsedMilliseconds / 1000)< 0)
                {
                    EventManager.Instance.PostNotification(EVENT_TYPE.TIME_OVER, this, true);
                    sw.Stop();
                    isStart = false;
                }
            }
        }
    }
    
	// Update is called once per frame

}
