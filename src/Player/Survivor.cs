using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;

public class Survivor : MonoBehaviour, IListener {

    public enum PlayerState { Idle = 0, Walk = 1, Run = 2, Crouch = 3, CrouchWalk = 4, Hit = 5, Die = 6, Gram = 7, Radio=8, Key=9 };

    private const int HeroInitHP = 100;
    private const float GaugeAdd = 19f;
    private const float SurvivorInitSpeed = 1.5f;
    private const float HitBreakTime = 0.7f;
    private const float SpeedRotationInit = 50;

    [Header("Canvas Settings")]
    public Transform CountDown;
    public Transform GameOver;
    public Transform JoyRightBtn;
    public Transform JoyLeftBtn;
    public Transform GramGuage;
    public Image Life;
    public Image Guage;
    

    [Header("Animation Settings")]
    public Animator Animator;
    [SerializeField]
    public PlayerState Playerstate;
    private int State;
    public GameObject[] Head;

    [Header("Controller Settings")]
    public CharacterController Controller;
    public Transform PlayerTr;
    public float SpeedRotation = SpeedRotationInit;
    public float WalkSpeed;
    public float RunSpeed;
    private float SurivivorSpeed = SurvivorInitSpeed;

    private Vector3 MoveDirection = Vector3.zero;
    private float Horizontal = 0f;
    private float Vertical = 0f;
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
    public GameObject Camera;
    public PhotonView Pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    [Header("Audio Settings")]
    public Survivor_Audio survivor_audio;
    public Survivor_Audio2 survivor_heart_audio;
    public Etc_Audio etcAudio;
    public AudioManager audioManager;

    Stopwatch sw;
    // Use this for initialization
    void Awake()
    {
        ItemKey = false;
        Die = false;
        Run = false;
        Crouch = false;
        Moving = false;
        Gram = false;
        Radio = false;
        Key = false;

        Hp = HeroInitHP;

        Playerstate = PlayerState.Idle;
        State = 0;
        Pv.synchronization = ViewSynchronization.UnreliableOnChange;

        Pv.ObservedComponents[0] = this;

        currPos = PlayerTr.position;
        currRot = PlayerTr.rotation;

        gameStart = false;
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


        if (Pv.isMine)
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
        Guage.fillAmount = (sw.ElapsedMilliseconds / 1000) / GaugeAdd;
        
        if (!Die && Playerstate == PlayerState.Idle && !Gram && gameStart && !Radio && !Key)
        {
            if (Pv.isMine)
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
        if (survivor_audio.GetCheck())
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Walk_01"))
            {
                survivor_audio.PlayAudio("WALK");
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Run_02"))
            {
                survivor_audio.PlayAudio("RUN");
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidCrouchWalk 0"))
            {
                survivor_audio.PlayAudio("CROUCH_WALK");
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidIdle"))
            {
                survivor_audio.PlayAudio("NOT");
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
            switch (Playerstate)
            {
                case PlayerState.Idle:
                    AnimationExcute("Idle");
                    break;
                case PlayerState.Walk:
                    SurivivorSpeed = WalkSpeed;
                    AnimationExcute("Walk");
                    break;
                case PlayerState.Run:
                    SurivivorSpeed = RunSpeed;
                    AnimationExcute("Run");
                    break;
                case PlayerState.Crouch:
                    AnimationExcute("Crouch");
                    break;
                case PlayerState.CrouchWalk:
                    SurivivorSpeed = WalkSpeed;
                    AnimationExcute("CrouchWalk");
                    break;
                case PlayerState.Hit:
                    AnimationExcute("Hit");
                    break;
                case PlayerState.Die:
                    AnimationExcute("Die");
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_DIE, this);
                    break;
                case PlayerState.Gram:
                    AnimationExcute("Gram");
                    break;
                case PlayerState.Radio:
                    AnimationExcute("Radio");
                    break;
                case PlayerState.Key:
                    AnimationExcute("Key");
                    break;
            }
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

    #region
    IEnumerator RemotePlayerAction()
    {
        while (true)
        {
            switch (State)
            {
                case (int)PlayerState.Idle:
                    sendMoveEvent(false,"");
                    AnimationExcute("Idle");
                    break;

                case (int)PlayerState.Walk:
                    sendMoveEvent(false, "");
                    SurivivorSpeed = WalkSpeed;
                    AnimationExcute("Walk");
                    break;

                case (int)PlayerState.Run:
                    sendMoveEvent(true, "");
                    SurivivorSpeed = RunSpeed;
                    AnimationExcute("Run");
                    break;

                case (int)PlayerState.Crouch:
                    sendMoveEvent(false, "");
                    AnimationExcute("Crouch");
                    break;

                case (int)PlayerState.CrouchWalk:

                    sendMoveEvent(false, "");
                    SurivivorSpeed = WalkSpeed;
                    AnimationExcute("CrouchWalk");

                    break;

                case (int)PlayerState.Hit:

                    sendMoveEvent(true, "");
                    AnimationExcute("Hit");

                    break;

                case (int)PlayerState.Die:

                    sendMoveEvent(true, "");
                    AnimationExcute("Die");
                    EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_DIE, this);
                    break;

                case (int)PlayerState.Gram:

                    sendMoveEvent(true, "Gram");
                    AnimationExcute("Gram");

                    break;
                case (int)PlayerState.Radio:

                    sendMoveEvent(true, "Radio");
                    AnimationExcute("Radio");

                    break;
                case (int)PlayerState.Key:

                    sendMoveEvent(true, "");
                    AnimationExcute("Key");

                    break;
            }
            yield return null;
        }

    }
    #endregion

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_HIT:

                if (Pv.isMine && Playerstate == PlayerState.Idle)
                {
                    if (Playerstate != PlayerState.Die)
                    {
                        StartCoroutine("Hit", Param);
                        Pv.RPC("Hit", PhotonTargets.Others, Param);
                    }
                }

                break;

            case EVENT_TYPE.SURVIVOR_GRAMCTRL:

                if (Pv.isMine)
                {
                    GramCtrl();
                    Pv.RPC("GramCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_GRAM_SUC:

                if (Pv.isMine)
                {
                    GramSuccess();
                    Pv.RPC("GramSuccess", PhotonTargets.Others, null);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIOCTRL:

                if (Pv.isMine)
                {
                    RadioCtrl(Param);
                    Pv.RPC("RadioCtrl", PhotonTargets.Others, Param);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIO_SUC:

                if (Pv.isMine)
                {
                    
                    RadioSuccess();
                    Pv.RPC("RadioSuccess", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL:

                if (Pv.isMine)
                {
                    KeyCtrl();
                    Pv.RPC("KeyCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL2:

                if (Pv.isMine)
                {
                    KeyCtrl2();
                    Pv.RPC("KeyCtrl2", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET:

                if (Pv.isMine)
                {
                    KeyGetItem();
                    Pv.RPC("KeyGetItem", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET2:

                if (Pv.isMine)
                {
                    KeyGetItem2();
                    Pv.RPC("KeyGetItem2", PhotonTargets.Others, null);
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
        yield return new WaitForSeconds(HitBreakTime);

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
        if(Pv.isMine)
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
