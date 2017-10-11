using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurdererCountDown : MonoBehaviour {

    public Murderer murderer;
    public void OnCountDownEnd()
    {
        print("CountDownEndFirst");
        murderer.OnCountEnd();
    }
}
