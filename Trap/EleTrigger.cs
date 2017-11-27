using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EleTrigger : MonoBehaviour {


	

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR"))
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_WIN, this);

        }
    }
}
