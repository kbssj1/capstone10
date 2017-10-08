using UnityEngine;
using System.Collections;

public class Murderer : MonoBehaviour, IListener
{
    private const int MurderDamage = 25;
    private const float SpeedRotationInit = 50;
    private const float MurderSpeedInit = 1.5f;
	private const float MoveDirectionInit = 9.8;
    private enum PlayerState { Idle = 0, Walk = 1, Run = 2, Attack = 3, Die = 4 };

    [Header("Canvas Settings")]
    public Transform GameOver;
    public Transform CountDown;

    [Header("Animation Settings")]
    public Animator Animator;
    private PlayerState Playerstate;
    private int State;
    [Header("Controller Settings")]
    public CharacterController Controller;
    public Transform PlayerTr;
    public float SpeedRotation = SpeedRotationInit;
    public float WalkSpeed;
    public float RunSpeed;
    private float Speed = MurderSpeedInit;

    private Vector3 MoveDirection = Vector3.zero;
    private float Horizontal = 0f;
    private float Vertical = 0f;
    private float Rotate;

    [Header("Character Settings")]
    public bool Die;
    private bool Attack;
    private bool Run;
    private int Damage;

    [Header("Photon Settings")]
    public GameObject Camera;
    public PhotonView Pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    #region Audio

    public Murder_Audio murder_Audio;
    public Etc_Audio etcAudio;

    #endregion

    public Attack attack;

    private bool gameStart;
    [SerializeField]
    private SkinnedMeshRenderer SurSkin1;
    [SerializeField]
    private SkinnedMeshRenderer SurSkin2;
    [SerializeField]
    private MeshRenderer SurEyesR;
    [SerializeField]
    private MeshRenderer SurEyesL;
    [SerializeField]
    private MeshRenderer SurMouseLTeeth;
    [SerializeField]
    private MeshRenderer SurMouseUTeeth;
    [SerializeField]
    private MeshRenderer SurMouseTongue;

    // Use this for initialization
    void Awake()
    {

        Die = false;
        Attack = false;
        Run = false;
        Damage = MurderDamage;
        Playerstate = PlayerState.Idle;
        State = 0;
        Pv.synchronization = ViewSynchronization.UnreliableOnChange;

        Pv.ObservedComponents[0] = this;

        currPos = PlayerTr.position;
        currRot = PlayerTr.rotation;

        gameStart = false;

        etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();

    }

    void Start()
    {
        for(EVENT_TYPE i = EVENT_TYPE.SURVIVOR_MOVE;i< EVENT_TYPE.SURVIVOR_CREATE; i++)
        {
            EventManager.Instance.AddListener(i, this);
        }

        EventManager.Instance.PostNotification(EVENT_TYPE.COUNT_DOWN, this, true);
        EventManager.Instance.PostNotification(EVENT_TYPE.MURDERER_CREATE, this, true);

        if (Pv.isMine)
        {
            Camera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());
    }

    #region
    void Update()
    {
        if (!Die && !Attack && gameStart)
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

                MoveDirection.x = desiredMove.x * Speed;
                MoveDirection.y -= MoveDirectionInit;
                MoveDirection.z = desiredMove.z * Speed;

                Controller.Move(MoveDirection * Time.deltaTime);


                transform.Rotate(0, Rotate * Time.deltaTime, 0);
            }

