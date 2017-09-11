using UnityEngine;
using System.Collections;

public class Murderer : MonoBehaviour, IListener
{

    private enum PlayerState { Idle = 0, Walk = 1, Run = 2, Attack = 3, Die = 4 };

    [Header("Canvas Settings")]
    public Transform GameOver;
    public Transform CountDown;

    [Header("Animation Settings")]
    public Animator m_Animator;
    private PlayerState m_PlayerState;
    private int m_State;
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
    public bool isDie;
    private bool isAttack;
    private bool m_IsRun;
    private int m_Damage;


    [Header("Photon Settings")]
    public GameObject m_Camera;
    public PhotonView m_pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    #region Audio

    public Murder_Audio murder_Audio;
    public Etc_Audio etcAudio;

    #endregion

    public Attack attack;

    private bool gameStart;
    [SerializeField]
    private SkinnedMeshRenderer _surSkin1;
    [SerializeField]
    private SkinnedMeshRenderer _surSkin2;
    [SerializeField]
    private MeshRenderer _surEyesR;
    [SerializeField]
    private MeshRenderer _surEyesL;
    [SerializeField]
    private MeshRenderer _surMouseLTeeth;
    [SerializeField]
    private MeshRenderer _surMouseUTeeth;
    [SerializeField]
    private MeshRenderer _surMouseTongue;

