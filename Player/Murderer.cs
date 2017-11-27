using UnityEngine;
using System.Collections;

public class Murderer : MonoBehaviour, IListener
{

    private enum PlayerState { Idle = 0, Walk = 1, Run = 2, Attack = 3, Die = 4 };

    [Header("Canvas Settings")]
    [SerializeField]
    private Transform gameOver;
    [SerializeField]
    private Transform countDown;
    [SerializeField]
    private Animator animator;
    private PlayerState playerState;
    private int m_State;
    private CharacterController characterController;
    private Transform playerTr;
    private const float speedRotation = 50;
    private const float walkSpeed = 1.5f;
    private const float runSpeed = 3.0f;
    private float speed = 1.5f;

    private Vector3 moveDirection = Vector3.zero;
    private float horizontal = 0f;
    private float vertical = 0f;
    private float rotate;

    private bool die;
    private bool attack;
    private bool run;
    private int damage;


    [Header("Photon Settings")]
    public GameObject characterCamera;
    public PhotonView pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    #region Audio

    public Murder_Audio murder_Audio;
    public Etc_Audio etcAudio;

    #endregion

    public Attack attackAudio;

    private bool gameStart;
    [SerializeField]
    private SkinnedMeshRenderer surSkin1;
    [SerializeField]
    private SkinnedMeshRenderer surSkin2;
    [SerializeField]
    private MeshRenderer surEyesR;
    [SerializeField]
    private MeshRenderer surEyesL;
    [SerializeField]
    private MeshRenderer surMouseLTeeth;
    [SerializeField]
    private MeshRenderer surMouseUTeeth;
    [SerializeField]
    private MeshRenderer surMouseTongue;

    // Use this for initialization

    void Initailize()
    {
        characterController = GetComponent<CharacterController>();
        playerTr = GetComponent<Transform>();

        die = false;
        attack = false;
        run = false;
        damage = 25;
        playerState = PlayerState.Idle;
        m_State = 0;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;

        pv.ObservedComponents[0] = this;

        currPos = playerTr.position;
        currRot = playerTr.rotation;

        gameStart = false;

        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();
    }

    void Awake()
    {
        Initailize();
    }


