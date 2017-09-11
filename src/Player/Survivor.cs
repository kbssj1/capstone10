using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;

public class Survivor : MonoBehaviour, IListener {

    public enum PlayerState { Idle = 0, Walk = 1, Run = 2, Crouch = 3, CrouchWalk = 4, Hit = 5, Die = 6, Gram = 7, Radio=8, Key=9 };

    [Header("Canvas Settings")]
    public Transform CountDown;
    public Transform GameOver;
    public Transform JoyRightBtn;
    public Transform JoyLeftBtn;
    public Transform GramGuage;
    public Image _life;
    public Image _guage;
    private float time = 19f;

    [Header("Animation Settings")]
    public Animator m_Animator;
    [SerializeField]
    public PlayerState m_PlayerState;
    private int m_State;
    public GameObject[] m_Head;

    [Header("Controller Settings")]
    public CharacterController m_Controller;
    public Transform m_PlayerTr;
    public float m_speedRotation = 50;
    public float m_WalkSpeed;
    public float m_RunSpeed;
    private float m_speed = 1.5f;

    private Vector3 m_moveDirection = Vector3.zero;
    private float m_horizontal = 0f;
    private float m_vertical = 0f;
    private float m_rotate;

    [Header("Character Settings")]
    private bool isDie;
    private bool m_IsRun;
    private bool m_IsCrouch;
    private bool m_IsHit;
    private bool m_Gram;
    private bool m_Radio;
    private bool m_Key;
    private int m_HP;
    private bool m_ItemKey;
    private bool m_IsMoving;


    private bool gameStart;

    [Header("Photon Settings")]
    public GameObject m_Camera;
    public PhotonView m_pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    #region Audio
    [Header("Audio Settings")]
    public Survivor_Audio survivor_audio;
    public Survivor_Audio2 survivor_heart_audio;
    private int HeartBeat = 0;
    public Etc_Audio etcAudio;
    public AudioManager audioManager;

    float cooltime = 19f;
    float leftTime = 19f;

    Stopwatch sw;
    #endregion
    // Use this for initialization
    void Awake()
    {
        m_ItemKey = false;
        isDie = false;
        m_IsRun = false;
        m_IsCrouch = false;
        m_IsHit = false;
        m_IsMoving = false;
        m_Gram = false;
        m_Radio = false;
        m_Key = false;

        m_HP = 100;

        m_PlayerState = PlayerState.Idle;
        m_State = 0;
        m_pv.synchronization = ViewSynchronization.UnreliableOnChange;

        m_pv.ObservedComponents[0] = this;

        currPos = m_PlayerTr.position;
        currRot = m_PlayerTr.rotation;

        gameStart = false;
        sw = new Stopwatch();
        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();
    }

    void Start()
    {
        
        
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_HIT, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_KEYCTRL, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_KEYCTRL2, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_RADIOCTRL, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_RADIO_SUC, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_GRAM_SUC, this);

