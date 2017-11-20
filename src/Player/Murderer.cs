using UnityEngine;
using System.Collections;

public class Murderer : MonoBehaviour, IListener
{
	private const int murderDamage = 25;
	private const float speedRotationInit = 50;
	private const float murderSpeedInit = 1.5f;
    private enum PlayerState { Idle = 0, Walk = 1, Run = 2, Attack = 3, die = 4 };

    [Header("Canvas Settings")]
    [SerializeField]
    private Transform GameOverUITansfrom;
    [SerializeField]
    private Transform CountDownUITransform;

    [Header("Animation Settings")]
    [SerializeField]
    private Animator ani;
    private PlayerState playerstate;
    private int state;
    [Header("Controller Settings")]
    [SerializeField]
    private CharacterController Controller;
    [SerializeField]
    private Transform playerTr;

    private float SpeedRotation = speedRotationInit;
    private float walkSpeed;
    private float runSpeed;
    private float Speed = murderSpeedInit;

    private Vector3 moveDirection = Vector3.zero;
    private float horizontal = 0f;
    private float vertical = 0f;
    private float rotate;

    [Header("Character Settings")]
    private bool die;
    private bool Attack;
    private bool Run;
    private int Damage;

    [Header("Photon Settings")]
    [SerializeField]
    private GameObject Camera;
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
	void Initialize(){
		die = false;
		Attack = false;
		Run = false;
		Damage = murderDamage;
		playerstate = PlayerState.Idle;
		state = 0;
		Pv.synchronization = ViewSynchronization.UnreliableOnChange;

		Pv.ObservedComponents[0] = this;

		currPos = playerTr.position;
		currRot = playerTr.rotation;

		gameStart = false;

		etcAudio = GameObject.FindGameObjectWithTag("AUDIO").GetComponent<Etc_Audio>();
	}
    void Awake()
    {
		Initialize ();
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
        if (!die && !Attack && gameStart)
        {
            if (Pv.isMine)
            {
                horizontal = Input.GetAxis("horizontal");
                vertical = Input.GetAxis("vertical");
                rotate = Input.GetAxis("Oculus_GearVR_RThumbstickX") * SpeedRotation; // 회전
                if (Input.GetButtonDown("Jump"))
                {
                    Run = true;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    Run = false;
                }

                Vector3 desiredMove = transform.forward * vertical + transform.right * horizontal;

                moveDirection.x = desiredMove.x * Speed;
                moveDirection.y -= 9.8f;
                moveDirection.z = desiredMove.z * Speed;

                Controller.Move(moveDirection * Time.deltaTime);


                transform.Rotate(0, rotate * Time.deltaTime, 0);
            }

            else
            {
                playerTr.position = Vector3.Lerp(playerTr.position, currPos, Time.deltaTime * 3f);
                playerTr.rotation = Quaternion.Slerp(playerTr.rotation, currRot, Time.deltaTime * 3f);

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
            stream.SendNext(playerTr.position);
            stream.SendNext(playerTr.rotation);
            stream.SendNext((int)playerstate);
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
                playerstate = PlayerState.Idle;
            }
            else if (horizontal != 0 || vertical != 0 || rotate != 0)
            {
                playerstate = Run ? playerstate = PlayerState.Run : PlayerState.Walk;
            }

            if (Input.GetButton("Fire1"))
            {
                playerstate = PlayerState.Attack;
            }
            if (die)
            {
                playerstate = PlayerState.die;
            }

            yield return null;
        }
    }

    #endregion
	string PlayerActionFunction(){
		string animationType = "";
		switch (playerstate)
		{
		case PlayerState.Idle:
			animationType = "Idle";
			attack.PlayWeaponIdleAudio();

			break;
		case PlayerState.Walk:
			animationType = "Walk";
			Speed = walkSpeed;
			attack.PlayWeaponIdleAudio();

			break;
		case PlayerState.Run:
			animationType = "Run";
			Speed = runSpeed;
			attack.PlayWeaponIdleAudio();

			break;
		case PlayerState.Attack:
			animationType = "Attack";
			Attack = true;

			if (!ani.GetCurrentAnimatorStateInfo(0).IsName("Attack(1)"))
			{                        
				attack.attack_audio.PlayAudio(AudioController.AudioType.CHAINSSAW_ATTACK, true);
				murder_Audio.PlayAudio(AudioController.AudioType.MURDER_ATTACK, true);
			}
			break;
		case PlayerState.die:
			animationType = "die";

			murder_Audio.PlayAudio(AudioController.AudioType.MURDER_DEATH, true);
			break;
		}
		return animationType;
	}
    #region
    IEnumerator PlayerAction()
    {
        while (true)
        {
			AnimationExcute(PlayerActionFunction());
            yield return null;
        }

    }
    #endregion

    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {
			AnimationExcute(PlayerActionFunction());
			yield return null;
        }

    }
    #endregion

    public void OnAttackEnd()
    {
        Attack = false;
    }

    public bool IsAttacked()
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
                OncountDown();
                break;
            case EVENT_TYPE.TIME_START:
                gameStart = true;
                if (Pv.isMine)
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

                if (Pv.isMine)
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
        //transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("gameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
        if (Pv.isMine)
			GameOverUITansfrom.GetComponentInChildren<Animator>().SetTrigger("LOSE");
    }

    public void SurvivorDead()
    {
        gameStart = false;
        // transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("gameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
		GameOverUITansfrom.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }

    public void OnTimerEnd()
    {
        gameStart = false;
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("gameOver").GetComponentInChildren<Animator>().SetTrigger("WIN");
		GameOverUITansfrom.GetComponentInChildren<Animator>().SetTrigger("WIN");
    }

    public void OnCountEnd()
    {
        EventManager.Instance.PostNotification(EVENT_TYPE.TIME_START, this);
    }

    public void OncountDown()
    {
        //  transform.FindChild("Canvas").transform.FindChild("Panel").transform.FindChild("countDown").GetComponentInChildren<Animator>().SetTrigger("countDown");
		CountDownUITransform.GetComponentInChildren<Animator>().SetTrigger("countDown");
        etcAudio.PlayAudio("countDown");
    }

    public void AnimationExcute(string name)
    {

        ani.SetBool("Idle", false);
        ani.SetBool("Walk", false);
        ani.SetBool("Run", false);
        ani.SetBool("Attack", false);
        ani.SetBool("die", false);
        ani.SetBool(name, true);
    }

    private void PlayMudererAudio()
    {
        if (murder_Audio.isAudioPlay())
        {
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.MURDER_WALK);
            }
            else if (ani.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.MURDER_RUN);
            }
            else if (ani.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                murder_Audio.PlayAudio(AudioController.AudioType.NOT);
            }
        }
    }
}