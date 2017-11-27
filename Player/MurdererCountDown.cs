using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurdererCountDown : MonoBehaviour {

    public Murderer _murderer;
    public void OnCountDownEnd()
    {
        print("CountDownEndFirst");
        _murderer.OnCountEnd();
    }
}