    // Use this for initialization
    void Awake()
    {

        isDie = false;
        isAttack = false;
        m_IsRun = false;
        m_Damage = 25;
        m_PlayerState = PlayerState.Idle;
        m_State = 0;
        m_pv.synchronization = ViewSynchronization.UnreliableOnChange;

        m_pv.ObservedComponents[0] = this;

        currPos = m_PlayerTr.position;
        currRot = m_PlayerTr.rotation;

        gameStart = false;

        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();

    }


    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_MOVE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_STOP, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_WIN, this);
        EventManager.Instance.AddListener(EVENT_TYPE.COUNT_DOWN, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        print("MurCountDown");
        EventManager.Instance.PostNotification(EVENT_TYPE.COUNT_DOWN, this, true);
        EventManager.Instance.PostNotification(EVENT_TYPE.MURDERER_CREATE, this, true);
        if (m_pv.isMine)
        {
            m_Camera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());

        //StartCoroutine(TestAudio());  // For Death Test 

    }

    #region
    void Update()
    {

        if (!isDie && !isAttack && gameStart)
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
        if (murder_Audio.GetCheck())
        {
            if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                murder_Audio.PlayAudio("WALK");
            }
            else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            {
                murder_Audio.PlayAudio("RUN");
            }
            else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
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

                m_PlayerState = PlayerState.Idle;
            }

            else if (m_horizontal != 0 || m_vertical != 0 || m_rotate != 0)
            {
                m_PlayerState = PlayerState.Walk;
                if (m_IsRun)
                    m_PlayerState = PlayerState.Run;
            }

            if (Input.GetButton("Fire1"))
            {
                m_PlayerState = PlayerState.Attack;



            }
            if (isDie)
            {
                m_PlayerState = PlayerState.Die;
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

            switch (m_PlayerState)
            {
                case PlayerState.Idle:
                    attack.PlayNormalAudio();
                    AnimationExcute("Idle");

                    break;

                case PlayerState.Walk:
                    m_speed = m_WalkSpeed;

                    attack.PlayNormalAudio();
                    AnimationExcute("Walk");


                    break;

                case PlayerState.Run:
                    m_speed = m_RunSpeed;

                    attack.PlayNormalAudio();
                    AnimationExcute("Run");

                    break;

                case PlayerState.Attack:
                    isAttack = true;

                    AnimationExcute("Attack");

                    if (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {
                        if (attack.attack_audio.GetCheck())
                            attack.attack_audio.PlayAudio("ATTACK", true);
                        if (murder_Audio.GetCheck())
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
                    attack.PlayNormalAudio();
                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    m_speed = m_WalkSpeed;
                    attack.PlayNormalAudio();
                    AnimationExcute("Walk");

                    break;

                case (int)PlayerState.Run:
                    m_speed = m_RunSpeed;
                    attack.PlayNormalAudio();
                    AnimationExcute("Run");
                    break;

                case (int)PlayerState.Attack:
                    isAttack = true;
                    AnimationExcute("Attack");

                    if (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {
                        if (attack.attack_audio.GetCheck())
                            attack.attack_audio.PlayAudio("ATTACK", true);
                        if (murder_Audio.GetCheck())
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

        isAttack = false;
    }

    public bool getAttacked()
    {
        return isAttack;
    }

    public int getDamage()
    {
        return m_Damage;
    }

    public void setThroughSystem(bool flag, object Param)
    {
        if (flag)
        {
            _surSkin1.enabled = true;
            _surSkin2.enabled = true;
            _surEyesR.enabled = true;
            _surEyesL.enabled = true;
            _surMouseLTeeth.enabled = true;
            _surMouseTongue.enabled = true;
            _surMouseUTeeth.enabled = true;
        }
        else
        {
            _surSkin1.enabled = false;
            _surSkin2.enabled = false;
            _surEyesR.enabled = false;
            _surEyesL.enabled = false;
            _surMouseLTeeth.enabled = false;
            _surMouseTongue.enabled = false;
            _surMouseUTeeth.enabled = false;
        }
        m_Camera.GetComponent<SeeThroughSystem>().checkRenderTypes = flag;

        if (Param != null)
        {
            string trap = (string)Param;

            if (trap.Equals("Gram"))
            {
                m_Camera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("GRAM")) | (1 << LayerMask.NameToLayer("SURVIVOR"));

            }
            else if (trap.Equals("Radio"))
            {
                m_Camera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("RADIO")) | (1 << LayerMask.NameToLayer("SURVIVOR"));
            }
            else
            {
                m_Camera.GetComponent<SeeThroughSystem>().TriggerLayers = 1 << LayerMask.NameToLayer("SURVIVOR");
            }

        }

    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_MOVE:

                if (m_pv.isMine)
                {
                    setThroughSystem(true, Param);

                }

                break;

            case EVENT_TYPE.SURVIVOR_STOP:

                if (m_pv.isMine)
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
                if (m_pv.isMine)
                {
                    _surEyesR.enabled = false;
                    _surEyesL.enabled = false;
                    _surMouseLTeeth.enabled = false;
                    _surMouseTongue.enabled = false;
                    _surMouseUTeeth.enabled = false;
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

                if (m_pv.isMine)
                {
                    _surSkin1 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Body").GetComponentInChildren<SkinnedMeshRenderer>();

                    _surSkin2 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Head").GetComponentInChildren<SkinnedMeshRenderer>();
                    _surEyesR = GameObject.Find("Survival_Chaaracter_02_Eyes_R").GetComponent<MeshRenderer>();
                    _surEyesL = GameObject.Find("Survival_Chaaracter_02_Eyes_L").GetComponent<MeshRenderer>();
                    _surMouseLTeeth = GameObject.Find("Survival_Character_02_Lower_Teeth").GetComponent<MeshRenderer>();
                    _surMouseTongue = GameObject.Find("Survival_Character_02_Tongue").GetComponent<MeshRenderer>();
                    _surMouseUTeeth = GameObject.Find("Survival_Character_Upper_Teeth").GetComponent<MeshRenderer>();

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
        if (m_pv.isMine)
            GameOver.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }
    public void SurvivorDead()
    {
        gameStart = false;
        // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        GameOver.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }
    public void OnTimerEnd()
    {
        gameStart = false;
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        GameOver.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }
    public void OnCountEnd()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.TIME_START, this);
    }
    public void OnCountDown()
    {
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("CountDown").GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        CountDown.GetComponentInChildren<Animator>().SetTrigger("COUNTDOWN");
        etcAudio.PlayAudio("COUNTDOWN");
    }
    public void AnimationExcute(string name)
    {

        m_Animator.SetBool("Idle", false);
        m_Animator.SetBool("Walk", false);
        m_Animator.SetBool("Run", false);
        m_Animator.SetBool("Attack", false);
        m_Animator.SetBool("Die", false);
        m_Animator.SetBool(name, true);
    }

    public IEnumerator TestDeath()
    {


        yield return new WaitForSeconds(3.0f);

        isDie = true;
    }
}