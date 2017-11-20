using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;

public class Survivor : MonoBehaviour, IListener {

    public enum PlayerState { Idle = 0, Walk = 1, Run = 2, Crouch = 3, CrouchWalk = 4, Hit = 5, Die = 6, Gram = 7, Radio=8, Key=9 };

    private const int heroInitHP = 100;
    private const float gaugeAdd = 19f;
    private const float survivorInitSpeed = 1.5f;
    private const float hitBreakTime = 0.7f;
    private const float rotationInitSpeed = 50;

    [Header("Canvas Settings")]
    [SerializeField]
    private Transform CountDown;
    [SerializeField]
    private Transform GameOver;
    [SerializeField]
    private Transform JoyRightBtn;
    [SerializeField]
    private Transform JoyLeftBtn;
    [SerializeField]
    private Transform GramGuage;
    [SerializeField]
    private Image Life;
    [SerializeField]
    private Image Guage;
    

    [Header("Animation Settings")]
    [SerializeField]
    private Animator Animator;
    [SerializeField]
	public PlayerState Playerstate;
    [SerializeField]
    private int State;
    [SerializeField]
    private GameObject[] Head;

    [Header("Controller Settings")]
    [SerializeField]
    private CharacterController Controller;
    [SerializeField]
    private Transform PlayerTr;
    [SerializeField]
	private float SpeedRotation ;
    [SerializeField]
    private float WalkSpeed;
    [SerializeField]
    private float RunSpeed;
	private float SurivivorSpeed;

    private Vector3 MoveDirection = Vector3.zero;
	private float Horizontal;
	private float Vertical;
    private float Rotate;

    [Header("Character Settings")]
    private bool Die;
    private bool Run;
    private bool Crouch;
    private bool Gram;
    private bool Radio;
    private bool Key;
    private int Hp;
    private bool ItemKey;
    private bool Moving;
    private bool gameStart;

    [Header("Photon Settings")]
    [SerializeField]
    private GameObject Camera;
    private PhotonView pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    [Header("Audio Settings")]
    public Survivor_Audio survivor_audio;
    public Survivor_Audio2 survivor_heart_audio;
    public Etc_Audio etcAudio;
    public AudioManager audioManager;

    Stopwatch sw;
    // Use this for initialization
	void Initailize(){
		Horizontal = 0f;
		Vertical = 0f;
		SpeedRotation = rotationInitSpeed;
		SurivivorSpeed = survivorInitSpeed;
		ItemKey = false;
		Die = false;
		Run = false;
		Crouch = false;
		Moving = false;
		Gram = false;
		Radio = false;
		Key = false;

		Hp = heroInitHP;

		Playerstate = PlayerState.Idle;
		State = 0;
		pv.synchronization = ViewSynchronization.UnreliableOnChange;

		pv.ObservedComponents[0] = this;

		currPos = PlayerTr.position;
		currRot = PlayerTr.rotation;

		gameStart = false;
	}
    void Awake()
    {
		Initailize ();
        sw = new Stopwatch();
        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();
    }

    void Start()
    {
        for (EVENT_TYPE i = EVENT_TYPE.SURVIVOR_HIT; i < EVENT_TYPE.KEY_GET2; i++)
        {
            EventManager.Instance.AddListener(i, this);
        }
        EventManager.Instance.AddListener(EVENT_TYPE.COUNT_DOWN, this);     
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_WIN, this);


