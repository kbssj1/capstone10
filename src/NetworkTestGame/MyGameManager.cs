using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MyGameManager : MonoBehaviour {

    public Text text;

    public Man man;
    public Woman woman;
    public Boss boss;

    public static string id;

    void Update()
    {
        text.text = "Man : " + man.GetHp() + " Woman : " + woman.GetHp() + " Boss : " + boss.GetHp();
    }

    public void TestBtn()
    {
        SceneManager.LoadScene("Lobby");
    }
}