            else
            {
                PlayerTr.position = Vector3.Lerp(PlayerTr.position, currPos, Time.deltaTime * 3f);
                PlayerTr.rotation = Quaternion.Slerp(PlayerTr.rotation, currRot, Time.deltaTime * 3f);

            }
        }
        PlayMudererAudio();
    }

    #endregion

    #region
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
    #endregion

    #region
    IEnumerator PlayerStateCheck()
    {
        while (true)
        {
            if (Horizontal == 0 && Vertical == 0 && Rotate == 0)
            {
                Playerstate = PlayerState.Idle;
            }
            else if (Horizontal != 0 || Vertical != 0 || Rotate != 0)
            {
                Playerstate = Run ? Playerstate = PlayerState.Run : PlayerState.Walk;
            }

            if (Input.GetButton("Fire1"))
            {
                Playerstate = PlayerState.Attack;
            }
            if (Die)
            {
                Playerstate = PlayerState.Die;
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
            switch (Playerstate)
            {
                case PlayerState.Idle:
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Idle");
                    break;
                case PlayerState.Walk:
                    Speed = WalkSpeed;
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Walk");
                    break;
                case PlayerState.Run:
                    Speed = RunSpeed;
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Run");
                    break;
                case PlayerState.Attack:
                    Attack = true;
                    AnimationExcute("Attack");
                    if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {                        
                        attack.attack_audio.PlayAudio(AudioController.AudioType.CHAINSSAW_ATTACK, true);
                        murder_Audio.PlayAudio(AudioController.AudioType.MURDER_ATTACK, true);
                    }
                    break;
                case PlayerState.Die:
                    AnimationExcute("Die");
                    murder_Audio.PlayAudio(AudioController.AudioType.MURDER_DEATH, true);
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
            switch (State)
            {
                case (int)PlayerState.Idle:
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Idle");
                    break;
                case (int)PlayerState.Walk:
                    Speed = WalkSpeed;
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Walk");
                    break;
                case (int)PlayerState.Run:
                    Speed = RunSpeed;
                    attack.PlayWeaponIdleAudio();
                    AnimationExcute("Run");
                    break;
                case (int)PlayerState.Attack:
                    Attack = true;
                    AnimationExcute("Attack");

                    if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
                    {
                        attack.attack_audio.PlayAudio(AudioController.AudioType.CHAINSSAW_ATTACK);
                        murder_Audio.PlayAudio(AudioController.AudioType.MURDER_ATTACK, true);
                    }
                    break;
                case (int)PlayerState.Die:

                    AnimationExcute("Die");                    
                    murder_Audio.PlayAudio(AudioController.AudioType.MURDER_DEATH, true);
                    
                    break;
            }
            yield return null;
        }

    }
    #endregion

    public void OnAttackEnd()
    {
        Attack = false;
    }

    public bool getAttacked()
    {
        return Attack;
    }

    public int getDamage()
    {
        return Damage;
    }

    public void setThroughSystem(bool flag, object Param)
    {
        if (flag)
        {
            SurSkin1.enabled = true;
            SurSkin2.enabled = true;
            SurEyesR.enabled = true;
            SurEyesL.enabled = true;
            SurMouseLTeeth.enabled = true;
            SurMouseTongue.enabled = true;
            SurMouseUTeeth.enabled = true;
        }
        else
        {
            SurSkin1.enabled = false;
            SurSkin2.enabled = false;
            SurEyesR.enabled = false;
            SurEyesL.enabled = false;
            SurMouseLTeeth.enabled = false;
            SurMouseTongue.enabled = false;
            SurMouseUTeeth.enabled = false;
        }
        Camera.GetComponent<SeeThroughSystem>().checkRenderTypes = flag;

        if (Param != null)
        {
            string trap = (string)Param;

            if (trap.Equals("Gram"))
            {
                Camera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("GRAM")) | (1 << LayerMask.NameToLayer("SURVIVOR"));

            }
            else if (trap.Equals("Radio"))
            {
                Camera.GetComponent<SeeThroughSystem>().TriggerLayers = (1 << LayerMask.NameToLayer("RADIO")) | (1 << LayerMask.NameToLayer("SURVIVOR"));
            }
            else
            {
                Camera.GetComponent<SeeThroughSystem>().TriggerLayers = 1 << LayerMask.NameToLayer("SURVIVOR");
            }

        }

    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_MOVE:
                if (Pv.isMine)
                {
                    setThroughSystem(true, Param);
                }
                break;

            case EVENT_TYPE.SURVIVOR_STOP:
                if (Pv.isMine)
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
                if (Pv.isMine)
                {
                    SurEyesR.enabled = false;
                    SurEyesL.enabled = false;
                    SurMouseLTeeth.enabled = false;
                    SurMouseTongue.enabled = false;
                    SurMouseUTeeth.enabled = false;
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

                if (Pv.isMine)
                {
                    SurSkin1 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Body").GetComponentInChildren<SkinnedMeshRenderer>();

                    SurSkin2 = GameObject.FindGameObjectWithTag("SURVIVOR").transform.FindChild("Survival_Chaaracter_02_Head").GetComponentInChildren<SkinnedMeshRenderer>();
                    SurEyesR = GameObject.Find("Survival_Chaaracter_02_Eyes_R").GetComponent<MeshRenderer>();
                    SurEyesL = GameObject.Find("Survival_Chaaracter_02_Eyes_L").GetComponent<MeshRenderer>();
                    SurMouseLTeeth = GameObject.Find("Survival_Character_02_Lower_Teeth").GetComponent<MeshRenderer>();
                    SurMouseTongue = GameObject.Find("Survival_Character_02_Tongue").GetComponent<MeshRenderer>();
                    SurMouseUTeeth = GameObject.Find("Survival_Character_Upper_Teeth").GetComponent<MeshRenderer>();

                    //Survival_Chaaracter_02_Eyes_R
                    //Survival_Character_Upper_Teeth
                }
                break;
        };
    }

    public void SurvivorWin()
    {
        gameStart = false;
        //transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("GameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        if (Pv.isMine)
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

        Animator.SetBool("Idle", false);
        Animator.SetBool("Walk", false);
        Animator.SetBool("Run", false);
        Animator.SetBool("Attack", false);
        Animator.SetBool("Die", false);
        Animator.SetBool(name, true);
    }

    private void PlayMudererAudio()
    {
        if (murder_Audio.GetCheck())
        {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.MURDER_WALK);
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.MURDER_RUN);
            }
            else if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.NOT);
            }
        }
    }
}