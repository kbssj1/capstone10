using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextSeting : MonoBehaviour {

    public Text[] text = new Text[600];

    void Awake()
    {
        text = Resources.FindObjectsOfTypeAll<Text>();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == null)
                break;

            text[i].fontSize = (int)(Screen.width * GameInformation.fontsize);
        }
    }
}
