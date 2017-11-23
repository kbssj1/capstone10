using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour {

    public string version = "v1.0";
    public GameObject []roomList = new GameObject[6];
    public Text AI_Text;
    public InfoMessage message;


    void Awake()
    {
        PhotonNetwork.ConnectUsingSettings(version);
    }

    void OnConnectedToMaster()
    {
        Debug.Log("Connected Master");
        OnReceivedRoomListUpdate();
        // PhotonNetwork.JoinRandomRoom();
    }

    void OnJoinedLobby()
    {
        Debug.Log("Entered Lobby");
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("No Rooms !");
        PhotonNetwork.CreateRoom("MyRoom");
    }

    void OnJoinedRoom()
    {
        StartCoroutine(this.LoadGameField());
    }

    public void OnClikJoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnClickCreateRoom()
    {
        if (GetRoomCount() == 6)
        {
            message.ShowInfoText("만들 수 있는 방 갯수가 최대입니다.");
        }
        else
        {
            string _roomName = "Room_" + Random.Range(0, 999).ToString("000");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = 2;

            PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        }
    }

    void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        Debug.Log("Create Room Failed = " + codeAndMsg[1]);
    }
    
    void OnReceivedRoomListUpdate()
    {
        for(int i=0;i< roomList.Length; i++)
        {
            roomList[i].SetActive(false);
        }

        int index = 0;
        foreach (RoomInfo _room in PhotonNetwork.GetRoomList())          
        {
            roomList[index].SetActive(true);
            roomList[index].transform.GetChild(0).GetComponent<Text>().text = _room.Name;

            if (_room.PlayerCount >= 2)
            {
                roomList[index].transform.GetChild(1).gameObject.SetActive(false);
                roomList[index].transform.GetChild(2).gameObject.SetActive(true);
            }
            else
            {
                roomList[index].transform.GetChild(1).gameObject.SetActive(true);
                roomList[index].transform.GetChild(2).gameObject.SetActive(false);
                roomList[index].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                {
                    PhotonNetwork.JoinRoom(_room.Name);
                });
            }
            
            ++index;

            if (index == roomList.Length)
                break;
        }              
    }
    
    IEnumerator LoadGameField()
    {
        PhotonNetwork.isMessageQueueRunning = false;
        AsyncOperation ao = SceneManager.LoadSceneAsync("MainScene");
        yield return ao;
    }

    private int GetRoomCount()
    {
        int Count = 0;
        for(int i=0; i< roomList.Length; i++)
        {
            if (roomList[i].activeSelf == true)
                ++Count;
        }

        return Count; 
    }

    public void GotoScene_Login_Btn()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Login");
    }

    public void AI_Btn()
    {
       
        if (GameManager.Instance.aIMode)
        {
            AI_Text.text = "2인용";
            GameManager.Instance.aIMode = false;
        }
        else
        {
            AI_Text.text = "1인용";
            GameManager.Instance.aIMode = true;
        }
    }

    /*
    void OnGUI()
    {
        Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
    */
}
