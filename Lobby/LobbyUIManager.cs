using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour {

    public bl_ProgressBar LoadingBar;
    public GameObject LoadingWindow;
    public GameObject LobbyWindow;
    public Text loadingMessage;
    public Text UserInfo_Text;

    public InfoMessage message;

    private bool isLoading = false;

    void Start()
    {
        UserInfo_Text.text = GameInformation.id + "     " + "현재 전적 : 0승 0패";
        //Debug.Log("Check");
        if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
        {
            LoadUi_LobbyWindow();
        }
        else
        {
            LoadUI_LoadingWindow();
        }
    }

    void Update()
    {
        if (isLoading)
        {
            loadingMessage.text = PhotonNetwork.connectionStateDetailed.ToString();

            if (PhotonNetwork.connectionStateDetailed == ClientState.ConnectingToNameServer)
            {
                LoadingBar.Value = 30;
            }
            else if(PhotonNetwork.connectionStateDetailed == ClientState.ConnectingToMasterserver)
            {
                LoadingBar.Value = 60;
            }
            else if(PhotonNetwork.connectionStateDetailed == ClientState.Authenticated)
            {
                LoadingBar.Value = 90;
            }
            else if(PhotonNetwork.connectionStateDetailed == ClientState.JoinedLobby)
            {
                LoadingBar.Value = 100;
                LoadUi_LobbyWindow();
            }
        }
    }

    public void LoadUI_LoadingWindow()
    {
        isLoading = true;
        LoadingWindow.SetActive(true);
        LobbyWindow.SetActive(false);
    }

    public void LoadUi_LobbyWindow()
    {
        isLoading = false;
        LoadingWindow.SetActive(false);
        LobbyWindow.SetActive(true);
    }
}
