using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;

public class Survivor : MonoBehaviour, IListener {

    public enum PlayerState { Idle = 0, Walk = 1, Run = 2, Crouch = 3, CrouchWalk = 4, Hit = 5, Die = 6, Gram = 7, Radio=8, Key=9 };

    private const int survivorInitHP = 100;
    private const float gaugeAdd = 19f;
    private const float survivorInitSpeed = 1.5f;
    private const float hitBreakTime = 0.7f;
    private const float rotationInitSpeed = 50;

    [Header("Canvas Settings")]
    public Transform countDown;
    public Transform gameOver;
    public Transform joyRightBtn;
    public Transform joyLeftBtn;
    public Transform gramGuage;
    public Image life;
    public Image guage;
    private float time = gaugeAdd;

    [Header("Animation Settings")]
    public Animator animator;
    [SerializeField]
    public PlayerState playerState;
    private int state;
    public GameObject[] head;

    [Header("Controller Settings")]
    public CharacterController characterController;
    public Transform playerTr;
    private float speedRotation = rotationInitSpeed;
    private float walkSpeed = survivorInitSpeed;
    private float runSpeed = 3.0f;
    private float speed = 1.5f;

    private Vector3 moveDirection = Vector3.zero;
    private float horizontal = 0f;
    private float vertical = 0f;
    private float rotate;

    [Header("Character Settings")]
    private bool die;
    private bool run;
    private bool crouch;
    private bool hit;
    private bool gram;
    private bool radio;
    private bool key;
    private int hp;
    private bool itemKey;
    private bool moving;
    private bool gameStart;

    [Header("Photon Settings")]
    public GameObject mainCamera;
    public PhotonView pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    #region Audio
    [Header("Audio Settings")]
    public Survivor_Audio survivor_audio;
    public Survivor_Audio2 survivor_heart_audio;
    public Etc_Audio etcAudio;
    public AudioManager audioManager;

    Stopwatch sw;
    #endregion
    // Use this for initialization

    void Initailize()
    {
        itemKey = false;
        die = false;
        run = false;
        crouch = false;
        hit = false;
        moving = false;
        gram = false;
        radio = false;
        key = false;

        hp = survivorInitHP;

        playerState = PlayerState.Idle;
        state = 0;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;

        pv.ObservedComponents[0] = this;

        currPos = playerTr.position;
        currRot = playerTr.rotation;

        gameStart = false;
        sw = new Stopwatch();
        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();
    }

    void Awake()
    {
        Initailize();
    }

    void Start()
    {
        for (EVENT_TYPE i = EVENT_TYPE.SURVIVOR_HIT; i <= EVENT_TYPE.B_RIGHT_BTN_IMPOSSIBLE; i++)
        {
            EventManager.Instance.AddListener(i, this);
        }
        for (EVENT_TYPE i = EVENT_TYPE.TIME_OVER; i <= EVENT_TYPE.SURVIVOR_DIE; i++)
        {
            EventManager.Instance.AddListener(i, this);
        }
        

        if (pv.isMine)
        {
            head[0].GetComponent<SkinnedMeshRenderer>().enabled = false;
            head[1].SetActive(false);
            mainCamera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());

        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_CREATE, this);

