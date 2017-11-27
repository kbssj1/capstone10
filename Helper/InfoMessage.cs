using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InfoMessage : MonoBehaviour {
    public Text infotext;
    public GameObject InfoMessageWindow;

    public Queue<string> MessageQueue = new Queue<string>();

    public void CancelButton()
    {
        InfoMessageWindow.gameObject.SetActive(false);

        if (MessageQueue.Count > 0)
        {
            ShowInfoText(MessageQueue.Dequeue());
        }
    }

    public void ShowInfoText(string a)
    {
        //AudioControl.PlayAudioInfoMessage();

        if (InfoMessageWindow.activeSelf == true)
        {
            MessageQueue.Enqueue(a);
        }
        else
        {
            InfoMessageWindow.SetActive(true);
            infotext.text = a;
        }
    }

}
