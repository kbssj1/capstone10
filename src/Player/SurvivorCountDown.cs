using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorCountDown : MonoBehaviour {

    public Survivor _survivor;
    public void OnCountDownEnd()
    {
        print("CountDownEndFirst");
        _survivor.OnCountEnd();
    }
}