        EventManager.Instance.AddListener(EVENT_TYPE.KEY_GET, this);
        EventManager.Instance.AddListener(EVENT_TYPE.KEY_GET2, this);
        EventManager.Instance.AddListener(EVENT_TYPE.COUNT_DOWN, this);
        EventManager.Instance.AddListener(EVENT_TYPE.B_RIGHT_BTN_POSSIBLE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE, this);

        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_WIN, this);


        if (m_pv.isMine)
        {
            m_Head[0].GetComponent<SkinnedMeshRenderer>().enabled = false;
            m_Head[1].SetActive(false);
            m_Camera.SetActive(true);
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

        _guage.fillAmount = (sw.ElapsedMilliseconds / 1000) / time;

        //Debug.Log(HeartBeat);
        if (!isDie && !m_IsHit && !m_Gram && gameStart && !m_Radio && !m_Key)
        {
            if (m_pv.isMine)
            {
                m_horizontal = Input.GetAxis("Horizontal");
                m_vertical = Input.GetAxis("Vertical");
                m_rotate = Input.GetAxis("Oculus_GearVR_RThumbstickX") * m_speedRotation; // 회전

                if (Input.GetButtonDown("Jump"))
                {

                    m_IsRun = true;

                }

                else if (Input.GetButtonUp("Jump"))
                {

                    m_IsRun = false;

                }
                /*
                if (Input.GetButtonDown("Fire1"))
                {
                    m_IsCrouch = !m_IsCrouch;
                    survivor_audio.PlayAudio("CROUCH", true);
                }
                */

                Vector3 desiredMove = transform.forward * m_vertical + transform.right * m_horizontal;


                m_moveDirection.x = desiredMove.x * m_speed;
                m_moveDirection.y -= 9.8f;
                m_moveDirection.z = desiredMove.z * m_speed;

                m_Controller.Move(m_moveDirection * Time.deltaTime);


                transform.Rotate(0, m_rotate * Time.deltaTime, 0);
            }

            else
            {
                m_PlayerTr.position = Vector3.Lerp(m_PlayerTr.position, currPos, Time.deltaTime * 3f);
                m_PlayerTr.rotation = Quaternion.Slerp(m_PlayerTr.rotation, currRot, Time.deltaTime * 3f);

            }
        }

        #region Audio
        if (survivor_audio.GetCheck())
            if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Walk_01"))
            {
                survivor_audio.PlayAudio("WALK");
            }
            else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Run_02"))
            {

                survivor_audio.PlayAudio("RUN");
            }
            else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidCrouchWalk 0"))
            {
                survivor_audio.PlayAudio("CROUCH_WALK");
            }
            else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidIdle"))
            {
                survivor_audio.PlayAudio("NOT");
            }

        /*
        if(survivor_heart_audio.GetCheck())
            survivor_heart_audio.PlayAudio("HEART_SPEED_UP", true);
        */
#endregion
            }

            #region
            void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(m_PlayerTr.position);
            stream.SendNext(m_PlayerTr.rotation);
            stream.SendNext((int)m_PlayerState);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            m_State = (int)stream.ReceiveNext();

        }
    }
    #endregion

    #region
    IEnumerator PlayerStateCheck()
    {

        while (true)
        {

            if (m_horizontal == 0 && m_vertical == 0 && m_rotate == 0)
            {

                if (m_IsCrouch)
                    m_PlayerState = PlayerState.Crouch;
                else
                {

                    m_PlayerState = PlayerState.Idle;
                }
            }

            else if (m_horizontal != 0 || m_vertical != 0 || m_rotate != 0)
            {
                if (m_IsCrouch)
                {
                    if (m_PlayerState == PlayerState.Crouch)
                        m_PlayerState = PlayerState.CrouchWalk;
                    else if (m_PlayerState != PlayerState.CrouchWalk)
                        m_PlayerState = PlayerState.Crouch;
                }

                else
                {
                    if (m_PlayerState == PlayerState.CrouchWalk)
                        m_PlayerState = PlayerState.Crouch;
                    else if (m_PlayerState == PlayerState.Crouch)
                        m_PlayerState = PlayerState.Idle;
                    else
                    {
                        m_PlayerState = PlayerState.Walk;
                        if (m_IsRun)
                            m_PlayerState = PlayerState.Run;
                    }
                }

            }

            if (isDie)
            {
                //Debug.Log("Death");
                m_PlayerState = PlayerState.Die;
               
            }

            if (m_PlayerState != PlayerState.Die && m_IsHit)
            {
                m_PlayerState = PlayerState.Hit;

            }
            if (m_Gram)
                m_PlayerState = PlayerState.Gram;
            if (m_Radio)
                m_PlayerState = PlayerState.Radio;
            if (m_Key)
                m_PlayerState = PlayerState.Key;
            yield return null;
        }
    }
    #endregion

    #region
    IEnumerator PlayerAction()
    {

        while (true)
        {

            switch (m_PlayerState)
            {
                case PlayerState.Idle:

                    AnimationExcute("Idle");

                    break;

                case PlayerState.Walk:
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");

                    break;

                case PlayerState.Run:
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");

                    break;

                case PlayerState.Crouch:
                    AnimationExcute("Crouch");

                    break;
                case PlayerState.CrouchWalk:
                    m_speed = m_WalkSpeed;
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

        if (!m_IsMoving && flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this);
            m_IsMoving = true;
        }

        else if (m_IsMoving && !flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_STOP, this);
            m_IsMoving = false;
        }
        if(!trap.Equals(""))
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this,trap);
        }
    }

    public bool getGramCtrl()
    {
        return m_Gram;
    }
    public void setGramCtrl(bool flag)
    {

        m_Gram = flag;
    }

    public bool getItemKey()
    {
        return m_ItemKey;
    }
    public void setItemKey(bool flag)
    {

        m_ItemKey = flag;
    }
    public bool getRadioCtrl()
    {
        return m_Radio;
    }
    public void setRadioCtrl(bool flag)
    {

        m_Radio = flag;
    }

    public bool getKeyCtrl()
    {
        return m_Key;
    }
    public void setKeyCtrl(bool flag)
    {
       
        m_Key = flag;
    }

    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {

            switch (m_State)
            {
                case (int)PlayerState.Idle:

                    sendMoveEvent(false,"");
                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    sendMoveEvent(false, "");
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");


                    break;

                case (int)PlayerState.Run:
                    sendMoveEvent(true, "");
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");

                    break;

                case (int)PlayerState.Crouch:
                    sendMoveEvent(false, "");
                    AnimationExcute("Crouch");

                    break;

                case (int)PlayerState.CrouchWalk:

                    sendMoveEvent(false, "");
                    m_speed = m_WalkSpeed;
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

                if (m_pv.isMine && !m_IsHit)
                {
                    if (m_PlayerState != PlayerState.Die)
                    {
                        StartCoroutine("Hit", Param);
                        m_pv.RPC("Hit", PhotonTargets.Others, Param);
                    }
                }

                break;

            case EVENT_TYPE.SURVIVOR_GRAMCTRL:

                if (m_pv.isMine)
                {
                    GramCtrl();
                    m_pv.RPC("GramCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_GRAM_SUC:

                if (m_pv.isMine)
                {
                    GramSuccess();
                    m_pv.RPC("GramSuccess", PhotonTargets.Others, null);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIOCTRL:

                if (m_pv.isMine)
                {
                    RadioCtrl(Param);
                    m_pv.RPC("RadioCtrl", PhotonTargets.Others, Param);
                }
                break;

            case EVENT_TYPE.SURVIVOR_RADIO_SUC:

                if (m_pv.isMine)
                {
                    
                    RadioSuccess();
                    m_pv.RPC("RadioSuccess", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL:

                if (m_pv.isMine)
                {
                    KeyCtrl();
                    m_pv.RPC("KeyCtrl", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.SURVIVOR_KEYCTRL2:

                if (m_pv.isMine)
                {
                    KeyCtrl2();
                    m_pv.RPC("KeyCtrl2", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET:

                if (m_pv.isMine)
                {
                    KeyGetItem();
                    m_pv.RPC("KeyGetItem", PhotonTargets.Others, null);
                }

                break;
            case EVENT_TYPE.KEY_GET2:

                if (m_pv.isMine)
                {
                    KeyGetItem2();
                    m_pv.RPC("KeyGetItem2", PhotonTargets.Others, null);
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
        if (m_PlayerState != PlayerState.Crouch && m_PlayerState != PlayerState.CrouchWalk)
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
        if (m_PlayerState != PlayerState.Crouch && m_PlayerState != PlayerState.CrouchWalk)
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
        if (m_PlayerState != PlayerState.Crouch && m_PlayerState != PlayerState.CrouchWalk)
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
        if (m_PlayerState != PlayerState.Crouch && m_PlayerState != PlayerState.CrouchWalk)
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
                m_Animator.SetBool("RadioCtrl", true); // 한 번씩 작동될때
                OnBtnRight_B(false);
            }
        }
    }

    [PunRPC]
    public IEnumerator Hit(object Param)
    {
        survivor_audio.PlayAudio("ATTACKED", true);
        int damage = (int)Param;
        m_HP -= damage;
        _life.fillAmount = _life.fillAmount - 0.25f;
        m_IsHit = true;

        StateHit();
        yield return new WaitForSeconds(0.7f);
        m_IsHit = false;

        if (m_HP <= 0)
        {
            survivor_audio.PlayAudio("DEATH", true);
            isDie = true;
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
        if(m_pv.isMine)
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
        if(flag)
        {
            GramGuage.gameObject.SetActive(flag);
            sw.Start();
            
        }
        else
        {
            GramGuage.gameObject.SetActive(flag);
            sw.Stop();
        }
    }
    public void AnimationExcute(string name)
    {

        m_Animator.SetBool("Idle", false);
        m_Animator.SetBool("Walk", false);
        m_Animator.SetBool("Run", false);
        m_Animator.SetBool("Crouch", false);
        m_Animator.SetBool("CrouchWalk", false);
        m_Animator.SetBool("Die", false);
        m_Animator.SetBool("Hit", false);
        m_Animator.SetBool("Gram", false);
        m_Animator.SetBool("Radio", false);
        m_Animator.SetBool("Key", false);
        m_Animator.SetBool(name, true);
    }
  
}
