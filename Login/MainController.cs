using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour {

    public GameObject Login_Screen;
    public GameObject CreateID_Screen;
    public CreateIDScreen CreateID;

    public void Open_Login_Screen()
    {
        Login_Screen.SetActive(true);
    }

    public void Close_Login_Screen()
    {
        Login_Screen.SetActive(false);
    }

    public void Open_CreateID_Screen()
    {
        CreateID_Screen.SetActive(true);
        CreateID.isIdOverlapCheck = false;
    }

    public void Close_CreateID_Screen()
    {
        CreateID_Screen.SetActive(false);
    }
}
