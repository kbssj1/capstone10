using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginScreen : MonoBehaviour {

    public InputField ID_InputField;
    public InputField PASSWORD_InputField;
    public InfoMessage message;

    public IEnumerator Login_()
    {
        string address = "http://kbssj1.cafe24.com/Universal/Main/UserLogin.php";

        WWWForm cFrom = new WWWForm();
        BaseNetwork.Base(ref cFrom);

        cFrom.AddField("id", ID_InputField.text);
        cFrom.AddField("password", PASSWORD_InputField.text);

        WWW wwwUrl = new WWW(address, cFrom);

        yield return wwwUrl;

        if (wwwUrl.text.Equals("UserLoginFail"))
        {
            message.ShowInfoText("아이디 및 비밀번호가 틀렸습니다.");
        }
        else
        {
            GameInformation.id = ID_InputField.text;
            SceneManager.LoadScene("Lobby");
        }
    }

    public void Login()
    {
        StartCoroutine(Login_());
    }
}
