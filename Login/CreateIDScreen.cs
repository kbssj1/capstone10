using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreateIDScreen : MonoBehaviour {

    public InfoMessage message;

    public InputField ID_InputField;
    public InputField PASSWORD_InputField;
    public InputField PASSWORDCHECK_InputField;
    public MainController mainControl;

    public bool isIdOverlapCheck = false;

    public IEnumerator Check_()
    {
        string address = "http://kbssj1.cafe24.com/Universal/CreateUser/idOverlapcheck.php";

        WWWForm cFrom = new WWWForm();
        BaseNetwork.Base(ref cFrom);

        cFrom.AddField("id", ID_InputField.text);

        WWW wwwUrl = new WWW(address, cFrom);
        //yield return new WaitForSeconds(TimeSpeed);
        yield return wwwUrl;

        if (wwwUrl.text.Equals("idable"))
        {
            isIdOverlapCheck = true;
            message.ShowInfoText("아이디 사용이 가능합니다.");
        }
        else
        {
            message.ShowInfoText("아이디가 존재합니다.");
        }
    }

    public void OverlapCheck()
    {
        StartCoroutine(Check_());
    }

    public void MakeID()
    {
        if(isIdOverlapCheck == false)
        {
            message.ShowInfoText("아이디 중복체크를 해주십시오.");
        }
        else
        {
            if (PASSWORD_InputField.text.Equals(PASSWORDCHECK_InputField.text))
            {
                if (isContainHangul(ID_InputField.text))
                {
                    message.ShowInfoText("아이디는 한글을 포함할 수 없습니다.");
                }
                else
                {
                    if (IdSpaceExist(ID_InputField.text))
                    {
                        message.ShowInfoText("아이디는 공백을 포함할 수 없습니다.");
                    }
                    else
                    {
                        StartCoroutine(MakeID_());
                    }                  
                }
            }
            else
            {
                message.ShowInfoText("비밀번호가 일치하지 않습니다.");
            }
        }       
    }

    public IEnumerator MakeID_()
    {
        string address = "http://kbssj1.cafe24.com/Universal/CreateUser/createUser.php";

        WWWForm cFrom = new WWWForm();
        BaseNetwork.Base(ref cFrom);

        cFrom.AddField("id", ID_InputField.text);
        cFrom.AddField("password", PASSWORD_InputField.text);

        WWW wwwUrl = new WWW(address, cFrom);

        yield return wwwUrl;

        if (wwwUrl.text.Equals("UserCreateSuccess"))
        {
            message.ShowInfoText("아이디 생성이 완료되었습니다.");
            mainControl.Close_CreateID_Screen();
            mainControl.Open_Login_Screen();
        }
        else
        {
            message.ShowInfoText("아이디 생성이 실패했습니다.");
        }
    }

    public bool isContainHangul(string s)
    {
        char[] charArr = s.ToCharArray();

        foreach (char c in charArr)
        {
            if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
            {
                return true;
            }
        }
        return false;
    }
    public bool IdSpaceExist(string str)
    {
        if (str.IndexOf(' ') > -1)
        {
            return true;
        }

        return false;
    }
}