        audioManager = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<AudioManager>();
    }

    private bool IsCanMoving()
    {
        return !die && !hit && !gram && gameStart && !radio && !key;
    }

    void Update()
    {

        guage.fillAmount = (sw.ElapsedMilliseconds / 1000) / time;

        //Debug.Log(HeartBeat);
        if (IsCanMoving())
        {
            if (pv.isMine)
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
                rotate = Input.GetAxis("Oculus_GearVR_RThumbstickX") * speedRotation; // 회전

                if (Input.GetButtonDown("Jump"))
                {
                    run = true;
                }

                else if (Input.GetButtonUp("Jump"))
                {
                    run = false;
                }
                /*
                if (Input.GetButtonDown("Fire1"))
                {
                    m_IsCrouch = !m_IsCrouch;
                    survivor_audio.PlayAudio("CROUCH", true);
                }
                */

                Vector3 desiredMove = transform.forward * vertical + transform.right * horizontal;


                moveDirection.x = desiredMove.x * speed;
                moveDirection.y -= 9.8f;
                moveDirection.z = desiredMove.z * speed;

                characterController.Move(moveDirection * Time.deltaTime);


                transform.Rotate(0, rotate * Time.deltaTime, 0);
            }

            else
            {
                playerTr.position = Vector3.Lerp(playerTr.position, currPos, Time.deltaTime * 3f);
                playerTr.rotation = Quaternion.Slerp(playerTr.rotation, currRot, Time.deltaTime * 3f);

            }
        }
        PlayAudioSurvivor();

    }
    private void PlayAudioSurvivor()
    {
        if (survivor_audio.isAudioPlay())
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Walk_01"))
            {
                survivor_audio.PlayAudio("WALK");
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Run_02"))
            {
                survivor_audio.PlayAudio("RUN");
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidCrouchWalk 0"))
            {
                survivor_audio.PlayAudio("CROUCH_WALK");
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidIdle"))
            {
                survivor_audio.PlayAudio("NOT");
            }
    }
    #region
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(playerTr.position);
            stream.SendNext(playerTr.rotation);
            stream.SendNext((int)playerState);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            state = (int)stream.ReceiveNext();

        }
    }
    #endregion

    #region
    IEnumerator PlayerStateCheck()
    {

        while (true)
        {

            if (horizontal == 0 && vertical == 0 && rotate == 0)
            {
                playerState = crouch ? PlayerState.Crouch : PlayerState.Idle;
            }

            else if (horizontal != 0 || vertical != 0 || rotate != 0)
            {
                if (crouch)
                {
                    if (playerState == PlayerState.Crouch)
                        playerState = PlayerState.CrouchWalk;
                    else if (playerState != PlayerState.CrouchWalk)
                        playerState = PlayerState.Crouch;
                }

                else
                {
                    if (playerState == PlayerState.CrouchWalk)
                        playerState = PlayerState.Crouch;
                    else if (playerState == PlayerState.Crouch)
                        playerState = PlayerState.Idle;
                    else
                    {
                        playerState = run ? PlayerState.Run : PlayerState.Walk;
                    }
                }

            }

            if (die)
            {
                //Debug.Log("Death");
                playerState = PlayerState.Die;
               
            }

            if (playerState != PlayerState.Die && hit)
            {
                playerState = PlayerState.Hit;

            }
            if (gram)
                playerState = PlayerState.Gram;
            if (radio)
                playerState = PlayerState.Radio;
            if (key)
                playerState = PlayerState.Key;
            yield return null;
        }
    }
    #endregion

    #region
    IEnumerator PlayerAction()
    {

        while (true)
        {

            switch (playerState)
            {
                case PlayerState.Idle:

                    AnimationExcute("Idle");

                    break;

                case PlayerState.Walk:
                    speed = walkSpeed;
                    AnimationExcute("Walk");

                    break;

                case PlayerState.Run:
                    speed = runSpeed;
                    AnimationExcute("Run");

                    break;

                case PlayerState.Crouch:
                    AnimationExcute("Crouch");

                    break;
                case PlayerState.CrouchWalk:
                    speed = walkSpeed;
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

        if (!moving && flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this);
            moving = true;
        }

        else if (moving && !flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_STOP, this);
            moving = false;
        }
        if(!trap.Equals(""))
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this,trap);
        }
    }

    public bool getGramCtrl()
    {
        return gram;
    }
    public void setGramCtrl(bool flag)
    {

        gram = flag;
    }

    public bool getItemKey()
    {
        return itemKey;
    }
    public void setItemKey(bool flag)
    {

        itemKey = flag;
    }
    public bool getRadioCtrl()
    {
        return radio;
    }
    public void setRadioCtrl(bool flag)
    {

        radio = flag;
    }

    public bool getKeyCtrl()
    {
        return key;
    }
    public void setKeyCtrl(bool flag)
    {

        key = flag;
    }

    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {

            switch (state)
            {
                case (int)PlayerState.Idle:

                    sendMoveEvent(false,"");
                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    sendMoveEvent(false, "");
                    speed = walkSpeed;
                    AnimationExcute("Walk");


                    break;

                case (int)PlayerState.Run:
                    sendMoveEvent(true, "");
                    speed = runSpeed;
                    AnimationExcute("Run");

                    break;

                case (int)PlayerState.Crouch:
                    sendMoveEvent(false, "");
                    AnimationExcute("Crouch");

                    break;

                case (int)PlayerState.CrouchWalk:

                    sendMoveEvent(false, "");
                    speed = walkSpeed;
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

                if (pv.isMine && playerState != PlayerState.Die)
                {                   
                    StartCoroutine("Hit", Param);
                    pv.RPC("Hit", PhotonTargets.Others, Param);                  
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
        if (playerState != PlayerState.Crouch && playerState != PlayerState.CrouchWalk)
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
        if (playerState != PlayerState.Crouch && playerState != PlayerState.CrouchWalk)
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
        if (playerState != PlayerState.Crouch && playerState != PlayerState.CrouchWalk)
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
        if (playerState != PlayerState.Crouch && playerState != PlayerState.CrouchWalk)
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
                animator.SetBool("RadioCtrl", true); // 한 번씩 작동될때
                OnBtnRight_B(false);
            }
        }
    }

    [PunRPC]
    public IEnumerator Hit(object Param)
    {
        survivor_audio.PlayAudio("ATTACKED", true);
        int damage = (int)Param;
        hp -= damage;
		life.fillAmount = life.fillAmount - ((float)damage/100f);
        hit = true;
		UnityEngine.Debug.Log (hp);
        StateHit();
        yield return new WaitForSeconds(hitBreakTime);
        hit = false;

        if (hp <= 0)
        {
            survivor_audio.PlayAudio("DEATH", true);
            die = true;
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
        gameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void SurvivorWin()
    {
        gameStart = false;
		if (pv.isMine) {
			gameOver.GetComponentInChildren<Animator> ().SetTrigger ("WIN");
			PlayerPrefs.SetInt (LevelManager.LevelKeyword, LevelManager.CurrentLevel + 1);
		}
		
    }
    public void OnTimerEnd()
    {
        gameStart = false;
        //transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("LOSE");
        gameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void OnCountDown()
    {
       // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("CountDown").GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        countDown.GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
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
        joyRightBtn.GetComponentInChildren<Animator>().SetBool("Btn_B", flag);
    }
    
    public void OnBtnLeft_R(bool flag)
    {
        joyLeftBtn.GetComponentInChildren<Animator>().SetBool("Btn_L", flag);
    }
    public void OnGramGuage(bool flag)
    {
        if(flag)
        {
            gramGuage.gameObject.SetActive(flag);
            sw.Start();
            
        }
        else
        {
            gramGuage.gameObject.SetActive(flag);
            sw.Stop();
        }
    }
    public void AnimationExcute(string name)
    {

        animator.SetBool("Idle", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Crouch", false);
        animator.SetBool("CrouchWalk", false);
        animator.SetBool("Die", false);
        animator.SetBool("Hit", false);
        animator.SetBool("Gram", false);
        animator.SetBool("Radio", false);
        animator.SetBool("Key", false);
        animator.SetBool(name, true);
    }
  
}
