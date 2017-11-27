using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {

    #region

    public static GameManager Instance
    {
        get { return instance; }
        set { }
    }

    private static GameManager instance = null;

    #endregion

    public enum NetDef
    {
        NotStated = 0,
        Server,
        Cliet
    }
    public static NetDef def = NetDef.NotStated;

    public bool aIMode = false;
    public GameObject AI;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        //Application.targetFrameRate = 50;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            DestroyImmediate(this);
    }

    void CreateCharacter()
    {
        if (PhotonNetwork.playerList.Length == 1)
        {
            def = NetDef.Server;
            PhotonNetwork.Instantiate("Survivor", new Vector3(-18.21f, -17.611f, -7.43f), Quaternion.identity, 0);

            if (!aIMode)
            {
                AI = GameObject.FindGameObjectWithTag("MURDERER");
                AI.SetActive(false);
            }
           
        }

        else
        {
            def = NetDef.Cliet;
            PhotonNetwork.Instantiate("Murderer", new Vector3(-11.64f, -17.52f, 7.28f), Quaternion.identity, 0);

            if (!aIMode)
            {
                AI = GameObject.FindGameObjectWithTag("MURDERER");
                AI.SetActive(false);
                
            }
          
        }
        
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name.Equals("MainScene"))
        {
            
            CreateCharacter();
            PhotonNetwork.isMessageQueueRunning = true;
        }

    }
}