        if (pv.isMine)
        {
            Head[0].GetComponent<SkinnedMeshRenderer>().enabled = false;
            Head[1].SetActive(false);
            Camera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());

        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_CREATE, this);

        audioManager = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<AudioManager>();
    }

    void Update()
    {
        Guage.fillAmount = (sw.ElapsedMilliseconds / 1000) / gaugeAdd;
        
        if (!Die && Playerstate == PlayerState.Idle && !Gram && gameStart && !Radio && !Key)
        {
            if (pv.isMine)
            {
                Horizontal = Input.GetAxis("Horizontal");
                Vertical = Input.GetAxis("Vertical");
                Rotate = Input.GetAxis("Oculus_GearVR_RThumbstickX") * SpeedRotation; // 회전

                if (Input.GetButtonDown("Jump"))
                {
                    Run = true;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    Run = false;
                }

                Vector3 desiredMove = transform.forward * Vertical + transform.right * Horizontal;
                MoveDirection.x = desiredMove.x * SurivivorSpeed;
                MoveDirection.y -= 9.8f;
                MoveDirection.z = desiredMove.z * SurivivorSpeed;
                Controller.Move(MoveDirection * Time.deltaTime);
                transform.Rotate(0, Rotate * Time.deltaTime, 0);
            }

            else
            {
                PlayerTr.position = Vector3.Lerp(PlayerTr.position, currPos, Time.deltaTime * 3f);
                PlayerTr.rotation = Quaternion.Slerp(PlayerTr.rotation, currRot, Time.deltaTime * 3f);

            }
        }

        #region Audio
		if (survivor_audio.isAudioPlay()){
			string playAudioType="";
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Walk_01"))
            {
				playAudioType="WALK";
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Run_02"))
            {
				playAudioType="RUN";   
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidCrouchWalk 0"))
            {
				playAudioType="CROUCH_WALK";    
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidIdle"))
            {
				playAudioType="NOT";  
            }
			survivor_audio.PlayAudio(playAudioType);
		}
            #endregion
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(PlayerTr.position);
            stream.SendNext(PlayerTr.rotation);
            stream.SendNext((int)Playerstate);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            State = (int)stream.ReceiveNext();
        }
    }

    #region
    IEnumerator PlayerStateCheck()
    {
        while (true)
        {
            if (Horizontal == 0 && Vertical == 0 && Rotate == 0)
            {
                Playerstate = Crouch ? PlayerState.Crouch : PlayerState.Idle;
            }
            else if (Horizontal != 0 || Vertical != 0 || Rotate != 0)
            {
                if (Crouch)
                {
                    if (Playerstate == PlayerState.Crouch)
                        Playerstate = PlayerState.CrouchWalk;
                    else if (Playerstate != PlayerState.CrouchWalk)
                        Playerstate = PlayerState.Crouch;
                }

                else
                {
                    if (Playerstate == PlayerState.CrouchWalk)
                        Playerstate = PlayerState.Crouch;
                    else if (Playerstate == PlayerState.Crouch)
                        Playerstate = PlayerState.Idle;
                    else
                    {
                        Playerstate = Run ? PlayerState.Run : PlayerState.Walk;
                    }
                }

            }

            if (Die)
            {
                Playerstate = PlayerState.Die;              
            }
            if (Playerstate != PlayerState.Die && Playerstate == PlayerState.Idle)
            {
                Playerstate = PlayerState.Hit;
            }
            if (Gram)
                Playerstate = PlayerState.Gram;
            if (Radio)
                Playerstate = PlayerState.Radio;
            if (Key)
                Playerstate = PlayerState.Key;
            yield return null;
        }
    }
    #endregion

    #region
    IEnumerator PlayerAction()
    {
        while (true)
        {
			string animationType="";
            switch (Playerstate)
            {
				case PlayerState.Idle:
					animationType = "Idle";
                    break;
                case PlayerState.Walk:
					animationType = "Walk";
                    SurivivorSpeed = WalkSpeed;
                   
                    break;
                case PlayerState.Run:
				animationType = "Run";
                    SurivivorSpeed = RunSpeed;
              
                    break;
                case PlayerState.Crouch:
				animationType = "Crouch";
                  
                    break;
                case PlayerState.CrouchWalk:
				animationType = "CrouchWalk";
                    SurivivorSpeed = WalkSpeed;
                  
                    break;
                case PlayerState.Hit:
				animationType = "Hit";
                   
                    break;
                case PlayerState.Die:
				animationType = "Die";
                  
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_DIE, this);
                    break;
                case PlayerState.Gram:
				animationType = "Gram";
                  
                    break;
                case PlayerState.Radio:
				animationType = "Radio";
                    
                    break;
                case PlayerState.Key:
				animationType = "Key";
                    
                    break;
            }
			AnimationExcute(animationType);
            yield return null;
        }

    }


    public void sendMoveEvent(bool flag,string trap)
    {

        if (!Moving && flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this);
            Moving = true;
        }

        else if (Moving && !flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_STOP, this);
            Moving = false;
        }
        if(!trap.Equals(""))
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this,trap);
        }
    }

    public bool getGramCtrl()
    {
        return Gram;
    }
    public void setGramCtrl(bool flag)
    {

        Gram = flag;
    }

    public bool getItemKey()
    {
        return ItemKey;
    }
    public void setItemKey(bool flag)
    {

        ItemKey = flag;
    }
    public bool getRadioCtrl()
    {
        return Radio;
    }
    public void setRadioCtrl(bool flag)
    {

        Radio = flag;
    }

    public bool getKeyCtrl()
    {
        return Key;
    }
    public void setKeyCtrl(bool flag)
    {
        Key = flag;
    }

    public Animator GetAni()
    {
        return Animator;
    }

    #region
    IEnumerator RemotePlayerAction()
    {
        while (true)
        {
			string animationType = "";

            switch (State)
            {
			case (int)PlayerState.Idle:
					animationType = "Idle";
                    sendMoveEvent(false,"");
                   
                    break;

                case (int)PlayerState.Walk:
				animationType = "Walk";
                    sendMoveEvent(false, "");
                    SurivivorSpeed = WalkSpeed;
               
                    break;

                case (int)PlayerState.Run:
				animationType = "Run";
                    sendMoveEvent(true, "");
                    SurivivorSpeed = RunSpeed;
                    
                    break;

                case (int)PlayerState.Crouch:
				animationType = "Crouch";
                    sendMoveEvent(false, "");
                   
                    break;

                case (int)PlayerState.CrouchWalk:
				animationType = "CrouchWalk";
                    sendMoveEvent(false, "");
                    SurivivorSpeed = WalkSpeed;
                 

                    break;

                case (int)PlayerState.Hit:
				animationType = "Hit";
                    sendMoveEvent(true, "");
                 

                    break;

                case (int)PlayerState.Die:
				animationType = "Die";
                    sendMoveEvent(true, "");
                  
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_DIE, this);
                    break;

                case (int)PlayerState.Gram:
				animationType = "Gram";
                    sendMoveEvent(true, "Gram");
                 

                    break;
                case (int)PlayerState.Radio:
				animationType = "Radio";
                    sendMoveEvent(true, "Radio");
                

                    break;
                case (int)PlayerState.Key:
				animationType = "Key";
                    sendMoveEvent(true, "");
                   

                    break;
            }
			AnimationExcute(animationType);
            yield return null;
        }

    }
    #endregion

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_HIT:

                if (pv.isMine && Playerstate == PlayerState.Idle)
                {
                    if (Playerstate != PlayerState.Die)
                    {
                        StartCoroutine("Hit", Param);
                        pv.RPC("Hit", PhotonTargets.Others, Param);
                    }
                }

                break;

            case EVENT_TYPE.SURVIVOR_GRAMCTRL:

                if (pv.isMine)
                {
                    GramCtrl();
                    pv.RPC("GramCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_GRAM_SUC:

                if (pv.isMine)
                {
                    GramSuccess();
                    pv.RPC("GramSuccess", PhotonTargets.Others, null);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIOCTRL:

                if (pv.isMine)
                {
                    RadioCtrl(Param);
                    pv.RPC("RadioCtrl", PhotonTargets.Others, Param);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIO_SUC:

                if (pv.isMine)
                {                    
                    RadioSuccess();
                    pv.RPC("RadioSuccess", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL:

                if (pv.isMine)
                {
                    KeyCtrl();
                    pv.RPC("KeyCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL2:

                if (pv.isMine)
                {
                    KeyCtrl2();
                    pv.RPC("KeyCtrl2", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET:

                if (pv.isMine)
                {
                    KeyGetItem();
                    pv.RPC("KeyGetItem", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET2:

                if (pv.isMine)
                {
                    KeyGetItem2();
                    pv.RPC("KeyGetItem2", PhotonTargets.Others, null);
                }
                
                break;
            case EVENT_TYPE.TIME_OVER:
                OnTimerEnd();
                break;
            case EVENT_TYPE.COUNT_DOWN:
                OnCountDown();
                break;

            case EVENT_TYPE.TIME_START:
                gameStart = true;
                break;

            case EVENT_TYPE.SURVIVOR_DIE:
                SurvivorDead();
                break;
            case EVENT_TYPE.SURVIVOR_WIN:
                SurvivorWin();
                break;
            case EVENT_TYPE.B_RIGHT_BTN_POSSIBLE:
                OnBtnRight_B(true);
                break;
            case EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE:
                OnBtnRight_B(false);
                break;
        };
    }

    [PunRPC]
    public void KeyGetItem() 
    {
        setItemKey(true);
        EventManager.Instance.PostNotification(EVENT_TYPE.KEY_GET_SUCCESS, this);
        audioManager.key_audio1.PlayAudio();
    }  // 키 아이템 얻었을때

    [PunRPC]
    public void KeyGetItem2() 
    {
        setItemKey(true);
        EventManager.Instance.PostNotification(EVENT_TYPE.KEY_GET_SUCCESS2, this);
        audioManager.key_audio2.PlayAudio();
    }

    [PunRPC]
    public void GramSuccess()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_SUCCESS, this);
        audioManager.mongue_audio2.PlayAudio();
    }  // 시체 안치실 열릴때
    
    [PunRPC]
    public void RadioSuccess()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.RADIO_SUCCESS, this);
        audioManager.mongue_audio1.PlayAudio();
    }  // 시체 안치실 열릴 때

    [PunRPC]
    public void GramCtrl() 
    {
        if (Playerstate != PlayerState.Crouch && Playerstate != PlayerState.CrouchWalk)
        {

            if (!getGramCtrl())
            {
                setGramCtrl(true);
                audioManager.gramophone_Audio_BGM.PlayBGMAudio();
                audioManager.gramophone_Audio_effect.PlayAudio();
                EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_START, this);
                OnBtnRight_B(false);
                OnGramGuage(true);
            }
            else
            {
                setGramCtrl(false);
                audioManager.gramophone_Audio_BGM.StopAudio();
                audioManager.gramophone_Audio_effect.StopAudio();
                EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_STOP, this);
                OnBtnRight_B(true);
                OnGramGuage(false);
            }
        }
    } // 축음기

    [PunRPC]
    public void KeyCtrl()
    {
        if (Playerstate != PlayerState.Crouch && Playerstate != PlayerState.CrouchWalk)
        {
            if (!getKeyCtrl())
            {
                setKeyCtrl(true);
                EventManager.Instance.PostNotification(EVENT_TYPE.KEY_SUCCESS, this);
                audioManager.elvator_btn2.PlayAudio();
                OnBtnRight_B(false);
            }

            else
            {
                setKeyCtrl(false);
                OnBtnRight_B(true);
            }
        }
    } // 엘레베이터 오른쪽

    [PunRPC]
    public void KeyCtrl2()
    {
        if (Playerstate != PlayerState.Crouch && Playerstate != PlayerState.CrouchWalk)
        {
            if (!getKeyCtrl())
            {
                setKeyCtrl(true);
                EventManager.Instance.PostNotification(EVENT_TYPE.KEY_SUCCESS2, this);
                audioManager.elvator_btn1.PlayAudio();
                OnBtnRight_B(false);
            }

            else
            {
                setKeyCtrl(false);
                OnBtnRight_B(true);
            }
        }
    } // 엘레베이터 왼쪽

    [PunRPC]
    public void RadioCtrl(object Param)
    {
        if (Playerstate != PlayerState.Crouch && Playerstate != PlayerState.CrouchWalk)
        {
            if (Param == null)
            {
                if (!getRadioCtrl())
                {
                    setRadioCtrl(true);
                    audioManager.radio_BGM_Audio.PlayAudio();
                    audioManager.radio_effect_Audio.PlayAudio();
                    OnBtnRight_B(false);
                    OnBtnLeft_R(true);
                }
                else
                {
                    setRadioCtrl(false);
                    audioManager.radio_BGM_Audio.PauseAudio();
                    audioManager.radio_effect_Audio.PauseAudio();
                    OnBtnRight_B(true);
                    OnBtnLeft_R(false);
                }
            }
            else
            {
                audioManager.radio_BGM_Audio.SetVolume();
                Animator.SetBool("RadioCtrl", true); // 한 번씩 작동될때
                OnBtnRight_B(false);
            }
        }
    }

    [PunRPC]
    public IEnumerator Hit(object Param)
    {
        survivor_audio.PlayAudio("ATTACKED", true);
        int damage = (int)Param;
        Hp -= damage;
        Life.fillAmount = Life.fillAmount - 0.25f;

        StateHit();
        yield return new WaitForSeconds(hitBreakTime);

        if (Hp <= 0)
        {
            survivor_audio.PlayAudio("DEATH", true);
            Die = true;
        }

    }

    public void StateHit()
    {
       
        if (getGramCtrl())
        {
           
            GramCtrl();
        }
        else if(getRadioCtrl())
        {            
            RadioCtrl(null);
        }
        else if (getKeyCtrl())
        {
            KeyCtrl();
            KeyCtrl2();
        }
        
    }

    public void SurvivorDead()
    {
        gameStart = false;
        //transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("LOSE");
        GameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void SurvivorWin()
    {
        gameStart = false;
        if(pv.isMine)
            GameOver.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }
    public void OnTimerEnd()
    {
        gameStart = false;
        //transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("LOSE");
        GameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void OnCountDown()
    {
       // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("CountDown").GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        CountDown.GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        etcAudio.PlayAudio("COUNTDOWN");
    }
    #endregion

    public void OnCountEnd()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.TIME_START, this);
    }

    public void OnKeyEnd()
    {
        KeyCtrl();
    }

    public PhotonView GetPhotonView()
    {
        return pv;
    }

    public void OnBtnRight_B(bool flag)
    {
        JoyRightBtn.GetComponentInChildren<Animator>().SetBool("Btn_B", flag);
    }
    
    public void OnBtnLeft_R(bool flag)
    {
        JoyLeftBtn.GetComponentInChildren<Animator>().SetBool("Btn_L", flag);
    }
    public void OnGramGuage(bool flag)
    {
        if (flag)
        {
            sw.Start();         
        }
        else
        {
            sw.Stop();
        }
        GramGuage.gameObject.SetActive(flag);
    }
    public void AnimationExcute(string name)
    {

        Animator.SetBool("Idle", false);
        Animator.SetBool("Walk", false);
        Animator.SetBool("Run", false);
        Animator.SetBool("Crouch", false);
        Animator.SetBool("CrouchWalk", false);
        Animator.SetBool("Die", false);
        Animator.SetBool("Hit", false);
        Animator.SetBool("Gram", false);
        Animator.SetBool("Radio", false);
        Animator.SetBool("Key", false);
        Animator.SetBool(name, true);
    }
  
}
