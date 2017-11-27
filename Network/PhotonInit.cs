using UnityEngine;
using System.Collections;

public class PhotonInit : MonoBehaviour {

    

    public enum NetDef
    {
        NotStated =0,
        Server,
        Cliet
    }
    public static NetDef def = NetDef.NotStated;

    void Awake()
    {
        Debug.Log("Enter Room");
        CreateCharacter();
        PhotonNetwork.isMessageQueueRunning = true;
    }
    /*
    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
    */
    void CreateCharacter()
    {
        if(PhotonNetwork.playerList.Length == 1)
        {
            def = NetDef.Server;
            PhotonNetwork.Instantiate("Murderer", new Vector3(-1.79f, 7.91f, -13.68f), Quaternion.identity, 0);
        }
        else
        {
            def = NetDef.Cliet;
            PhotonNetwork.Instantiate("Woman", new Vector3(1.93f, 7.91f, -13.68f), Quaternion.identity, 0);
        }
    }
}