    void Start()
    {
        for (EVENT_TYPE i = EVENT_TYPE.SURVIVOR_MOVE; i <= EVENT_TYPE.SURVIVOR_CREATE; i++)
        {
            EventManager.Instance.AddListener(i, this);
        }

        print("MurCountDown");
        EventManager.Instance.PostNotification(EVENT_TYPE.COUNT_DOWN, this, true);
        EventManager.Instance.PostNotification(EVENT_TYPE.MURDERER_CREATE, this, true);
        if (pv.isMine)
        {
            characterCamera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());

        //StartCoroutine(TestAudio());  // For Death Test 

    }
    private bool isCharacterMove()
    {
        return !die && !attack && gameStart;
    }
    #region
    void Update()
    {

        if (isCharacterMove())
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

        #region Audio
        if (murder_Audio.isAudioPlay())
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                murder_Audio.PlayAudio("WALK");
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            {
                murder_Audio.PlayAudio("RUN");
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                murder_Audio.PlayAudio("NOT");
            }
        }
        #endregion
    }

    #endregion

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
            m_State = (int)stream.ReceiveNext();

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

                playerState = PlayerState.Idle;
            }

            else if (horizontal != 0 || vertical != 0 || rotate != 0)
            {

                playerState = run ? PlayerState.Run : PlayerState.Walk;
            }
            
            if (Input.GetButton("Fire1"))
            {
                playerState = PlayerState.Attack;



            }
            if (die)
            {
                playerState = PlayerState.Die;
            }

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
                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Idle");

                    break;

                case PlayerState.Walk:
                    
                    speed = walkSpeed;

                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Walk");

                    break;

                case PlayerState.Run:
                    speed = runSpeed;
                    
                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Run");
                    break;

                case PlayerState.Attack:
                    attack = true;

                    AnimationExcute("Attack");

                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {
                        if (attackAudio.attack_audio.isAudioPlay())
                            attackAudio.attack_audio.PlayAudio("ATTACK", true);
                        if (murder_Audio.isAudioPlay())
                            murder_Audio.PlayAudio("ATTACK", true);
                    }
                    break;
                case PlayerState.Die:

                    AnimationExcute("Die");

                    if (murder_Audio.RepeatCheck_Death == true)
                    {
                        murder_Audio.PlayAudio("DEATH", true);
                    }
                    break;
            }
            yield return null;
        }

    }
    #endregion

    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {

            switch (m_State)
            {
                case (int)PlayerState.Idle:
                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    speed = walkSpeed;
                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Walk");

                    break;

                case (int)PlayerState.Run:
                    speed = runSpeed;
                    attackAudio.PlayIdleWeaponAudio();
                    AnimationExcute("Run");
                    break;

                case (int)PlayerState.Attack:
                    attack = true;
                    AnimationExcute("Attack");

                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {
                        if (attackAudio.attack_audio.isAudioPlay())
                            attackAudio.attack_audio.PlayAudio("ATTACK", true);
                        if (murder_Audio.isAudioPlay())
                            murder_Audio.PlayAudio("ATTACK", true);
                    }
                    break;
                case (int)PlayerState.Die:

                    AnimationExcute("Die");
                    if (murder_Audio.RepeatCheck_Death == true)
                    {
                        murder_Audio.PlayAudio("DEATH", true);
                    }
                    break;
            }

            //murder_Audio.PlayCurrentAudio();

            yield return null;
        }

    }
    #endregion

    public void OnAttackEnd()
    {

        attack = false;
    }

    public bool getAttacked()
    {
        return attack;
    }

    public int getDamage()
    {
        return damage;
    }

    public void setThroughSystem(bool flag, object Param)
    {
        if (flag)
        {
            surSkin1.enabled = true;
            surSkin2.enabled = true;
            surEyesR.enabled = true;
            surEyesL.enabled = true;
            surMouseLTeeth.enabled = true;
            surMouseTongue.enabled = true;
            surMouseUTeeth.enabled = true;
        }
        else
        {
            surSkin1.enabled = false;
            surSkin2.enabled = false;
            surEyesR.enabled = false;
            surEyesL.enabled = false;
            surMouseLTeeth.enabled = false;
            surMouseTongue.enabled = false;
            surMouseUTeeth.enabled = false;
        }
        characterCamera.GetComponent<SeeThroughSystem>().checkRenderTypes = flag;

        if (Param != null)
        {
            string trap = (string)Param;

            if (trap.Equals("Gram"))
            {
                characterCamera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("GRAM")) | (1 << LayerMask.NameToLayer("SURVIVOR"));

            }
            else if (trap.Equals("Radio"))
            {
                characterCamera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("RADIO")) | (1 << LayerMask.NameToLayer("SURVIVOR"));
            }
            else
            {
                characterCamera.GetComponent<SeeThroughSystem>().TriggerLayers = 1 << LayerMask.NameToLayer("SURVIVOR");
            }

        }

    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_MOVE:

                if (pv.isMine)
                {
                    setThroughSystem(true, Param);

                }

                break;

            case EVENT_TYPE.SURVIVOR_STOP:

                if (pv.isMine)
                {

                    setThroughSystem(false, Param);
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
                if (pv.isMine)
                {
                    surEyesR.enabled = false;
                    surEyesL.enabled = false;
                    surMouseLTeeth.enabled = false;
                    surMouseTongue.enabled = false;
                    surMouseUTeeth.enabled = false;
                }
                break;
            case EVENT_TYPE.SURVIVOR_DIE:
                SurvivorDead();
                break;
            case EVENT_TYPE.SURVIVOR_WIN:
                SurvivorWin();
                break;
            case EVENT_TYPE.SURVIVOR_CREATE:
                //

                if (pv.isMine)
                {
                    surSkin1 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Body").GetComponentInChildren<SkinnedMeshRenderer>();

                    surSkin2 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Head").GetComponentInChildren<SkinnedMeshRenderer>();
                    surEyesR = GameObject.Find("Survival_Chaaracter_02_Eyes_R").GetComponent<MeshRenderer>();
                    surEyesL = GameObject.Find("Survival_Chaaracter_02_Eyes_L").GetComponent<MeshRenderer>();
                    surMouseLTeeth = GameObject.Find("Survival_Character_02_Lower_Teeth").GetComponent<MeshRenderer>();
                    surMouseTongue = GameObject.Find("Survival_Character_02_Tongue").GetComponent<MeshRenderer>();
                    surMouseUTeeth = GameObject.Find("Survival_Character_Upper_Teeth").GetComponent<MeshRenderer>();

                    //Survival_Chaaracter_02_Eyes_R
                    //Survival_Character_Upper_Teeth
                }
                break;
        };
    }


    public void SurvivorWin()
    {
        gameStart = false;
        // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        if (pv.isMine)
            gameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void SurvivorDead()
    {
        gameStart = false;
        // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        gameOver.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }
    public void OnTimerEnd()
    {
        gameStart = false;
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        gameOver.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }
    public void OnCountEnd()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.TIME_START, this);
    }
    public void OnCountDown()
    {
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("CountDown").GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        countDown.GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        etcAudio.PlayAudio("COUNTDOWN");
    }
    public void AnimationExcute(string name)
    {

        animator.SetBool("Idle", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Die", false);
        animator.SetBool(name, true);
    }

    public IEnumerator TestDeath()
    {


        yield return new WaitForSeconds(3.0f);

        die = true;
    }
}